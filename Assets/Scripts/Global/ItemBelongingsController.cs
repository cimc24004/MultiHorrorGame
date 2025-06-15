using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Sirenix.OdinInspector;
using System.Linq;
using TMPro;
using UnityEngine.UI;
public class ItemBelongingsController : NetworkBehaviour
{
    // アイテムUI情報用クラス
    public class ItemStatusUI
    {
        public Image           ItemIcon;
        public TextMeshProUGUI ItemCountTextUI;
        public GameObject      ItemUI;
        public TextMeshProUGUI ItemNameTextUI;
    }

    // 個数＋使用許可フラグ
    public class ItemCountWithPermit
    {
        public int  item_count;
        public bool CanUse = true;
    }

    //── Inspector設定 ───────────────────────────
    [Header("最大所持数")]
    public int maxCarryableItems = 6;

    [Header("初期所持アイテム (Ownerのみ配布)")]
    [SerializeField] private List<ItemStatus> InitialItemStatuses;

    [Header("UIプレハブ・親")]
    public GameObject ItemUIPrefab;
    public Transform  ItemGroup;

    [Header("関連コンポーネント")]
    public CrossHairEvent cross_hair_event;
    [HideInInspector] public ItemMenuController item_menu_controller;

    //── ランタイム用 ───────────────────────────
    public Dictionary<ItemStatus, ItemCountWithPermit> belonging_items    = new Dictionary<ItemStatus, ItemCountWithPermit>();
    public Dictionary<ItemStatus, ItemStatusUI>        belonging_item_uis = new Dictionary<ItemStatus, ItemStatusUI>();

    private ItemThrowController     item_throw_controller;
    private PlayerStatus            player_status;
    private SubNoticedUIController  sub_noticed_ui_controller;
    private EnemyDatabases          enemy_databases;
    private EnemyAllController      enemy_all_controller;
    private ItemDatabases           item_databases;

    void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InitOnPlayerSpawn;
    }

    void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitOnPlayerSpawn;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    // ローカルプレイヤー生成時に呼ばれる
    private void InitOnPlayerSpawn(PlayerStatus _playerStatus)
    {
        // Owner のみ初期化
        if (!IsOwner) return;

        player_status          = _playerStatus;
        sub_noticed_ui_controller = player_status.sub_noticed_ui_controller;
        item_menu_controller   = GetComponent<ItemMenuController>();
        item_throw_controller  = GetComponent<ItemThrowController>();
        item_databases         = ItemDatabases.instance;
        enemy_all_controller   = EnemyDatabases.instance.gameObject.GetComponent<EnemyAllController>();

        // // 初期アイテム配布
        // foreach (ItemStatus stat in InitialItemStatuses)
        // {
        //     ItemAddToBelonging(stat);
        //     item_menu_controller.OnItemGetChangeUI(false, stat);
        // }
    }

    // アイテム追加（初回 or 2個目以降）
    [Button]
    public void ItemAddToBelonging(ItemStatus _item_status)
    {
        // ── 1) 在庫辞書を先に更新 ──
        if (belonging_items.TryGetValue(_item_status, out var countPermit))
        {
            // 既存アイテムならカウントを増やす
            countPermit.item_count++;
        }
        else
        {
            // 初めてのアイテムなら新規登録
            belonging_items[_item_status] = new ItemCountWithPermit {
                item_count = 1,
                CanUse     = true
            };
        }

        // ── 2) UI 用辞書を更新 or 生成 ──
        // イベントアイテムは UI を持たないので処理を抜ける
        if (_item_status.item_type == ItemStatus.ItemType.Event)
            return;

        if (belonging_item_uis.TryGetValue(_item_status, out var uiEntry))
        {
            // 既存の UI があればテキストだけ更新
            uiEntry.ItemCountTextUI.text = belonging_items[_item_status].item_count.ToString();
        }
        else
        {
            // UI がなければ新規作成
            GameObject uiObj = Instantiate(ItemUIPrefab, ItemGroup);
            ItemUI ui = uiObj.GetComponent<ItemUI>();
            ui.item_belonging_controller = this;
            ui.item_status              = _item_status;

            var newEntry = new ItemStatusUI {
                ItemIcon         = ui.ItemIcon,
                ItemCountTextUI  = ui.ItemCountTextUI,
                ItemUI           = ui.gameObject
            };
            belonging_item_uis[_item_status] = newEntry;

            newEntry.ItemIcon.sprite        = _item_status.MenuSprite;
            newEntry.ItemCountTextUI.text   = belonging_items[_item_status].item_count.ToString();

            // 取得したらすぐに選択状態にする
            item_menu_controller.ItemSelectByIndex(uiObj.transform.GetSiblingIndex());
        }
    }
    // アイテム使用
    [Button]
    public void ItemUseFromBelonging(ItemStatus item_status)
    {
        Debug.Log("ItemUseFromBelonging-----");

        int cntNow = belonging_items[item_status].item_count;

        // 投擲アイテム
        if (item_status.item_type == ItemStatus.ItemType.CanThrow)
        {
            item_throw_controller.ThrowStart(item_status);
            ItemUse(cntNow, item_status);
        }
        // 消費アイテム
        else if (item_status.item_type == ItemStatus.ItemType.Consumable)
        {
            ItemUse(cntNow, item_status);
        }
        // 装備アイテム
        else if (item_status.item_type == ItemStatus.ItemType.Equip)
        {
            // （装備系処理があればここに）
        }
        // 鍵アイテム
        else if (item_status.item_type == ItemStatus.ItemType.Key)
        {
            Debug.Log("RaycasstedObj: " + cross_hair_event.RaycasstedObj);
            if (cross_hair_event.RaycasstedObj != null &&
                cross_hair_event.RaycasstedObj.tag == "Door")
            {
                var doorOcc = cross_hair_event.RaycasstedObj
                                .GetComponent<DoorOpenCloseController>();
                var doorCtrl = doorOcc.the_door_controller;
                if (doorCtrl.NeededKey.Value &&
                    !doorCtrl.DoorOpenPermitCheck())
                {
                    Debug.Log("NetworkObject???: " + doorCtrl.GetComponent<NetworkObject>());
                    doorCtrl.open_permit_by_key_controller.KeyUseRequestServerRpc();
                    ItemUse(cntNow, item_status);
                }
            }
        }
        // イベントアイテム
        else if (item_status.item_type == ItemStatus.ItemType.Event)
        {
            ItemUse(cntNow, item_status);
        }
        // 緊急攻撃アイテム
        else if (item_status.item_type == ItemStatus.ItemType.EmergencyAttack)
        {
            ItemUse(cntNow, item_status);
        }

        // スクリプト生成プレハブ
        GameObject inst = Instantiate(item_status.InstantiateItemScriptPrefab);
        inst.GetComponent<InstantiateItemScript>().item_status = item_status;
    }

    // アイテム破棄
    public void ItemDump(ItemStatus _item_status)
    {
        if (!ExistsItems()) return;
        if (!_item_status.CanItemDump) return;
        if (_item_status == null || !belonging_items.ContainsKey(_item_status)) return;

        int c = belonging_items[_item_status].item_count - 1;
        ItemDumpSpawnServerRpc(_item_status.GlobalId, player_status.HeadPos.position);
        if(c <= 0){
            item_menu_controller.item_equip_controller.EquipChange(_item_status, false);
            if (belonging_items.Count > 1)
                item_menu_controller.ItemSelectAuto(_item_status);

            Destroy(belonging_item_uis[_item_status].ItemUI);
            belonging_item_uis.Remove(_item_status);
            belonging_items.Remove(_item_status);
        }
        else
        {
            belonging_items[_item_status].item_count = c;
            SetItemStatusToItemMenu(_item_status);
        }

        // アイテムの出現はサーバー側の ItemDumpSpawnServerRpc で行う
        // ローカル生成するとネットワーク同期されないため削除
    }
    [ServerRpc]
    private void ItemDumpSpawnServerRpc(uint itemGlobalId, Vector3 position)
    {
        ItemStatus status = ItemDatabases.instance.GetStatusByGlobalId(itemGlobalId);
        if (status == null) return;
        GameObject obj = Instantiate(status.ItemPutObjectPrefab, position, Quaternion.identity);
        var netObj = obj.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn(true);
        }
    }
    // 存在チェック
    public bool ExistsItems() => belonging_items.Count > 0;

    public bool ExistBelongingItemByFind(ItemStatus s)
        => belonging_items.ContainsKey(s);

    public bool ExistItemUIItemStatusByFind(ItemStatus s)
        => belonging_item_uis.ContainsKey(s);

    public bool WhichCanUseItem(ItemStatus s)
        => belonging_items[s].CanUse;

    // 1) クライアント呼び出し用
    public void RequestCanUseItem(bool canUse, ItemStatus status)
    {
        // 自分のアイテムGlobalIdを渡す
        CanUseItemServerRpc(status.GlobalId, canUse);
    }
    [ServerRpc]
    // 2) サーバー側で実行
    private void CanUseItemServerRpc(uint itemGlobalId, bool canUse, ServerRpcParams rpcParams = default)
    {
        var status = ItemDatabases.instance.GetStatusByGlobalId(itemGlobalId);
        if (status == null || !belonging_items.ContainsKey(status)) return;

        // サーバーで権限を担保して更新
        belonging_items[status].CanUse = canUse;

        // 結果を送信。発行元クライアントだけに返したいなら rpcParams.SourceClientId を使う
        var clientRpcParams = new ClientRpcParams {
            Send = new ClientRpcSendParams {
                TargetClientIds = new ulong[]{ rpcParams.Receive.SenderClientId }
            }
        };
        CanUseItemClientRpc(itemGlobalId, canUse, clientRpcParams);
    }

    // 3) クライアント側で実行
    [ClientRpc]
    private void CanUseItemClientRpc(uint itemGlobalId, bool canUse, ClientRpcParams rpcParams = default)
    {
        var status = ItemDatabases.instance.GetStatusByGlobalId(itemGlobalId);
        if (status == null || !belonging_items.ContainsKey(status)) return;

        // ローカル辞書も更新して…
        belonging_items[status].CanUse = canUse;
        // UI 更新
        item_menu_controller.OnCanItemUseChangeUI(canUse, status);
    }

    public List<ItemStatus> ItemFindByItemType(ItemStatus.ItemType t)
    {
        var dict = belonging_items
                   .Where(x => x.Key.item_type == t)
                   .ToDictionary(x => x.Key, x => x.Value);
        return dict.Keys.ToList();
    }

    // 個数減少＆UI更新
    private void ItemUse(int beforeCount, ItemStatus status)
    {
        int after = beforeCount - 1;
        belonging_items[status].item_count = after;

        if (after > 0)
        {
            SetItemStatusToItemMenu(status);
        }
        else
        {
            SetItemStatusToItemMenu(status);
            belonging_items.Remove(status);
        }
    }

    // UIテキスト更新 or アイコン破棄
    private void SetItemStatusToItemMenu(ItemStatus status)
    {
        int cnt = belonging_items[status].item_count;
        var textUI = belonging_item_uis[status].ItemCountTextUI;
        if (cnt > 0)
            textUI.text = cnt.ToString();
        else
        {
            Destroy(belonging_item_uis[status].ItemUI);
            belonging_item_uis.Remove(status);
        }
    }
}
