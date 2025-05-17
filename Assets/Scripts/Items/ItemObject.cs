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
    private bool pickupRequested = false;

    private void Start()
    {
        networkObject = GetComponent<NetworkObject>();
    }

    /// <summary>
    /// クライアント側：アイテムを拾いたいときに呼ぶ
    /// </summary>
    public void TryPickup()
    {
        // ── 1) クライアント先行チェック ──
        var inv = PlayerStatus.LocalInstance
                            .ItemController
                            .GetComponent<ItemBelongingsController>();

        int diffCount = inv.belonging_items
            .Where(kvp => kvp.Key != item_status && kvp.Key.item_type != ItemStatus.ItemType.Event)
            .Count();
        bool overMax = diffCount >= inv.maxCarryableItems;
        bool overSame = inv.belonging_items.TryGetValue(item_status, out var cnt)
                       && cnt.item_count + 1 > item_status.MaxCanHave;

        if (overMax || overSame)
        {
            inv.item_menu_controller.OnItemGetChangeUI(true, item_status);
            return;
        }

        // 二重リクエスト防止
        if (pickupRequested) return;
        pickupRequested = true;

        // ここでサーバーへ RPC 投げる
        RequestPickupServerRpc();
    }

    /// <summary>
    /// サーバー側：本当に拾えるかチェックして同期
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void RequestPickupServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        var clientInfo = NetworkManager.Singleton.ConnectedClients[clientId];
        var ps = clientInfo.PlayerObject.GetComponent<PlayerStatus>();
        var inv = ps.ItemController.GetComponent<ItemBelongingsController>();

        // サーバー側チェック
        int diffCount = inv.belonging_items
            .Where(kvp => kvp.Key != item_status && kvp.Key.item_type != ItemStatus.ItemType.Event)
            .Count();
        bool overMax = diffCount >= inv.maxCarryableItems;
        bool overSame = inv.belonging_items.TryGetValue(item_status, out var cnt)
                       && cnt.item_count + 1 > item_status.MaxCanHave;

        if (overMax || overSame)
        {
            CannotPickupClientRpc(new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { clientId }
                }
            });
            return;
        }

        // サーバー在庫も更新
        if (inv.belonging_items.ContainsKey(item_status))
            inv.belonging_items[item_status].item_count++;
        else
            inv.belonging_items[item_status] = new ItemBelongingsController.ItemCountWithPermit
            {
                item_count = 1,
                CanUse = true
            };

        // 取得成功通知
        SuccessfulPickupClientRpc(new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { clientId }
            }
        });

        // 全クライアントから消去
        if (networkObject != null && networkObject.IsSpawned)
            networkObject.Despawn(destroy: true);
    }

    /// <summary>
    /// 取得失敗時：クライアント側で通知＆再リクエスト許可
    /// </summary>
    [ClientRpc]
    private void CannotPickupClientRpc(ClientRpcParams rpcParams = default)
    {
        var inv = PlayerStatus.LocalInstance
                            .ItemController
                            .GetComponent<ItemBelongingsController>();

        // 再度拾えるようにフラグ戻し
        pickupRequested = false;
        inv.item_menu_controller.OnItemGetChangeUI(true, item_status);
    }

    /// <summary>
    /// 取得成功時：ローカル追加＋UI＋音＋自身破棄
    /// </summary>
    [ClientRpc]
    private void SuccessfulPickupClientRpc(ClientRpcParams rpcParams = default)
    {
        var inv = PlayerStatus.LocalInstance
                            .ItemController
                            .GetComponent<ItemBelongingsController>();

        // 1) ローカル在庫に追加
        inv.ItemAddToBelonging(item_status);

        // 2) UI更新／エフェクト
        ItemGetAddEvent.Invoke();

        // 3) 取得音再生
        PlayerStatus.LocalInstance
                    .cross_hair_event
                    .ItemGetSource
                    .Play();

        // 4) 自身 GameObject 破棄
        // Destroy(gameObject);
    }
    
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        // ここで必ず Destroy することで、
        // クライアントにオブジェクトが残らないようにします。
        Destroy(gameObject);
    }
}
