// ItemBelongingsController.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;           // ← 追加
using Sirenix.OdinInspector;
using System.Linq;
using TMPro;
using UnityEngine.UI;
public class ItemBelongingsController : NetworkBehaviour  // ← MonoBehaviour → NetworkBehaviour
{
    public class ItemStatusUI
    {
        public Image           ItemIcon;
        public TextMeshProUGUI ItemCountTextUI;
        public GameObject      ItemUI;
        public TextMeshProUGUI ItemNameTextUI;
    }
    public class ItemCountWithPermit
    {
        public int  item_count;
        public bool CanUse = true;
    }

    public int maxCarryableItems = 6;
    public Dictionary<ItemStatus, ItemCountWithPermit> belonging_items = new Dictionary<ItemStatus, ItemCountWithPermit>();
    public Dictionary<ItemStatus, ItemStatusUI> belonging_item_uis = new Dictionary<ItemStatus, ItemStatusUI>();

    public GameObject ItemUIPrefab;
    public Transform ItemGroup;

    public CrossHairEvent cross_hair_event;
    [HideInInspector] public ItemMenuController item_menu_controller;
    private ItemThrowController item_throw_controller;
    private PlayerStatus player_status;
    private SubNoticedUIController sub_noticed_ui_controller;
    private EnemyDatabases enemy_databases;
    private EnemyAllController enemy_all_controller;
    private ItemDatabases item_databases;

    [SerializeField] private List<ItemStatus> InitialItemStatuses;

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
        // ここで何か NetworkBehaviour としての初期化が必要なら
        base.OnNetworkSpawn();
    }

    private void InitOnPlayerSpawn(PlayerStatus _playerStatus)
    {
        if (!IsOwner) return;  // オーナーだけが自分のインベントリを持つ
        player_status = _playerStatus;
        sub_noticed_ui_controller = player_status.sub_noticed_ui_controller;
        item_menu_controller = this.gameObject.GetComponent<ItemMenuController>();
        item_throw_controller = this.gameObject.GetComponent<ItemThrowController>();
        item_databases = ItemDatabases.instance;
        enemy_all_controller = EnemyDatabases.instance.gameObject.GetComponent<EnemyAllController>();

        // 初期アイテム（懐中電灯など）を全クライアントで付与
        foreach (ItemStatus _itemStatus in InitialItemStatuses)
        {
            ItemAddToBelonging(_itemStatus);
            item_menu_controller.OnItemGetChangeUI(false, _itemStatus);
        }
    }

    [Button]
    public void ItemAddToBelonging(ItemStatus _item_status)
    {
        if (!belonging_items.ContainsKey(_item_status))
        {
            var item_count_with_permit = new ItemCountWithPermit { item_count = 1, CanUse = true };
            belonging_items.Add(_item_status, item_count_with_permit);

            if (_item_status.item_type != ItemStatus.ItemType.Event)
            {
                var item_ui_obj = Instantiate(ItemUIPrefab, ItemGroup);
                var item_ui = item_ui_obj.GetComponent<ItemUI>();
                item_ui.item_belonging_controller = this;
                item_ui.item_status = _item_status;

                var item_status_ui = new ItemStatusUI
                {
                    ItemIcon = item_ui.ItemIcon,
                    ItemCountTextUI = item_ui.ItemCountTextUI,
                    ItemUI = item_ui.gameObject
                };

                belonging_item_uis.Add(_item_status, item_status_ui);
                belonging_item_uis[_item_status].ItemIcon.sprite = _item_status.MenuSprite;
                belonging_item_uis[_item_status].ItemCountTextUI.text = "1";

                item_menu_controller.ItemSelectByIndex(item_ui_obj.transform.GetSiblingIndex());
            }
        }
        else
        {
            int count = ++belonging_items[_item_status].item_count;
            Debug.Log("SetItemStatusToItemMenu アイテムメニューにアイテムが存在する");
            SetItemStatusToItemMenu(_item_status);
            item_menu_controller.ItemSelectByIndex(belonging_item_uis[_item_status].ItemUI.transform.GetSiblingIndex());
        }
    }

    [Button]
    public void ItemUseFromBelonging(ItemStatus item_status)
    {
        Debug.Log("ItemUseFromBelonging-----");
        int BelongingCountNow = belonging_items[item_status].item_count;

        // （省略：元のまま。投擲、消費、キー、イベント、緊急攻撃…）

        // ? 実際のスクリプト生成は元どおり
        var prefab = Instantiate(item_status.InstantiateItemScriptPrefab);
        prefab.GetComponent<InstantiateItemScript>().item_status = item_status;
    }

    public void ItemDump(ItemStatus _item_status)
    {
        if (!ExistsItems() || !_item_status.CanItemDump || _item_status == null || !belonging_items.ContainsKey(_item_status)) return;

        int currentCount = --belonging_items[_item_status].item_count;
        sub_noticed_ui_controller.Notice($"{_item_status.ItemName} を捨てた。");

        if (currentCount <= 0)
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
            SetItemStatusToItemMenu(_item_status);
        }

        Instantiate(_item_status.ItemPutObjectPrefab, player_status.HeadPos.position, Quaternion.identity);
    }

    public bool ExistsItems()            => belonging_items.Count > 0;
    public bool ExistBelongingItemByFind(ItemStatus s) => belonging_items.ContainsKey(s);
    public bool ExistItemUIItemStatusByFind(ItemStatus s) => belonging_item_uis.ContainsKey(s);
    public bool WhichCanUseItem(ItemStatus s) => belonging_items[s].CanUse;
    public void CanUseItem(bool can, ItemStatus s)
    {
        belonging_items[s].CanUse = can;
        item_menu_controller.OnCanItemUseChangeUI(can, s);
    }

    public List<ItemStatus> ItemFindByItemType(ItemStatus.ItemType _type)
    {
        var dict = belonging_items
            .Where(kvp => kvp.Key.item_type == _type)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        return dict.Keys.ToList();
    }

    private void ItemUse(int BelongingCountNow, ItemStatus item_status)
    {
        BelongingCountNow--;
        belonging_items[item_status].item_count = BelongingCountNow;

        if (BelongingCountNow > 0)
            SetItemStatusToItemMenu(item_status);
        else
        {
            SetItemStatusToItemMenu(item_status);
            belonging_items.Remove(item_status);
        }
    }

    private void SetItemStatusToItemMenu(ItemStatus item_status)
    {
        int count = belonging_items[item_status].item_count;
        var textUI = belonging_item_uis[item_status].ItemCountTextUI;
        Debug.Log("ItemCount-----:??? " + count);
        if (count > 0)
            textUI.text = count.ToString();
        else
        {
            Destroy(belonging_item_uis[item_status].ItemUI);
            belonging_item_uis.Remove(item_status);
        }
    }
}
