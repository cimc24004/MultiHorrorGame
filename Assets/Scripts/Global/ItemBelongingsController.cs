using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using System.Linq;
using Unity.Netcode;                               // ★

public class ItemBelongingsController : NetworkBehaviour   // ★ NetworkBehaviour 化
{
    //──────────────── 定義 ────────────────
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

    //──────────────── 変数 ────────────────
    public int maxCarryableItems = 6;

    public Dictionary<ItemStatus, ItemCountWithPermit> belonging_items    = new();
    public Dictionary<ItemStatus, ItemStatusUI>        belonging_item_uis = new();

    public GameObject ItemUIPrefab;
    public Transform  ItemGroup;

    public CrossHairEvent cross_hair_event;
    [HideInInspector] public ItemMenuController item_menu_controller;

    private ItemThrowController     item_throw_controller;
    private PlayerStatus            player_status;
    private SubNoticedUIController  sub_noticed_ui_controller;
    private EnemyAllController      enemy_all_controller;
    private ItemDatabases           item_databases;

    [SerializeField] private List<ItemStatus> InitialItemStatuses;

    //──────────────── Unity イベント ────────────────
    void OnEnable()  => PlayerStatus.OnLocalPlayerSpawned += InitOnPlayerSpawn;
    void OnDisable() => PlayerStatus.OnLocalPlayerSpawned -= InitOnPlayerSpawn;

    void Start()
    {
        item_throw_controller = GetComponent<ItemThrowController>();
        item_databases        = ItemDatabases.instance;
        enemy_all_controller  = EnemyDatabases.instance.gameObject.GetComponent<EnemyAllController>();
    }

    private void InitOnPlayerSpawn(PlayerStatus _playerStatus)
    {
        player_status             = _playerStatus;
        sub_noticed_ui_controller = player_status.sub_noticed_ui_controller;
        item_menu_controller      = GetComponent<ItemMenuController>();

        // 初期アイテムはサーバーだけで配布 → ClientRpc で同期
        if (IsServer)
        {
            GiveInitialItems();                                              // ★
            SendInitialItemsClientRpc(
                InitialItemStatuses.Select(st => st.GlobalId).ToArray());    // ★
        }
    }

    //──────────────── 初期アイテム配布 ────────────────
    private void GiveInitialItems()                                          // ★
    {
        foreach (var st in InitialItemStatuses)
        {
            ItemAddToBelonging(st);
            item_menu_controller.OnItemGetChangeUI(false, st);
        }
    }

    [ClientRpc]                                                              // ★
    private void SendInitialItemsClientRpc(uint[] gids, ClientRpcParams _ = default)
    {
        if (IsServer) return; // ホストはもう持ってる

        foreach (uint gid in gids)
        {
            var st = item_databases.GetStatusByGlobalId(gid);
            ForceSyncFromServer(st, 1);
        }
    }

    //──────────────── 外部 API ────────────────
    [Button]
    public void ItemAddToBelonging(ItemStatus _item_status)
    {
        // クライアント側はサーバー経由に統一
        if (NetworkManager.Singleton && !NetworkManager.Singleton.IsServer) return;

        if (!belonging_items.ContainsKey(_item_status))
        {
            //―― 新規取得 ―――――――――――――――――――――――――――――――
            ItemCountWithPermit cp = new() { item_count = 1, CanUse = true };
            belonging_items.Add(_item_status, cp);

            if (_item_status.item_type != ItemStatus.ItemType.Event)
            {
                GameObject uiObj = Instantiate(ItemUIPrefab, ItemGroup);
                ItemUI      iu   = uiObj.GetComponent<ItemUI>();

                iu.item_belonging_controller = this;
                iu.item_status               = _item_status;

                ItemStatusUI ui = new() {
                    ItemIcon        = iu.ItemIcon,
                    ItemCountTextUI = iu.ItemCountTextUI,
                    ItemUI          = iu.gameObject,
                    ItemNameTextUI  = null
                };
                belonging_item_uis.Add(_item_status, ui);

                ui.ItemIcon.sprite            = _item_status.MenuSprite;
                ui.ItemCountTextUI.text       = "1";
                item_menu_controller.ItemSelectByIndex(uiObj.transform.GetSiblingIndex());
            }
        }
        else
        {
            //―― すでに所持 → 個数だけ増加 ――――――――――――――――――――
            int count = belonging_items[_item_status].item_count + 1;
            belonging_items[_item_status].item_count = count;
            SetItemStatusToItemMenu(_item_status);
            item_menu_controller.ItemSelectByIndex(
                belonging_item_uis[_item_status].ItemUI.transform.GetSiblingIndex());
        }
    }

    //──────────────── アイテム使用 / 破棄 ────────────────
    [Button]
    public void ItemUseFromBelonging(ItemStatus item_status)
    {
        int countNow = belonging_items[item_status].item_count;

        if (item_status.item_type == ItemStatus.ItemType.CanThrow)
        {
            item_throw_controller.ThrowStart(item_status.ItemThrowPrefab);
            item_throw_controller.item_status = item_status;
            ItemUse(countNow, item_status);
        }
        else if (item_status.item_type == ItemStatus.ItemType.Consumable)
        {
            ItemUse(countNow, item_status);
        }
        else if (item_status.item_type == ItemStatus.ItemType.Equip)
        {
            // 懐中電灯など
        }
        else if (item_status.item_type == ItemStatus.ItemType.Key)
        {
            if (cross_hair_event.RaycasstedObj &&
                cross_hair_event.RaycasstedObj.CompareTag("Door"))
            {
                var doorCtrl  = cross_hair_event.RaycasstedObj.GetComponent<DoorOpenCloseController>();
                var theDoor   = doorCtrl.the_door_controller;
                if (theDoor.NeededKey.Value && !theDoor.DoorOpenPermitCheck())
                {
                    theDoor.open_permit_by_key_controller.KeyUse();
                    ItemUse(countNow, item_status);
                }
            }
        }
        else if (item_status.item_type == ItemStatus.ItemType.Event)
        {
            ItemUse(countNow, item_status);
        }
        else if (item_status.item_type == ItemStatus.ItemType.EmergencyAttack)
        {
            ItemUse(countNow, item_status);
        }

        GameObject prefab = Instantiate(item_status.InstantiateItemScriptPrefab);
        prefab.GetComponent<InstantiateItemScript>().item_status = item_status;
    }

    public void ItemDump(ItemStatus st)
    {
        if (!ExistsItems() || !st.CanItemDump || !belonging_items.ContainsKey(st))
            return;

        int count = --belonging_items[st].item_count;
        sub_noticed_ui_controller.Notice($"{st.ItemName} を捨てた。");

        if (count <= 0)
        {
            item_menu_controller.item_equip_controller.EquipChange(st, false);
            if (belonging_items.Count > 1) item_menu_controller.ItemSelectAuto(st);
            Destroy(belonging_item_uis[st].ItemUI);
            belonging_item_uis.Remove(st);
            belonging_items.Remove(st);
        }
        else
        {
            SetItemStatusToItemMenu(st);
        }

        Instantiate(st.ItemPutObjectPrefab, player_status.HeadPos.position, Quaternion.identity);
    }

    //──────────────── 補助 ────────────────
    public bool ExistsItems() => belonging_items.Count > 0;

    public bool ExistBelongingItemByFind(ItemStatus st) =>
        belonging_items.ContainsKey(st);

    public bool ExistItemUIItemStatusByFind(ItemStatus st) =>
        belonging_item_uis.ContainsKey(st);

    public bool WhichCanUseItem(ItemStatus st) => belonging_items[st].CanUse;

    public void CanUseItem(bool can, ItemStatus st)
    {
        belonging_items[st].CanUse = can;
        item_menu_controller.OnCanItemUseChangeUI(can, st);
    }

    public List<ItemStatus> ItemFindByItemType(ItemStatus.ItemType t)
    {
        var list = belonging_items
                   .Where(kvp => kvp.Key.item_type == t)
                   .Select(kvp => kvp.Key)
                   .ToList();
        return list.Count > 0 ? list : null;
    }

    private void ItemUse(int countNow, ItemStatus st)
    {
        countNow--;
        belonging_items[st].item_count = countNow;

        if (countNow > 0)
        {
            SetItemStatusToItemMenu(st);
        }
        else
        {
            SetItemStatusToItemMenu(st);
            belonging_items.Remove(st);
        }
    }

    private void SetItemStatusToItemMenu(ItemStatus st)
    {
        int cnt = belonging_items.ContainsKey(st) ? belonging_items[st].item_count : 0;

        if (cnt > 0)
        {
            belonging_item_uis[st].ItemCountTextUI.text = cnt.ToString();
        }
        else
        {
            Destroy(belonging_item_uis[st].ItemUI);
            belonging_item_uis.Remove(st);
        }
    }

    //──────────────── サーバー → クライアント同期 ────────────────
    public void ForceSyncFromServer(ItemStatus st, int newCount)
    {
        if (!belonging_items.ContainsKey(st))
        {
            ItemAddToBelonging(st);
            belonging_items[st].item_count = newCount;
            SetItemStatusToItemMenu(st);
        }
        else
        {
            belonging_items[st].item_count = newCount;
            SetItemStatusToItemMenu(st);
        }
    }
}
