// ItemBelongingsController.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Linq;
using TMPro;
using Unity.Netcode;

public class ItemBelongingsController : MonoBehaviour
{
    //──────────────── アイテム UI 情報 ────────────────
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

    //──────────────── Inspector ────────────────
    public int maxCarryableItems = 6;
    [ShowInInspector] public Dictionary<ItemStatus, ItemCountWithPermit> belonging_items    = new();
    [ShowInInspector] public Dictionary<ItemStatus, ItemStatusUI>        belonging_item_uis = new();
    public GameObject ItemUIPrefab;
    public Transform  ItemGroup;
    public CrossHairEvent cross_hair_event;
    [HideInInspector] public ItemMenuController item_menu_controller;
    private PlayerStatus player_status;
    private ItemThrowController     item_throw_controller;
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
        enemy_all_controller  = EnemyDatabases.instance.GetComponent<EnemyAllController>();
    }

    private void InitOnPlayerSpawn(PlayerStatus ps)
    {
        player_status             = ps;
        sub_noticed_ui_controller = ps.sub_noticed_ui_controller;
        item_menu_controller      = GetComponent<ItemMenuController>();

        // ホストだけ最初のアイテムを辞書登録＆クライアント同期
        if (NetworkManager.Singleton.IsServer)
        {
            foreach (var st in InitialItemStatuses)
            {
                belonging_items[st] = new ItemCountWithPermit { item_count = 1 };
                var rpcParams = new ClientRpcParams {
                    Send = new ClientRpcSendParams { TargetClientIds = new[] { ps.OwnerClientId } }
                };
                SendItemToClientClientRpc(st, 1, rpcParams);
                item_menu_controller.OnItemGetChangeUI(false, st);
            }
        }
    }

    //──────────────── サーバー→クライアント同期 ────────────────
    [ClientRpc]
    void SendItemToClientClientRpc(ItemStatus st, int newCount, ClientRpcParams rpcParams = default)
    {
        ForceSyncFromServer(st, newCount);
    }

    //──────────────── アイテム追加 ────────────────
    public void ItemAddToBelonging(ItemStatus st, bool notifyUI = true)
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            if (!belonging_items.ContainsKey(st))
                belonging_items[st] = new ItemCountWithPermit { item_count = 1 };
            else
                belonging_items[st].item_count++;

            int newCount = belonging_items[st].item_count;
            var rpcParams = new ClientRpcParams {
                Send = new ClientRpcSendParams { TargetClientIds = new[] { NetworkManager.Singleton.LocalClientId } }
            };
            SendItemToClientClientRpc(st, newCount, rpcParams);

            if (notifyUI)
                item_menu_controller.OnItemGetChangeUI(false, st);
        }
    }

    //──────────────── サーバー→クライアント強制同期 ────────────────
    public void ForceSyncFromServer(ItemStatus st, int newCount)
    {
        if (!belonging_items.ContainsKey(st))
            belonging_items[st] = new ItemCountWithPermit { item_count = newCount };
        else
            belonging_items[st].item_count = newCount;

        if (!belonging_item_uis.ContainsKey(st))
        {
            var go = Instantiate(ItemUIPrefab, ItemGroup);
            var iu = go.GetComponent<ItemUI>();
            iu.item_belonging_controller = this;
            iu.item_status               = st;

            var info = new ItemStatusUI {
                ItemIcon        = iu.ItemIcon,
                ItemCountTextUI = iu.ItemCountTextUI,
                ItemUI          = go,
            };
            belonging_item_uis[st] = info;
            info.ItemIcon.sprite = st.MenuSprite;
        }

        belonging_item_uis[st].ItemCountTextUI.text =
            belonging_items[st].item_count.ToString();

        item_menu_controller.ItemSelectByIndex(
            belonging_item_uis[st].ItemUI.transform.GetSiblingIndex()
        );
    }

    //──────────────── アイテム使用 ────────────────
    public void ItemUseFromBelonging(ItemStatus st)
    {
        int cnt = belonging_items[st].item_count;

        if (st.item_type == ItemStatus.ItemType.CanThrow)
        {
            item_throw_controller.ThrowStart(st.ItemThrowPrefab);
            item_throw_controller.item_status = st;
            Consume(st, cnt);
        }
        else if (st.item_type == ItemStatus.ItemType.Consumable)
        {
            Consume(st, cnt);
        }
        else if (st.item_type == ItemStatus.ItemType.Key)
        {
            if (cross_hair_event.RaycasstedObj &&
                cross_hair_event.RaycasstedObj.CompareTag("Door"))
            {
                var dCtrl = cross_hair_event.RaycasstedObj.GetComponent<DoorOpenCloseController>();
                var door  = dCtrl.the_door_controller;
                if (door.NeededKey.Value && !door.DoorOpenPermitCheck())
                {
                    door.open_permit_by_key_controller.KeyUse();
                    Consume(st, cnt);
                }
            }
        }
        else if (st.item_type == ItemStatus.ItemType.Event ||
                 st.item_type == ItemStatus.ItemType.EmergencyAttack)
        {
            Consume(st, cnt);
        }

        var obj = Instantiate(st.InstantiateItemScriptPrefab);
        obj.GetComponent<InstantiateItemScript>().item_status = st;
    }

    //──────────────── アイテム破棄 ────────────────
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

    //──────────────── 判定ヘルパー ────────────────
    public bool ExistsItems() => belonging_items.Count > 0;
    public bool ExistBelongingItemByFind(ItemStatus st) => belonging_items.ContainsKey(st);
    public bool ExistItemUIItemStatusByFind(ItemStatus st) => belonging_item_uis.ContainsKey(st);
    public bool WhichCanUseItem(ItemStatus st) => belonging_items[st].CanUse;
    public void CanUseItem(bool can, ItemStatus st)
    {
        belonging_items[st].CanUse = can;
        item_menu_controller.OnCanItemUseChangeUI(can, st);
    }
    public List<ItemStatus> ItemFindByItemType(ItemStatus.ItemType t) =>
        belonging_items.Where(kvp => kvp.Key.item_type == t).Select(kvp => kvp.Key).ToList();

    //──────────────── 内部ヘルパー ────────────────
    void Consume(ItemStatus st, int cntNow)
    {
        cntNow--;
        belonging_items[st].item_count = cntNow;

        if (cntNow > 0)
            SetItemStatusToItemMenu(st);
        else
        {
            Destroy(belonging_item_uis[st].ItemUI);
            belonging_item_uis.Remove(st);
            belonging_items.Remove(st);
        }
    }

    void SetItemStatusToItemMenu(ItemStatus st)
    {
        belonging_item_uis[st].ItemCountTextUI.text =
            belonging_items[st].item_count.ToString();
    }
}
