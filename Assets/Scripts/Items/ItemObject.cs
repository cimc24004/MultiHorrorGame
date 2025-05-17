using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class ItemObject : NetworkBehaviour
{
    // ───────── Inspector 設定項目 ─────────
    [Header("このワールドアイテムのステータス")]
    public ItemStatus item_status;                 // ScriptableObject などで保持した情報
    [Header("取得成功時に呼ばれる UnityEvent (効果音などを紐づける)")]
    public UnityEvent ItemGetAddEvent;             // AudioSource → Play() を登録しておく

    // ───────── 参照キャッシュ ─────────
    private NeedKeysController need_keys_controller;
    private NetworkObject      thisNetObj;         // 自分の NetworkObject

    // ────────────────────────────────────────
    // ネットワーク初期化
    // ────────────────────────────────────────
    public override void OnNetworkSpawn()
    {
        need_keys_controller = NeedKeysController.instance;
        thisNetObj           = GetComponent<NetworkObject>();
    }

    // ────────────────────────────────────────
    // クライアント側エントリポイント
    // ワールド上のアイテムをクリックしたスクリプトから呼ばれる
    // ────────────────────────────────────────
    public void RequestItemGet()
    {
        RequestItemGetServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    // ────────────────────────────────────────
    // (1) Client → Server
    // プレイヤーが拾いたいと要求
    // ────────────────────────────────────────
    [ServerRpc(RequireOwnership = false)]
    private void RequestItemGetServerRpc(ulong clientId, ServerRpcParams _ = default)
    {
        // ① 拾い主のインベントリ取得
        var playerObj = NetworkManager.Singleton.SpawnManager
                              .GetPlayerNetworkObject(clientId)
                              .gameObject;
        var inv = playerObj.GetComponent<PlayerStatus>()
                           .ItemController
                           .GetComponent<ItemBelongingsController>();

        // ② 所持上限チェック
        bool reachedMax = inv.belonging_items
                             .Where(kvp => kvp.Key.item_type != ItemStatus.ItemType.Event)
                             .Count() >= inv.maxCarryableItems
                          || IsItemAtMaxCapacity(inv);

        bool success = !reachedMax ||
                       item_status.item_type == ItemStatus.ItemType.Event;

        // ③ サーバー側の処理
        if (success)
        {
            // 鍵ならカウント
            if (item_status.item_type == ItemStatus.ItemType.Key)
                need_keys_controller.GottenKeyCount++;

            // サーバー側インベントリ追加
            inv.ItemAddToBelonging(item_status);

            // 全クライアントからアイテムを消す
            thisNetObj.Despawn(true);
        }

        // ④ 結果を拾い主クライアントだけに通知
        var cp = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { clientId }
            }
        };
        ItemGetResponseClientRpc(success, cp);
    }

    // ────────────────────────────────────────
    // (2) Server → Client（拾い主のみ）
    // 成功／失敗を受け取り UI と効果音を処理
    // ────────────────────────────────────────
    [ClientRpc]
    private void ItemGetResponseClientRpc(bool success, ClientRpcParams _ = default)
    {
        // ここに来るのは「拾い主」のクライアントだけ

        // ① ローカルのインベントリ UI 取得
        var invCtrl = PlayerStatus.LocalInstance
                          .ItemController
                          .GetComponent<ItemBelongingsController>();

        // ② 成功時のローカル処理
        if (success)
        {
            // クライアント側インベントリリスト（ローカルコピー）更新
            invCtrl.ItemAddToBelonging(item_status);

            // 効果音・演出は UnityEvent で一括
            ItemGetAddEvent.Invoke();
        }

        // ③ UI (インベントリ枠が足りない等) の通知
        invCtrl.item_menu_controller
               .OnItemGetChangeUI(!success, item_status);
    }

    // ────────────────────────────────────────
    // ヘルパー：所持数上限チェック
    // ────────────────────────────────────────
    private bool IsItemAtMaxCapacity(ItemBelongingsController inv)
    {
        if (!inv.belonging_items.TryGetValue(item_status, out var entry))
            return false; // まだ持っていなければ容量オーバーではない

        return entry.item_count + 1 > item_status.MaxCanHave;
    }
}
