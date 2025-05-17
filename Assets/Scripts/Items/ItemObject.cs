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
    /// クライアント側：拾いたい！とサーバーにリクエスト
    /// </summary>
    public void TryPickup()
    {
        // ── クライアント先行チェック ──
        var inv = PlayerStatus.LocalInstance
                            .ItemController
                            .GetComponent<ItemBelongingsController>();

        int diffCount = inv.belonging_items
            .Where(kvp => kvp.Key != item_status
                       && kvp.Key.item_type != ItemStatus.ItemType.Event)
            .Count();
        bool overMax  = diffCount >= inv.maxCarryableItems;
        bool overSame = inv.belonging_items.TryGetValue(item_status, out var cnt)
                       && cnt.item_count + 1 > item_status.MaxCanHave;

        if (overMax || overSame)
        {
            inv.item_menu_controller.OnItemGetChangeUI(true, item_status);
            return;
        }

        if (pickupRequested) return;
        pickupRequested = true;

        RequestPickupServerRpc();
    }

    /// <summary>
    /// サーバー側：本当に拾えるかチェック＆同期
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
            CannotPickupClientRpc();
            return;
        }

        // サーバー在庫更新
        if (inv.belonging_items.ContainsKey(item_status))
            inv.belonging_items[item_status].item_count++;
        else
            inv.belonging_items[item_status] = new ItemBelongingsController.ItemCountWithPermit {
                item_count = 1, CanUse = true
            };

        // 成功通知（全クライアント）
        SuccessfulPickupClientRpc();

        // ネットワークから Despawn → OnNetworkDespawn が全クライアントで呼ばれる
        if (networkObject != null && networkObject.IsSpawned)
            networkObject.Despawn(destroy: true);
    }

    /// <summary>
    /// 取得失敗時：クライアント側で通知＆再リクエスト許可
    /// </summary>
    [ClientRpc]
    private void CannotPickupClientRpc(ClientRpcParams rpcParams = default)
    {
        pickupRequested = false;
        var inv = PlayerStatus.LocalInstance
                            .ItemController
                            .GetComponent<ItemBelongingsController>();
        inv.item_menu_controller.OnItemGetChangeUI(true, item_status);
    }

    /// <summary>
    /// 取得成功時：クライアント側でインベントリ追加＋UI＋音
    /// </summary>
    [ClientRpc]
    private void SuccessfulPickupClientRpc(ClientRpcParams rpcParams = default)
    {
        var inv = PlayerStatus.LocalInstance
                            .ItemController
                            .GetComponent<ItemBelongingsController>();

        // ホストでもクライアントでも +1 はここだけ
        inv.ItemAddToBelonging(item_status);

        ItemGetAddEvent.Invoke();
        PlayerStatus.LocalInstance
                    .cross_hair_event
                    .ItemGetSource
                    .Play();
    }

    /// <summary>
    /// ネットワーク Despawn 時に、必ず GameObject を破棄する
    /// （ホスト／クライアント問わず実行される）
    /// </summary>
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        // Despawn されたら必ず消す
        Destroy(gameObject);
    }
}
