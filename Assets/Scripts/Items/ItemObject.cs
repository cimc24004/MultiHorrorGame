<<<<<<< HEAD
// ItemObject.cs
=======
>>>>>>> c99834a4e5f81487c896c8539c73d8fde6ea283e
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;

<<<<<<< HEAD
public class ItemObject : NetworkBehaviour
{
    [Tooltip("このオブジェクトが持つアイテム情報")]
=======
[RequireComponent(typeof(NetworkObject))]
public class ItemObject : NetworkBehaviour
{
>>>>>>> c99834a4e5f81487c896c8539c73d8fde6ea283e
    public ItemStatus item_status;

    [Tooltip("取得時に発火するイベント (UI更新など)")]
    public UnityEvent ItemGetAddEvent;

<<<<<<< HEAD
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
=======
    private NeedKeysController need_keys_controller;
    private NetworkObject      netObj;

    void Awake()
    {
        netObj = GetComponent<NetworkObject>();
        if (NetworkManager.Singleton == null) return;
    }

    public override void OnNetworkSpawn()
    {
        need_keys_controller = NeedKeysController.instance;
    }
    public override void OnNetworkDespawn()
    {
        Debug.Log("アイテムDespawn");
        this.gameObject.SetActive(false);

    }
    // これが無いと CrossHairEvent から呼べない
    public void RequestItemGet()
    {
        // ローカル→サーバーへ拾いリクエスト飛ばすだけ
        if (!IsSpawned) return;                     // NetObj がまだ Spawn してなかったら無視
        RequestItemGetServerRpc(NetworkManager.Singleton.LocalClientId);
    }
    /*──────── Client → Server ───────*/
    [ServerRpc(RequireOwnership = false)]
    void RequestItemGetServerRpc(ulong clientId, ServerRpcParams _ = default)
    {
        var playerObj = NetworkManager.Singleton.SpawnManager
                              .GetPlayerNetworkObject(clientId).gameObject;

        var inv = playerObj.GetComponent<PlayerStatus>()
                           .ItemController
                           .GetComponent<ItemBelongingsController>();

        /* ---------- 上限チェック ---------- */
        int  uniqueCount = inv.belonging_items
                               .Where(kvp => kvp.Key.item_type != ItemStatus.ItemType.Event)
                               .Count();
        bool alreadyHave = inv.belonging_items.ContainsKey(item_status);
        bool slotLimit   = !alreadyHave && uniqueCount >= inv.maxCarryableItems;
        bool reachedMax  = slotLimit || IsItemAtMaxCapacity(inv);
        /* ---------------------------------- */

        bool success = !reachedMax ||
                       item_status.item_type == ItemStatus.ItemType.Event;

        int newCount = 0;
        if (success)
        {
            if (item_status.item_type == ItemStatus.ItemType.Key)
                need_keys_controller.GottenKeyCount++;

            inv.ItemAddToBelonging(item_status);
            newCount = inv.belonging_items[item_status].item_count;
        }

        var cp = new ClientRpcParams {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
        };
        ItemGetResponseClientRpc(success, newCount, cp);

        if (success)
        {
          netObj.Despawn(true);          // 全員から削除  
        } 
    }

    /*──────── Server → Client（拾い主）────────*/
    [ClientRpc]
    void ItemGetResponseClientRpc(bool success, int newCount, ClientRpcParams _ = default)
    {
        var invUI = PlayerStatus.LocalInstance
                      .ItemController
                      .GetComponent<ItemBelongingsController>();

        if (success)
        {
            invUI.ForceSyncFromServer(item_status, newCount);  // ★ホストでも呼ぶ
            ItemGetAddEvent.Invoke();                          // 効果音など
        }

        invUI.item_menu_controller.OnItemGetChangeUI(!success, item_status);
    }

    bool IsItemAtMaxCapacity(ItemBelongingsController inv)
    {
        if (!inv.belonging_items.TryGetValue(item_status, out var entry))
            return false;
        return entry.item_count + 1 > item_status.MaxCanHave;
>>>>>>> c99834a4e5f81487c896c8539c73d8fde6ea283e
    }
}
