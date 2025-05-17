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
    /// 1) 本当に拾えるかチェック
    /// 2) NGなら CannotPickupClientRpc
    /// 3) OKなら先に SuccessfulPickupClientRpc → 後で Despawn(true)
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void RequestPickupServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        var clientInfo = NetworkManager.Singleton.ConnectedClients[clientId];
        var ps         = clientInfo.PlayerObject.GetComponent<PlayerStatus>();
        var inv        = ps.ItemController.GetComponent<ItemBelongingsController>();

        // サーバー側チェック
        int diffCount = inv.belonging_items
            .Where(kvp => kvp.Key != item_status 
                       && kvp.Key.item_type != ItemStatus.ItemType.Event)
            .Count();
        bool overMax  = diffCount >= inv.maxCarryableItems;
        bool overSame = inv.belonging_items.TryGetValue(item_status, out var cnt) &&
                        cnt.item_count + 1 > item_status.MaxCanHave;

        if (overMax || overSame)
        {
            CannotPickupClientRpc(new ClientRpcParams {
                Send = new ClientRpcSendParams {
                    TargetClientIds = new[]{ clientId }
                }
            });
            return;
        }

        // 1) 取得成功通知を先に飛ばす
        SuccessfulPickupClientRpc(new ClientRpcParams {
            Send = new ClientRpcSendParams {
                TargetClientIds = new[]{ clientId }
            }
        });

        // 2) ワールド上オブジェクトを消去（全クライアント同期）
        if (networkObject != null && networkObject.IsSpawned)
            networkObject.Despawn(destroy: true);
    }

    /// <summary>
    /// 取得失敗時：クライアント側で「これ以上持てない」UIを表示
    /// </summary>
    [ClientRpc]
    private void CannotPickupClientRpc(ClientRpcParams rpcParams = default)
    {
        var inv = PlayerStatus.LocalInstance
                            .ItemController
                            .GetComponent<ItemBelongingsController>();
        inv.item_menu_controller.OnItemGetChangeUI(true, item_status);
    }

    /// <summary>
    /// 取得成功時：クライアント側で
    /// 1) インベントリに追加
    /// 2) UI更新／エフェクト
    /// 3) サウンド再生
    /// 4) 自身の GameObject を Destroy
    /// </summary>
    [ClientRpc]
    private void SuccessfulPickupClientRpc(ClientRpcParams rpcParams = default)
    {
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

        // 4) クライアント自身の GameObject を削除
        Destroy(gameObject);
    }
}
