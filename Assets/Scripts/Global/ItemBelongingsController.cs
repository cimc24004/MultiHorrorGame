using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Sirenix.OdinInspector;

public class ItemBelongingsController : NetworkBehaviour
{
    /*──────────────── 内部構造体 ────────────────*/
    public class ItemStatusUI
    {
        public Image           ItemIcon;
        public TextMeshProUGUI ItemCountTextUI;
        public GameObject      ItemUI;
    }
    public class ItemCountWithPermit
    {
        public int  item_count;
        public bool CanUse = true;
    }

    /*──────────────── Inspector ────────────────*/
    public int maxCarryableItems = 6;
    public GameObject ItemUIPrefab;
    public Transform  ItemGroup;

    public CrossHairEvent cross_hair_event;
    [HideInInspector] public ItemMenuController item_menu_controller;

    [SerializeField] private List<ItemStatus> InitialItemStatuses;

    /*──────────────── ランタイム ────────────────*/
    public Dictionary<ItemStatus, ItemCountWithPermit> belonging_items    = new();
    public Dictionary<ItemStatus, ItemStatusUI>        belonging_item_uis = new();

    private ItemThrowController     item_throw_controller;
    private PlayerStatus            player_status;
    private SubNoticedUIController  sub_noticed_ui_controller;

    /*―― DB 遅延対策 ――*/
    private readonly Queue<(uint gid, int cnt)> _pendingInit = new();
    private bool _dbReady;

    /*──────────────── Unity Events ────────────────*/
    void OnEnable()  => PlayerStatus.OnLocalPlayerSpawned += InitOnPlayerSpawn;
    void OnDisable() => PlayerStatus.OnLocalPlayerSpawned -= InitOnPlayerSpawn;

    void Start()
    {
        item_throw_controller = GetComponent<ItemThrowController>();
    }

    void Update()
    {
        if (!_dbReady && ItemDatabases.instance != null)
        {
            _dbReady = true;
            while (_pendingInit.Count > 0)
            {
                var (gid, cnt) = _pendingInit.Dequeue();
                var st = ItemDatabases.instance.GetStatusByGlobalId(gid);
                if (st) ForceSyncFromServer(st, cnt);
            }
        }
    }

    /*──────────────── 初期化 ────────────────*/
    private void InitOnPlayerSpawn(PlayerStatus ps)
    {
        player_status             = ps;
        sub_noticed_ui_controller = player_status.sub_noticed_ui_controller;
        item_menu_controller      = GetComponent<ItemMenuController>();

        if (IsServer)
        {
            GiveInitialItems();
            SendInitialItemsClientRpc(
                InitialItemStatuses.Select(s => s.GlobalId).ToArray());
        }
    }

    private void GiveInitialItems()
    {
        foreach (var st in InitialItemStatuses)
        {
            ItemAddToBelonging(st);
            item_menu_controller?.OnItemGetChangeUI(false, st);
        }
    }

    [ClientRpc]
    private void SendInitialItemsClientRpc(uint[] gids, ClientRpcParams _ = default)
    {
        if (IsServer) return;

        if (!_dbReady)
        {
            foreach (var gid in gids) _pendingInit.Enqueue((gid, 1));
            return;
        }

        foreach (var gid in gids)
        {
            var st = ItemDatabases.instance.GetStatusByGlobalId(gid);
            if (st) ForceSyncFromServer(st, 1);
            else    Debug.LogWarning($"[Init] Unknown gid {gid}");
        }
    }

    /*──────────────── アイテム追加 ────────────────*/
    public void ItemAddToBelonging(ItemStatus st)
    {
        if (NetworkManager.Singleton && !NetworkManager.Singleton.IsServer) return;

        if (!belonging_items.TryGetValue(st, out var entry))
        {
            entry = new ItemCountWithPermit { item_count = 1 };
            belonging_items.Add(st, entry);

            if (st.item_type != ItemStatus.ItemType.Event && item_menu_controller)
            {
                GameObject uiObj = Instantiate(ItemUIPrefab, ItemGroup);
                ItemUI iu = uiObj.GetComponent<ItemUI>();
                iu.item_belonging_controller = this;
                iu.item_status               = st;

                var ui = new ItemStatusUI {
                    ItemIcon        = iu.ItemIcon,
                    ItemCountTextUI = iu.ItemCountTextUI,
                    ItemUI          = iu.gameObject
                };
                belonging_item_uis.Add(st, ui);

                ui.ItemIcon.sprite      = st.MenuSprite;
                ui.ItemCountTextUI.text = "1";
                item_menu_controller.ItemSelectByIndex(uiObj.transform.GetSiblingIndex());
            }
            return;
        }

        entry.item_count++;
        if (item_menu_controller) SetItemStatusToItemMenu(st);
    }

    /*──────────────── 同期ヘルパ ────────────────*/
    public void ForceSyncFromServer(ItemStatus st, int newCount)
    {
        if (st == null) return;

        if (!belonging_items.TryGetValue(st, out var entry))
        {
            entry = new ItemCountWithPermit { item_count = newCount };
            belonging_items.Add(st, entry);

            if (st.item_type != ItemStatus.ItemType.Event && item_menu_controller)
            {
                GameObject uiObj = Instantiate(ItemUIPrefab, ItemGroup);
                ItemUI iu = uiObj.GetComponent<ItemUI>();
                iu.item_belonging_controller = this;
                iu.item_status               = st;

                var ui = new ItemStatusUI {
                    ItemIcon        = iu.ItemIcon,
                    ItemCountTextUI = iu.ItemCountTextUI,
                    ItemUI          = iu.gameObject
                };
                belonging_item_uis.Add(st, ui);

                ui.ItemIcon.sprite      = st.MenuSprite;
                ui.ItemCountTextUI.text = newCount.ToString();
                item_menu_controller.ItemSelectByIndex(uiObj.transform.GetSiblingIndex());
            }
            return;
        }

        entry.item_count = newCount;
        if (item_menu_controller) SetItemStatusToItemMenu(st);
    }

    /*──────────────── 判定ユーティリティ ────────────────*/
    public bool ExistsItems() => belonging_items.Count > 0;
    public bool ExistBelongingItemByFind(ItemStatus st) => belonging_items.ContainsKey(st);
    public bool ExistItemUIItemStatusByFind(ItemStatus st) => belonging_item_uis.ContainsKey(st);
    public bool WhichCanUseItem(ItemStatus st) => belonging_items[st].CanUse;
    public void  CanUseItem(bool can, ItemStatus st)
    {
        belonging_items[st].CanUse = can;
        item_menu_controller?.OnCanItemUseChangeUI(can, st);
    }

    public List<ItemStatus> ItemFindByItemType(ItemStatus.ItemType t)
    {
        return belonging_items
               .Where(kvp => kvp.Key.item_type == t)
               .Select(kvp => kvp.Key)
               .ToList();
    }

    /*──────────────── アイテム使用 ────────────────*/
    public void ItemUseFromBelonging(ItemStatus st)
    {
        if (!belonging_items.TryGetValue(st, out var entry)) return;
        int cnt = entry.item_count;

        switch (st.item_type)
        {
            case ItemStatus.ItemType.CanThrow:
                item_throw_controller.ThrowStart(st.ItemThrowPrefab);
                item_throw_controller.item_status = st;
                ItemUse(cnt, st);
                break;

            case ItemStatus.ItemType.Consumable:
                ItemUse(cnt, st);
                break;

            case ItemStatus.ItemType.Key:
                if (cross_hair_event.RaycasstedObj &&
                    cross_hair_event.RaycasstedObj.CompareTag("Door"))
                {
                    var doorCtrl = cross_hair_event.RaycasstedObj.GetComponent<DoorOpenCloseController>();
                    var door     = doorCtrl.the_door_controller;

                    if (door.NeededKey.Value && !door.DoorOpenPermitCheck())
                    {
                        door.open_permit_by_key_controller.KeyUse();
                        ItemUse(cnt, st);
                    }
                }
                break;

            case ItemStatus.ItemType.Event:
            case ItemStatus.ItemType.EmergencyAttack:
                ItemUse(cnt, st);
                break;
        }

        GameObject p = Instantiate(st.InstantiateItemScriptPrefab);
        p.GetComponent<InstantiateItemScript>().item_status = st;
    }

    /*──────────────── アイテム破棄 ────────────────*/
    public void ItemDump(ItemStatus st)
    {
        if (!ExistsItems() || !st.CanItemDump || !belonging_items.ContainsKey(st))
            return;

        int cnt = --belonging_items[st].item_count;
        sub_noticed_ui_controller.Notice($"{st.ItemName} を捨てた。");

        if (cnt <= 0)
        {
            item_menu_controller.item_equip_controller.EquipChange(st, false);

            if (belonging_items.Count > 1) item_menu_controller.ItemSelectAuto(st);

            Destroy(belonging_item_uis[st].ItemUI);
            belonging_item_uis.Remove(st);
            belonging_items.Remove(st);
        }
        else
        {
            if (item_menu_controller) SetItemStatusToItemMenu(st);
        }

        Instantiate(st.ItemPutObjectPrefab, player_status.HeadPos.position, Quaternion.identity);
    }

    /*──────────────── 内部ヘルパ ────────────────*/
    private void ItemUse(int cntNow, ItemStatus st)
    {
        cntNow--;
        belonging_items[st].item_count = cntNow;

        if (cntNow > 0)
        {
            if (item_menu_controller) SetItemStatusToItemMenu(st);
        }
        else
        {
            if (item_menu_controller) SetItemStatusToItemMenu(st);
            belonging_items.Remove(st);
        }
    }

    private void SetItemStatusToItemMenu(ItemStatus st)
    {
        if (!belonging_item_uis.TryGetValue(st, out var ui) ||
            !belonging_items.TryGetValue(st, out var entry))
            return;

        int cnt = entry.item_count;
        if (cnt > 0)
        {
            ui.ItemCountTextUI.text = cnt.ToString();
        }
        else
        {
            Destroy(ui.ItemUI);
            belonging_item_uis.Remove(st);
        }
    }
}
