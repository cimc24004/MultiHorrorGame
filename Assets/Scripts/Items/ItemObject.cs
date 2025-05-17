using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class ItemObject : NetworkBehaviour
{
    private NeedKeysController need_keys_controller;
    public ItemStatus item_status;
    public UnityEvent  ItemGetAddEvent;
    private NetworkObject ItemNetworkObject;
    private ItemBelongingsController belongings;
    private bool successItemGet;
    public override void OnNetworkSpawn()
    {
        need_keys_controller = NeedKeysController.instance;
        ItemNetworkObject = GetComponent<NetworkObject>();
    }

    //───────────────────────────────────────────────────
    // ① クライアント側から「拾いたい！」を呼ぶ（MonoBehaviour から）
    //───────────────────────────────────────────────────
    public bool RequestItemGet()
    {
        // LocalClientId は Netcode が割り当てる自分の ID
        RequestItemGetServerRpc(
            //  ScriptableObject なら固有IDを用意しておく
            NetworkManager.Singleton.LocalClientId
        );
        return successItemGet;
    }

    //───────────────────────────────────────────────────
    // ② ServerRpc：クライアント→サーバー（RequireOwnership=false で所有権不要）
    //───────────────────────────────────────────────────
    [ServerRpc(RequireOwnership = false)]
    void RequestItemGetServerRpc(
        ulong clientId,
        ServerRpcParams rpcParams = default
    )
    {
        // (A) サーバーでアイテムを探してチェック
        var playerObj = NetworkManager.Singleton.SpawnManager
                            .GetPlayerNetworkObject(clientId)
                            .gameObject;
        belongings = playerObj
                            .GetComponent<PlayerStatus>()
                            .ItemController
                            .GetComponent<ItemBelongingsController>();

        bool reachedMax = belongings.belonging_items
            .Where(kvp => kvp.Key.item_type != ItemStatus.ItemType.Event)
            .Count() >= belongings.maxCarryableItems
            || IsItemAtMaxCapacity(belongings);

        bool success = !reachedMax || item_status.item_type == ItemStatus.ItemType.Event;
        successItemGet = success;
        if (success)
        {
            if (item_status.item_type == ItemStatus.ItemType.Key)
                need_keys_controller.GottenKeyCount++;

            belongings.ItemAddToBelonging(item_status);      // サーバー上のインベントリ更新
            ItemNetworkObject.Despawn(true);                // 全クライアントから削除
        }
        // (B) 呼び出し元クライアントにだけ結果を返す
        var clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };
        ItemGetResponseClientRpc(success, clientRpcParams);
    }

    //───────────────────────────────────────────────────
    // ③ ClientRpc：サーバー→特定クライアント（結果返却）
    //───────────────────────────────────────────────────
    [ClientRpc]
    void ItemGetResponseClientRpc(
        bool success,
        ClientRpcParams clientRpcParams = default
    )
    {
        // (C) 成功ならインベントリUI更新＆イベント
        belongings.ItemAddToBelonging(item_status);
        ItemGetAddEvent.Invoke();
        belongings.item_menu_controller
            .OnItemGetChangeUI(!success, item_status);
    }

    bool IsItemAtMaxCapacity(ItemBelongingsController c)
    {
        if (!c.belonging_items.TryGetValue(item_status, out var e))
            return false;
        return e.item_count + 1 > item_status.MaxCanHave;
    }
}
