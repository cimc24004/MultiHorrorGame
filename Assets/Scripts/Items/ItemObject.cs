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

    void Awake()
    {
        netObj = GetComponent<NetworkObject>();
        if (NetworkManager.Singleton == null) return;
    }

    public override void OnNetworkSpawn()
    {
        need_keys_controller = NeedKeysController.instance;
    }
    public override void OnNetworkDespawn()
    {
        Debug.Log("アイテムDespawn");
        this.gameObject.SetActive(false);

    }
    // これが無いと CrossHairEvent から呼べない
    public void RequestItemGet()
    {
        // ローカル→サーバーへ拾いリクエスト飛ばすだけ
        if (!IsSpawned) return;                     // NetObj がまだ Spawn してなかったら無視
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

        /* ---------- 上限チェック ---------- */
        int  uniqueCount = inv.belonging_items
                               .Where(kvp => kvp.Key.item_type != ItemStatus.ItemType.Event)
                               .Count();
        bool alreadyHave = inv.belonging_items.ContainsKey(item_status);
        bool slotLimit   = !alreadyHave && uniqueCount >= inv.maxCarryableItems;
        bool reachedMax  = slotLimit || IsItemAtMaxCapacity(inv);
        /* ---------------------------------- */

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

        var cp = new ClientRpcParams {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
        };
        ItemGetResponseClientRpc(success, newCount, cp);

        if (success)
        {
            Debug.Log("アイテム取得成功---------------------------");
          netObj.Despawn(true);          // 全員から削除  
        } 
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
            invUI.ForceSyncFromServer(item_status, newCount);  // ★ホストでも呼ぶ
            ItemGetAddEvent.Invoke();                          // 効果音など
        }

        invUI.item_menu_controller.OnItemGetChangeUI(!success, item_status);
    }

    bool IsItemAtMaxCapacity(ItemBelongingsController inv)
    {
        if (!inv.belonging_items.TryGetValue(item_status, out var entry))
            return false;
        return entry.item_count + 1 > item_status.MaxCanHave;
    }
}
