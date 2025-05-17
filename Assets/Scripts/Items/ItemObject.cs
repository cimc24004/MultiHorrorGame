// ItemObject.cs
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;

public class ItemObject : NetworkBehaviour
{
    [Tooltip("このオブジェクトが持つアイテム情報")]
    public ItemStatus item_status;

    [Tooltip("取得時に発火するイベント (UI更新など)")]
    public UnityEvent ItemGetAddEvent;

    private NetworkObject ItemNetworkObject;

    private void Start()
    {
        ItemNetworkObject = GetComponent<NetworkObject>();
    }

    /// <summary>
    /// クライアント側で呼び出す：
    /// 1) 上限チェック＋ローカルUI通知
    /// 2) 問題なければサーバーへ消去要求
    /// 3) ローカルでインベントリ追加＆通知
    /// </summary>
    public bool TryPickup(ItemBelongingsController item_belonging_controller)
    {
        // ── 1) 所持上限チェック ──
        var diff = item_belonging_controller.belonging_items
            .Where(kvp => kvp.Key != item_status && kvp.Key.item_type != ItemStatus.ItemType.Event)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        bool notHaveMore = (item_belonging_controller.maxCarryableItems <= diff.Count)
                          || (item_belonging_controller.belonging_items.ContainsKey(item_status)
                              && item_belonging_controller.belonging_items[item_status].item_count + 1 > item_status.MaxCanHave);

        // UI通知（「これ以上持てない」 or 「取得」）
        item_belonging_controller.item_menu_controller.OnItemGetChangeUI(notHaveMore, item_status);

        // イベントアイテム以外で上限なら何もしない
        if (notHaveMore && item_status.item_type != ItemStatus.ItemType.Event)
            return false;

        // ── 2) サーバーへワールド上オブジェクト消去要求 ──
        RequestDespawnServerRpc();

        // ── 3) ローカルでインベントリに追加 & 通知 ──
        if (item_status.item_type == ItemStatus.ItemType.Key)
        {
            // キーアイテムならキー数も増やす
            NeedKeysController.instance.GottenKeyCount += 1;
        }

        item_belonging_controller.ItemAddToBelonging(item_status);
        ItemGetAddEvent.Invoke();

        // ほんとは NetworkObject.Despawn() で消えるけど、
        // クライアントのシーン上からも Remove したければ Destroy
        Destroy(gameObject, 0.1f);
        return true;
    }

    /// <summary>
    /// サーバー側で実行：全クライアントに Despawn を反映
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void RequestDespawnServerRpc(ServerRpcParams rpcParams = default)
    {
        if (ItemNetworkObject != null && ItemNetworkObject.IsSpawned)
        {
            ItemNetworkObject.Despawn();
        }
    }
}
