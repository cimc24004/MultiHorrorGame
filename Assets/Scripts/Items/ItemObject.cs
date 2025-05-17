using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(NetworkObject))]
public class ItemObject : NetworkBehaviour
{
    public ItemStatus item_status;
    public UnityEvent ItemGetAddEvent;

    private NeedKeysController need_keys_controller;
    private NetworkObject      netObj;

    /*──────── Awake : 余計なクライアントオブジェクト破棄＋サーバー Spawn ───────*/
    void Awake()
    {
        netObj = GetComponent<NetworkObject>();

        if (NetworkManager.Singleton == null) return;

        if (NetworkManager.Singleton.IsServer)
        {
            if (!netObj.IsSpawned) netObj.Spawn();
        }
        else
        {
            Destroy(gameObject);          // クライアント側のダミーを即削除
        }
    }

    public override void OnNetworkSpawn()
    {
        need_keys_controller = NeedKeysController.instance;
    }

    /*──────── クライアント入口 ───────*/
    public void RequestItemGet()
    {
        RequestItemGetServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    /*──────── Client → Server ───────*/
    [ServerRpc(RequireOwnership = false)]
    void RequestItemGetServerRpc(ulong clientId, ServerRpcParams _ = default)
    {
        var playerObj = NetworkManager.Singleton.SpawnManager
                              .GetPlayerNetworkObject(clientId).gameObject;

        var inv = playerObj.GetComponent<PlayerStatus>()
                           .ItemController
                           .GetComponent<ItemBelongingsController>();

        /* ---------- 上限チェック修正版 ---------- */
        int  uniqueCount  = inv.belonging_items
                               .Where(kvp => kvp.Key.item_type != ItemStatus.ItemType.Event)
                               .Count();
        bool alreadyHave  = inv.belonging_items.ContainsKey(item_status);
        bool slotLimit    = !alreadyHave && uniqueCount >= inv.maxCarryableItems;
        bool reachedMax   = slotLimit || IsItemAtMaxCapacity(inv);
        /* ---------------------------------------- */

        bool success = !reachedMax ||
                       item_status.item_type == ItemStatus.ItemType.Event;

        int newCount = 0;
        if (success)
        {
            if (item_status.item_type == ItemStatus.ItemType.Key)
                need_keys_controller.GottenKeyCount++;

            inv.ItemAddToBelonging(item_status);
            newCount = inv.belonging_items[item_status].item_count;
        }

        // 取得結果を呼び出し主のクライアントへ
        var cp = new ClientRpcParams {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
        };
        ItemGetResponseClientRpc(success, newCount, cp);

        // 成功したら Despawn
        if (success) netObj.Despawn(true);
    }

    /*──────── Server → Client（拾い主）────────*/
    [ClientRpc]
    void ItemGetResponseClientRpc(bool success, int newCount, ClientRpcParams _ = default)
    {
        var invUI = PlayerStatus.LocalInstance
                      .ItemController
                      .GetComponent<ItemBelongingsController>();

        if (success)
        {
            invUI.ForceSyncFromServer(item_status, newCount);
            ItemGetAddEvent.Invoke();      // 効果音
        }

        invUI.item_menu_controller.OnItemGetChangeUI(!success, item_status);
    }

    /*──────── 同アイテム上限チェック────────*/
    bool IsItemAtMaxCapacity(ItemBelongingsController inv)
    {
        if (!inv.belonging_items.TryGetValue(item_status, out var entry))
            return false;
        return entry.item_count + 1 > item_status.MaxCanHave;
    }
}
