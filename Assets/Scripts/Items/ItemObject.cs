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
    /// 3) OKなら→サーバー在庫を更新→SuccessfulPickupClientRpc→Despawn(true)
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
        bool overSame = inv.belonging_items.TryGetValue(item_status, out var cnt)
                       && cnt.item_count + 1 > item_status.MaxCanHave;

        if (overMax || overSame)
        {
            // 取得失敗通知
            CannotPickupClientRpc(new ClientRpcParams {
                Send = new ClientRpcSendParams {
                    TargetClientIds = new[]{ clientId }
                }
            });
            return;
        }

        // サーバー在庫を更新（これがないとチェックがずっと 0 のままになる）
        if (!inv.belonging_items.ContainsKey(item_status))
            inv.belonging_items[item_status] = new ItemBelongingsController.ItemCountWithPermit {
                item_count = 1, CanUse = true
            };
        else
            inv.belonging_items[item_status].item_count++;

        // 取得成功通知を先に送信
        SuccessfulPickupClientRpc(new ClientRpcParams {
            Send = new ClientRpcSendParams {
                TargetClientIds = new[]{ clientId }
            }
        });

        // 全クライアントのワールド上から消去
        if (networkObject != null && networkObject.IsSpawned)
            networkObject.Despawn(destroy: true);
    }

    [ClientRpc]
    private void CannotPickupClientRpc(ClientRpcParams rpcParams = default)
    {
        var inv = PlayerStatus.LocalInstance
                            .ItemController
                            .GetComponent<ItemBelongingsController>();
        inv.item_menu_controller.OnItemGetChangeUI(true, item_status);
    }

    [ClientRpc]
    private void SuccessfulPickupClientRpc(ClientRpcParams rpcParams = default)
    {
        var inv = PlayerStatus.LocalInstance
                            .ItemController
                            .GetComponent<ItemBelongingsController>();

        // ローカルに追加
        inv.ItemAddToBelonging(item_status);
        ItemGetAddEvent.Invoke();
        PlayerStatus.LocalInstance
                    .cross_hair_event
                    .ItemGetSource
                    .Play();

        // 自身の GameObject も消しておく
        Destroy(gameObject);
    }
}
