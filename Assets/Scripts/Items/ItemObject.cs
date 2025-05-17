using System.Linq;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;

public class ItemObject : NetworkBehaviour
{
    [Tooltip("このオブジェクトが持つアイテム情報")]
    public ItemStatus item_status;

    [Tooltip("取得成功時にローカルで発火するイベント（UI更新など）")]
    public UnityEvent ItemGetAddEvent;

    private NetworkObject networkObject;

    private void Start()
    {
        networkObject = GetComponent<NetworkObject>();
    }

    /// <summary>
    /// クライアント側：アイテムを拾いたいときに呼び出す
    /// </summary>
    public void TryPickup()
    {
        RequestPickupServerRpc();
    }

    /// <summary>
    /// サーバー側で実行：
    /// 1) 真に拾えるかチェック
    /// 2) NGなら CannotPickupClientRpc
    /// 3) OKなら Despawn(true) ＆ SuccessfulPickupClientRpc
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void RequestPickupServerRpc(ServerRpcParams rpcParams = default)
    {
        // リクエスト元クライアントID
        ulong clientId = rpcParams.Receive.SenderClientId;

        // ① そのクライアントの PlayerStatus を取得
        var clientInfo = NetworkManager.Singleton.ConnectedClients[clientId];
        var playerObj = clientInfo.PlayerObject;
        var ps = playerObj.GetComponent<PlayerStatus>();

        // ② ItemBelongingsController を PlayerStatus.ItemController 経由で取得
        var inv = ps.ItemController
                    .GetComponent<ItemBelongingsController>();

        // ③ サーバー側チェック：種類の異なるアイテム数
        int diffCount = inv.belonging_items
            .Where(kvp => kvp.Key != item_status
                       && kvp.Key.item_type != ItemStatus.ItemType.Event)
            .Count();
        bool overMax = diffCount >= inv.maxCarryableItems;

        // 同種アイテム数チェック
        bool overSame = inv.belonging_items.TryGetValue(item_status, out var cnt) &&
                        cnt.item_count + 1 > item_status.MaxCanHave;

        if (overMax || overSame)
        {
            // 取得失敗：該当クライアントだけに通知
            CannotPickupClientRpc(new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { clientId }
                }
            });
            return;
        }


        // ⑤ 成功クライアントだけにローカル処理を通知
        SuccessfulPickupClientRpc(new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { clientId }
            }
        });
        
        // ④ 取得成功：ワールド上から確実に削除
        if (networkObject != null && networkObject.IsSpawned)
            networkObject.Despawn(destroy: true);
    }

    /// <summary>
    /// 取得失敗時：クライアント側で「これ以上持てない」通知
    /// </summary>
    [ClientRpc]
    private void CannotPickupClientRpc(ClientRpcParams rpcParams = default)
    {
        // 失敗した自分だけ処理
        var inv = PlayerStatus.LocalInstance
                            .ItemController
                            .GetComponent<ItemBelongingsController>();
        inv.item_menu_controller.OnItemGetChangeUI(
             true,
             item_status
        );
    }

    /// <summary>
    /// 取得成功時：クライアント側でインベントリ追加＋通知＋サウンド
    /// </summary>
    [ClientRpc]
    private void SuccessfulPickupClientRpc(ClientRpcParams rpcParams = default)
    {
        // 成功した自分だけ処理
        var inv = PlayerStatus.LocalInstance
                            .ItemController
                            .GetComponent<ItemBelongingsController>();

        // 1) ローカルインベントリに追加
        inv.ItemAddToBelonging(item_status);

        // 2) UI更新／エフェクト
        ItemGetAddEvent.Invoke();

        // 3) 取得音を再生
        PlayerStatus.LocalInstance
                    .cross_hair_event
                    .ItemGetSource
                    .Play();

        // 4) ワールド上オブジェクトはサーバーで Despawn(true) 済み
    }
}
