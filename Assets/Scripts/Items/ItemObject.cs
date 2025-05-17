using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class ItemObject : NetworkBehaviour
{
    private NeedKeysController need_keys_controller;
    public ItemStatus    item_status;
    public UnityEvent    ItemGetAddEvent;       // Inspector で「AudioSource→Play()」を登録
    private NetworkObject ItemNetworkObject;

    public override void OnNetworkSpawn()
    {
        need_keys_controller = NeedKeysController.instance;
        ItemNetworkObject   = GetComponent<NetworkObject>();
    }

    /// <summary>
    /// クライアント側から「このアイテム拾いたい！」を呼ぶ
    /// </summary>
    public void RequestItemGet()
    {
        RequestItemGetServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    //────────────────────────────────────────────────────
    // (1) ServerRpc：クライアント→サーバー
    //────────────────────────────────────────────────────
    [ServerRpc(RequireOwnership = false)]
    private void RequestItemGetServerRpc(
        ulong clientId,
        ServerRpcParams rpcParams = default
    )
    {
        // (A) 拾ったプレイヤーのインベントリ & 上限チェック
        var playerObj = NetworkManager.Singleton.SpawnManager
                              .GetPlayerNetworkObject(clientId)
                              .gameObject;
        var inv = playerObj
                    .GetComponent<PlayerStatus>()
                    .ItemController
                    .GetComponent<ItemBelongingsController>();

        bool reachedMax = inv.belonging_items
            .Where(kvp => kvp.Key.item_type != ItemStatus.ItemType.Event)
            .Count() >= inv.maxCarryableItems
         || IsItemAtMaxCapacity(inv);

        bool success = !reachedMax
                    || item_status.item_type == ItemStatus.ItemType.Event;

        // (B) サーバー側インベントリ更新
        if (success)
        {
            if (item_status.item_type == ItemStatus.ItemType.Key)
                need_keys_controller.GottenKeyCount++;

            inv.ItemAddToBelonging(item_status);
        }

        // (C) ClientRpc で呼び出し元だけ結果通知
        var rpcParamsClient = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { clientId }
            }
        };
        ItemGetResponseClientRpc(success, rpcParamsClient);

        // (D) 成功時のみ Despawn（全クライアントから消す）
        if (success)
        {
            ItemNetworkObject.Despawn(true);
        }
    }

    //────────────────────────────────────────────────────
    // (2) ClientRpc：サーバー→呼び出し元クライアント
    //            ここで UnityEvent を発火 ⇒ 音再生＆UI
    //────────────────────────────────────────────────────
    [ClientRpc]
    private void ItemGetResponseClientRpc(
        bool success,
        ClientRpcParams clientRpcParams = default
    )
    {
        // 呼ばれたクライアント以外は無視
        if (NetworkManager.Singleton.LocalClientId
            != clientRpcParams.Send.TargetClientIds[0]) return;

        // (E) ローカルのインベントリ & UI 更新
        var inv = PlayerStatus.LocalInstance
                    .ItemController
                    .GetComponent<ItemBelongingsController>();

        if (success)
        {
            inv.ItemAddToBelonging(item_status);
            // UnityEvent に登録した音再生やエフェクトを一括実行
            ItemGetAddEvent.Invoke();
        }

        // 「持てなかった」通知は !success
        inv.item_menu_controller
           .OnItemGetChangeUI(!success, item_status);
    }

    private bool IsItemAtMaxCapacity(ItemBelongingsController c)
    {
        if (!c.belonging_items.TryGetValue(item_status, out var e))
            return false;
        return e.item_count + 1 > item_status.MaxCanHave;
    }
}
