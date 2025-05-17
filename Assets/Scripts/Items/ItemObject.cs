using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

/// <summary>ワールドに置かれた拾得アイテム。</summary>
[RequireComponent(typeof(NetworkObject))]
public class ItemObject : NetworkBehaviour
{
    [Header("アイテム本体のステータス")]
    public ItemStatus item_status;

    [Header("取得成功時に呼ばれる UnityEvent (効果音・演出)")]
    public UnityEvent ItemGetAddEvent;

    private NeedKeysController need_keys_controller;
    private NetworkObject      netObj;

    /* ─────────────────────────────────────── */

    private void Awake()
    {
        netObj = GetComponent<NetworkObject>();

        if (NetworkManager.Singleton == null) return;

        if (NetworkManager.Singleton.IsServer)
        {
            // ★ サーバーだけが Spawn する
            if (!netObj.IsSpawned) netObj.Spawn();
        }
        else
        {
            // ★ クライアントがシーンから読み込んだ
            //    余分なオブジェクトは即破棄
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        need_keys_controller = NeedKeysController.instance;
    }

    /* ─────────────────────────────────────── */
    /* クライアント入口：Raycast などから呼ぶ */
    /* ─────────────────────────────────────── */
    public void RequestItemGet()
    {
        RequestItemGetServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    /* ─────────────────────────────────────── */
    /* (1) Client → Server  RPC                */
    /* ─────────────────────────────────────── */
    [ServerRpc(RequireOwnership = false)]
    void RequestItemGetServerRpc(ulong clientId, ServerRpcParams _ = default)
    {
        /* ① 拾い主のインベントリを取得 */
        var playerObj = NetworkManager.Singleton.SpawnManager
                              .GetPlayerNetworkObject(clientId).gameObject;

        var inv = playerObj.GetComponent<PlayerStatus>()
                           .ItemController
                           .GetComponent<ItemBelongingsController>();

        /* ② 所持上限チェック */
        bool reachedMax = inv.belonging_items
                             .Where(kvp => kvp.Key.item_type != ItemStatus.ItemType.Event)
                             .Count() >= inv.maxCarryableItems
                      ||  IsItemAtMaxCapacity(inv);

        bool success = !reachedMax ||
                       item_status.item_type == ItemStatus.ItemType.Event;

        /* ③ 成功時のみサーバー側のインベントリを更新 */
        int newCount = 0;
        if (success)
        {
            if (item_status.item_type == ItemStatus.ItemType.Key)
                need_keys_controller.GottenKeyCount++;

            inv.ItemAddToBelonging(item_status);           // authoritative
            newCount = inv.belonging_items[item_status].item_count;
        }

        /* ④ 呼び出し主だけに結果と新カウントを返す */
        var cp = new ClientRpcParams {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
        };
        ItemGetResponseClientRpc(success, newCount, cp);

        /* ⑤ 成功時に Despawn → 全クライアントからアイテム消滅 */
        if (success)
            netObj.Despawn(true);
    }

    /* ─────────────────────────────────────── */
    /* (2) Server → Client RPC（拾い主のみ）   */
    /* ─────────────────────────────────────── */
    [ClientRpc]
    void ItemGetResponseClientRpc(
        bool success,
        int  newCount,
        ClientRpcParams _ = default)
    {
        // ここは拾ったクライアントだけに届く

        var inv = PlayerStatus.LocalInstance
                    .ItemController
                    .GetComponent<ItemBelongingsController>();

        if (success)
        {
            /* クライアント側でも辞書と UI を合わせる               */
            inv.ForceSyncFromServer(item_status, newCount);

            /* 効果音・エフェクトを UnityEvent で実行 */
            ItemGetAddEvent.Invoke();
        }

        /* 持てなかった場合の UI 通知 */
        inv.item_menu_controller.OnItemGetChangeUI(!success, item_status);
    }

    /* ヘルパー：同アイテム所持数上限チェック */
    bool IsItemAtMaxCapacity(ItemBelongingsController inv)
    {
        if (!inv.belonging_items.TryGetValue(item_status, out var entry))
            return false;

        return entry.item_count + 1 > item_status.MaxCanHave;
    }
}
