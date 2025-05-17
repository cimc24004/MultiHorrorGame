using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class ItemObject : NetworkBehaviour
{
    private NeedKeysController need_keys_controller;
    public ItemStatus         item_status;
    public UnityEvent         ItemGetAddEvent;  // Inspector で音再生を登録
    private NetworkObject     ItemNetworkObject;
    private PlayerStatus player_status;
    private CrossHairEvent cross_hair_event;
    public override void OnNetworkSpawn()
    {
        need_keys_controller = NeedKeysController.instance;
        ItemNetworkObject = GetComponent<NetworkObject>();
    }
    private void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InitWhenPlayerSpawned;
        if (PlayerStatus.LocalInstance != null)
        {
            InitWhenPlayerSpawned(PlayerStatus.LocalInstance);
        }
    }
    private void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitWhenPlayerSpawned;
    }
    private void InitWhenPlayerSpawned(PlayerStatus _player_status)
    {
        player_status = _player_status;
        cross_hair_event = player_status.cross_hair_event;
        this.ItemGetAddEvent.AddListener(() =>
        {
            Debug.Log("ItemGetAddEvent-----------ItemGetSource-------------");
            cross_hair_event.ItemGetSource.Play();
        });
    }

    /// <summary>
    /// クライアント側から「このアイテムを拾いたい！」を呼ぶ
    /// </summary>
    public void RequestItemGet()
    {
        RequestItemGetServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    //──────────────────────────────────────────────
    // (1) ServerRpc：クライアント→サーバー
    //──────────────────────────────────────────────
    [ServerRpc(RequireOwnership = false)]
    private void RequestItemGetServerRpc(
        ulong clientId,
        ServerRpcParams rpcParams = default
    )
    {
        // サーバー側で、拾ったプレイヤーのインベントリと上限チェック
        var playerObj = NetworkManager.Singleton.SpawnManager
                          .GetPlayerNetworkObject(clientId)
                          .gameObject;
        var inv = playerObj
                    .GetComponent<PlayerStatus>()
                    .ItemController
                    .GetComponent<ItemBelongingsController>();

        bool reachedMax = inv.belonging_items
            .Where(kvp => kvp.Key.item_type != ItemStatus.ItemType.Event)
            .Count() >= inv.maxCarryableItems
         || IsItemAtMaxCapacity(inv);

        bool success = !reachedMax
                    || item_status.item_type == ItemStatus.ItemType.Event;

        // 鍵アイテムならキー数カウント
        if (success && item_status.item_type == ItemStatus.ItemType.Key)
            need_keys_controller.GottenKeyCount++;

        // サーバー権限でインベントリ更新 & Despawn
        if (success)
        {
            inv.ItemAddToBelonging(item_status);
            ItemGetAddEvent.Invoke();
            ItemNetworkObject.Despawn(true);
        }

        // (2) ClientRpc で呼び出し元クライアントだけ結果通知
        var rpcParamsClient = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { clientId }
            }
        };
        ItemGetResponseClientRpc(success, rpcParamsClient);
    }

    //──────────────────────────────────────────────
    // (3) ClientRpc：サーバー→呼び出し元クライアント
    //      ここで UnityEvent を発火 → 音やUIをまとめて実行
    //──────────────────────────────────────────────
    [ClientRpc]
    private void ItemGetResponseClientRpc(
        bool success,
        ClientRpcParams clientRpcParams = default
    )
    {
        // 自分以外は無視
        if (NetworkManager.Singleton.LocalClientId
            != clientRpcParams.Send.TargetClientIds[0]) return;

        // UI更新：持てなかった場合は !success、持てたら success
        var inv = PlayerStatus.LocalInstance
                    .ItemController
                    .GetComponent<ItemBelongingsController>();

        // 成功したらインベントリ追加＆UnityEvent発火（ここで音を鳴らします）
        if (success)
        {
            inv.ItemAddToBelonging(item_status);
            Debug.Log("アイテム取得した！");
            ItemGetAddEvent.Invoke();
        }

        // UIへの「持てなかった」通知
        inv.item_menu_controller
           .OnItemGetChangeUI(!success, item_status);
    }

    private bool IsItemAtMaxCapacity(ItemBelongingsController c)
    {
        if (!c.belonging_items.TryGetValue(item_status, out var e))
            return false;
        return e.item_count + 1 > item_status.MaxCanHave;
    }
}
