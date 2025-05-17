using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Linq;
using TMPro;
using Unity.Netcode;

public class ItemBelongingsController : NetworkBehaviour   // ★FIX NetworkBehaviour
{
    //────────────────────────────────────────
    // アイテムの UI 情報
    //────────────────────────────────────────
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

    //────────────────────────────────────────
    // Inspector
    //────────────────────────────────────────
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
    private EnemyDatabases          enemy_databases;
    private EnemyAllController      enemy_all_controller;
    private ItemDatabases           item_databases;

    [SerializeField] private List<ItemStatus> InitialItemStatuses;

    //────────────────────────────────────────
    // NetworkSpawn（全プレイヤーで呼ばれる）
    //────────────────────────────────────────
    public override void OnNetworkSpawn()
    {
        item_throw_controller = GetComponent<ItemThrowController>();
        item_databases        = ItemDatabases.instance;
        enemy_all_controller  = EnemyDatabases.instance.GetComponent<EnemyAllController>();

        // ★FIX サーバーが必ず初期アイテムを辞書に登録
        if (IsServer)
        {
            foreach (var st in InitialItemStatuses)
                if (!belonging_items.ContainsKey(st))
                    ItemAddToBelonging(st, false);     // notifyUI=false
        }
    }

    //────────────────────────────────────────
    // ローカルプレイヤー初期化
    //────────────────────────────────────────
    void OnEnable()  => PlayerStatus.OnLocalPlayerSpawned += InitLocalPlayer;
    void OnDisable() => PlayerStatus.OnLocalPlayerSpawned -= InitLocalPlayer;

    void InitLocalPlayer(PlayerStatus ps)
    {
        player_status             = ps;
        sub_noticed_ui_controller = ps.sub_noticed_ui_controller;
        item_menu_controller      = GetComponent<ItemMenuController>();

        // ★FIX サーバー辞書 → ローカル UI を強制同期
        foreach (var kv in belonging_items.ToArray())
            ForceSyncFromServer(kv.Key, kv.Value.item_count);
    }

    //────────────────────────────────────────
    // PUBLIC : アイテム追加（サーバー専用）
    //────────────────────────────────────────
    [Button]
    public void ItemAddToBelonging(ItemStatus _item_status, bool notifyUI = true)
    {
        if (!IsServer) return;      // 追加は常にサーバー側だけ

        if (!belonging_items.ContainsKey(_item_status))
        {
            var entry = new ItemCountWithPermit { item_count = 1 };
            belonging_items.Add(_item_status, entry);

            if (_item_status.item_type != ItemStatus.ItemType.Event && item_menu_controller)
                CreateUI(_item_status, 1);

            if (notifyUI)
                item_menu_controller?.OnItemGetChangeUI(false, _item_status);
        }
        else
        {
            belonging_items[_item_status].item_count++;
            SetItemStatusToItemMenu(_item_status);
            item_menu_controller?.ItemSelectByIndex(
                belonging_item_uis[_item_status].ItemUI.transform.GetSiblingIndex());
        }
    }

    //────────────────────────────────────────
    // PUBLIC : サーバー→クライアント同期
    //────────────────────────────────────────
    public void ForceSyncFromServer(ItemStatus st, int newCount)
    {
        if (!belonging_items.ContainsKey(st))
            belonging_items.Add(st, new ItemCountWithPermit { item_count = newCount });
        else
            belonging_items[st].item_count = newCount;

        if (st.item_type != ItemStatus.ItemType.Event && item_menu_controller)
        {
            if (!belonging_item_uis.ContainsKey(st))
                CreateUI(st, newCount);
            else
                SetItemStatusToItemMenu(st);
        }
    }

    //────────────────────────────────────────
    // PUBLIC : アイテム使用
    //────────────────────────────────────────
    public void ItemUseFromBelonging(ItemStatus item_status)
    {
        int BelongingCountNow = belonging_items[item_status].item_count;

        if (item_status.item_type == ItemStatus.ItemType.CanThrow)
        {
            item_throw_controller.ThrowStart(item_status.ItemThrowPrefab);
            item_throw_controller.item_status = item_status;
            Consume(item_status, BelongingCountNow);
        }
        else if (item_status.item_type == ItemStatus.ItemType.Consumable)
        {
            Consume(item_status, BelongingCountNow);
        }
        else if (item_status.item_type == ItemStatus.ItemType.Equip)
        {
            // 装備系
        }
        else if (item_status.item_type == ItemStatus.ItemType.Key)
        {
            if (cross_hair_event.RaycasstedObj &&
                cross_hair_event.RaycasstedObj.CompareTag("Door"))
            {
                var doorCtrl = cross_hair_event.RaycasstedObj.GetComponent<DoorOpenCloseController>();
                var door     = doorCtrl.the_door_controller;
                if (door.NeededKey.Value && !door.DoorOpenPermitCheck())
                {
                    door.open_permit_by_key_controller.KeyUse();
                    Consume(item_status, BelongingCountNow);
                }
            }
        }
        else if (item_status.item_type == ItemStatus.ItemType.Event)
        {
            Consume(item_status, BelongingCountNow);
        }
        else if (item_status.item_type == ItemStatus.ItemType.EmergencyAttack)
        {
            Consume(item_status, BelongingCountNow);
        }

        GameObject InstantiateItemScriptPrefab = Instantiate(item_status.InstantiateItemScriptPrefab);
        InstantiateItemScriptPrefab.GetComponent<InstantiateItemScript>().item_status = item_status;
    }

    //────────────────────────────────────────
    // PUBLIC : アイテム破棄
    //────────────────────────────────────────
    public void ItemDump(ItemStatus st)
    {
        if (!ExistsItems() || !st.CanItemDump || !belonging_items.ContainsKey(st))
            return;

        var entry = belonging_items[st];
        entry.item_count--;
        sub_noticed_ui_controller.Notice($"{st.ItemName} を捨てた。");

        if (entry.item_count <= 0)
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

    //────────────────────────────────────────
    // PUBLIC : 判定ヘルパー（元コード全て残しています）
    //────────────────────────────────────────
    public bool ExistsItems() => belonging_items.Count > 0;

    public bool ExistBelongingItemByFind(ItemStatus item_status)
    {
        return belonging_items.ContainsKey(item_status);
    }

    public bool ExistItemUIItemStatusByFind(ItemStatus item_status)
    {
        return belonging_item_uis.ContainsKey(item_status);
    }

    public bool WhichCanUseItem(ItemStatus item_status)
    {
        return belonging_items[item_status].CanUse;
    }

    public void CanUseItem(bool can, ItemStatus item_status)
    {
        belonging_items[item_status].CanUse = can;
        item_menu_controller.OnCanItemUseChangeUI(can, item_status);
    }

    public List<ItemStatus> ItemFindByItemType(ItemStatus.ItemType itemType)
    {
        Dictionary<ItemStatus, ItemCountWithPermit> dict =
            belonging_items.Where(x => x.Key.item_type == itemType)
                           .ToDictionary(x => x.Key, x => x.Value);

        List<ItemStatus> list = new();
        foreach (var kv in dict)
            if (itemType == kv.Key.item_type) list.Add(kv.Key);

        return list.Count > 0 ? list : null;
    }

    //────────────────────────────────────────
    // PRIVATE : UI & Consume
    //────────────────────────────────────────
    void CreateUI(ItemStatus st, int cnt)
    {
        GameObject go = Instantiate(ItemUIPrefab, ItemGroup);
        ItemUI iu = go.GetComponent<ItemUI>();
        iu.item_belonging_controller = this;
        iu.item_status               = st;

        var u = new ItemStatusUI {
            ItemIcon        = iu.ItemIcon,
            ItemCountTextUI = iu.ItemCountTextUI,
            ItemUI          = go
        };
        belonging_item_uis.Add(st, u);

        u.ItemIcon.sprite      = st.MenuSprite;
        u.ItemCountTextUI.text = cnt.ToString();
        item_menu_controller.ItemSelectByIndex(go.transform.GetSiblingIndex());
    }

    void SetItemStatusToItemMenu(ItemStatus st)
    {
        belonging_item_uis[st].ItemCountTextUI.text =
            belonging_items[st].item_count.ToString();
    }

    void Consume(ItemStatus st, int currentCount)
    {
        currentCount--;
        belonging_items[st].item_count = currentCount;

        if (currentCount > 0)
        {
            SetItemStatusToItemMenu(st);
        }
        else
        {
            Destroy(belonging_item_uis[st].ItemUI);
            belonging_item_uis.Remove(st);
            belonging_items.Remove(st);
        }
    }
}
