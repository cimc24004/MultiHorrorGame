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

        // 初期アイテム配布
        foreach (ItemStatus stat in InitialItemStatuses)
        {
            ItemAddToBelonging(stat);
            item_menu_controller.OnItemGetChangeUI(false, stat);
        }
    }

    // アイテム追加（初回 or 2個目以降）
    [Button]
    public void ItemAddToBelonging(ItemStatus _item_status)
    {
        if (!belonging_items.ContainsKey(_item_status))
        {
            // 新規登録
            var icwp = new ItemCountWithPermit { item_count = 1, CanUse = true };
            belonging_items.Add(_item_status, icwp);

            // UI設置 (イベントアイテム以外)
            if (_item_status.item_type != ItemStatus.ItemType.Event)
            {
                GameObject uiObj = Instantiate(ItemUIPrefab, ItemGroup);
                ItemUI ui = uiObj.GetComponent<ItemUI>();
                ui.item_belonging_controller = this;
                ui.item_status = _item_status;

                var statusUI = new ItemStatusUI
                {
                    ItemIcon        = ui.ItemIcon,
                    ItemCountTextUI = ui.ItemCountTextUI,
                    ItemUI          = ui.gameObject
                };
                belonging_item_uis.Add(_item_status, statusUI);

                statusUI.ItemIcon.sprite                 = _item_status.MenuSprite;
                statusUI.ItemCountTextUI.text            = "1";
                item_menu_controller.ItemSelectByIndex(uiObj.transform.GetSiblingIndex());
            }
        }
        else
        {
            // 個数増加
            int cnt = belonging_items[_item_status].item_count + 1;
            belonging_items[_item_status].item_count = cnt;
            SetItemStatusToItemMenu(_item_status);
            item_menu_controller.ItemSelectByIndex(
                belonging_item_uis[_item_status].ItemUI.transform.GetSiblingIndex()
            );
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
            item_throw_controller.ThrowStart(item_status.ItemThrowPrefab);
            item_throw_controller.item_status = item_status;
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
                    doorCtrl.open_permit_by_key_controller.KeyUse();
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
        sub_noticed_ui_controller.Notice($"{_item_status.ItemName} を捨てた。");

        if (c <= 0)
        {
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

        Instantiate(_item_status.ItemPutObjectPrefab,
                    player_status.HeadPos.position,
                    Quaternion.identity);
    }

    // 存在チェック
    public bool ExistsItems() => belonging_items.Count > 0;

    public bool ExistBelongingItemByFind(ItemStatus s)
        => belonging_items.ContainsKey(s);

    public bool ExistItemUIItemStatusByFind(ItemStatus s)
        => belonging_item_uis.ContainsKey(s);

    public bool WhichCanUseItem(ItemStatus s)
        => belonging_items[s].CanUse;

    public void CanUseItem(bool can, ItemStatus s)
    {
        belonging_items[s].CanUse = can;
        item_menu_controller.OnCanItemUseChangeUI(can, s);
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
