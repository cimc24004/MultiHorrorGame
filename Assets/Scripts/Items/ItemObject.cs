using System.Linq;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;

public class ItemObject : NetworkBehaviour
{
    [Tooltip("このオブジェクトが持つアイテム情報")]
    public ItemStatus item_status;

    [Tooltip("取得成功時に発火するイベント（UI更新など）")]
    public UnityEvent ItemGetAddEvent;

    private NetworkObject networkObject;

    // ← 追加：一度だけサーバー呼び出しを行うためのガード
    private bool pickupRequested = false;

    private void Start()
    {
        networkObject = GetComponent<NetworkObject>();
    }

    /// <summary>
    /// クライアント側：アイテムを拾いたいときに呼び出す
    /// </summary>
    public void TryPickup()
    {
        // すでに一度リクエスト済みなら何もしない
        if (pickupRequested) return;
        pickupRequested = true;

        RequestPickupServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPickupServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId     = rpcParams.Receive.SenderClientId;
        var clientInfo     = NetworkManager.Singleton.ConnectedClients[clientId];
        var ps             = clientInfo.PlayerObject.GetComponent<PlayerStatus>();
        var inv            = ps.ItemController.GetComponent<ItemBelongingsController>();

        // ── サーバーチェック ──
        int diffCount = inv.belonging_items
            .Where(kvp => kvp.Key != item_status && kvp.Key.item_type != ItemStatus.ItemType.Event)
            .Count();
        bool overMax  = diffCount >= inv.maxCarryableItems;
        bool overSame = inv.belonging_items.TryGetValue(item_status, out var cnt)
                       && cnt.item_count + 1 > item_status.MaxCanHave;

        if (overMax || overSame)
        {
            CannotPickupClientRpc(new ClientRpcParams {
                Send = new ClientRpcSendParams {
                    TargetClientIds = new[]{ clientId }
                }
            });
            return;
        }

        // ── 取得OK通知を先に送る ──
        SuccessfulPickupClientRpc(new ClientRpcParams {
            Send = new ClientRpcSendParams {
                TargetClientIds = new[]{ clientId }
            }
        });

        // ── 全クライアントのワールド上から消去 ──
        if (networkObject != null && networkObject.IsSpawned)
            networkObject.Despawn(destroy: true);
    }

    /// <summary>
    /// 取得失敗時：再度リクエスト可能に戻して、UI通知
    /// </summary>
    [ClientRpc]
    private void CannotPickupClientRpc(ClientRpcParams rpcParams = default)
    {
        // 失敗した自分だけ
        pickupRequested = false;

        var inv = PlayerStatus.LocalInstance
                            .ItemController
                            .GetComponent<ItemBelongingsController>();
        inv.item_menu_controller.OnItemGetChangeUI(true, item_status);
    }

    /// <summary>
    /// 取得成功時：インベントリ追加＋音＋自身オブジェクト破棄
    /// </summary>
    [ClientRpc]
    private void SuccessfulPickupClientRpc(ClientRpcParams rpcParams = default)
    {
        var inv = PlayerStatus.LocalInstance
                            .ItemController
                            .GetComponent<ItemBelongingsController>();

        // ローカル在庫追加
        inv.ItemAddToBelonging(item_status);

        // UI 更新／エフェクト
        ItemGetAddEvent.Invoke();

        // 取得音
        PlayerStatus.LocalInstance
                    .cross_hair_event
                    .ItemGetSource
                    .Play();

        // 自分のオブジェクトも消す
        Destroy(gameObject);
    }
}
