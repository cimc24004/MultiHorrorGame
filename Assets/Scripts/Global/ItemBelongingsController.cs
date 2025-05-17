// ItemBelongingsController.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;           // ← 追加
using Sirenix.OdinInspector;
using System.Linq;
using TMPro;
<<<<<<< HEAD
using UnityEngine.UI;
public class ItemBelongingsController : NetworkBehaviour  // ← MonoBehaviour → NetworkBehaviour
{
=======
using Unity.Netcode;

public class ItemBelongingsController : MonoBehaviour
{
    //──────────────── アイテム UI 情報 ────────────────
>>>>>>> c99834a4e5f81487c896c8539c73d8fde6ea283e
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

<<<<<<< HEAD
    public int maxCarryableItems = 6;
    public Dictionary<ItemStatus, ItemCountWithPermit> belonging_items = new Dictionary<ItemStatus, ItemCountWithPermit>();
    public Dictionary<ItemStatus, ItemStatusUI> belonging_item_uis = new Dictionary<ItemStatus, ItemStatusUI>();

    public GameObject ItemUIPrefab;
    public Transform ItemGroup;

    public CrossHairEvent cross_hair_event;
    [HideInInspector] public ItemMenuController item_menu_controller;
    private ItemThrowController item_throw_controller;
=======
    //──────────────── Inspector ────────────────
    public int maxCarryableItems = 6;
    [ShowInInspector] public Dictionary<ItemStatus, ItemCountWithPermit> belonging_items    = new();
    [ShowInInspector] public Dictionary<ItemStatus, ItemStatusUI>        belonging_item_uis = new();
    public GameObject ItemUIPrefab;
    public Transform  ItemGroup;
    public CrossHairEvent cross_hair_event;
    [HideInInspector] public ItemMenuController item_menu_controller;
>>>>>>> c99834a4e5f81487c896c8539c73d8fde6ea283e
    private PlayerStatus player_status;
    private ItemThrowController     item_throw_controller;
    private SubNoticedUIController  sub_noticed_ui_controller;
    private EnemyAllController      enemy_all_controller;
    private ItemDatabases           item_databases;
    [SerializeField] private List<ItemStatus> InitialItemStatuses;

<<<<<<< HEAD
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
=======
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
>>>>>>> c99834a4e5f81487c896c8539c73d8fde6ea283e
    {
        player_status             = ps;
        sub_noticed_ui_controller = ps.sub_noticed_ui_controller;
        item_menu_controller      = GetComponent<ItemMenuController>();

        // ホストだけ最初のアイテムを辞書登録＆クライアント同期
        if (NetworkManager.Singleton.IsServer)
        {
<<<<<<< HEAD
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
=======
            foreach (var st in InitialItemStatuses)
            {
                belonging_items[st] = new ItemCountWithPermit { item_count = 1 };
                var rpcParams = new ClientRpcParams {
                    Send = new ClientRpcSendParams { TargetClientIds = new[] { ps.OwnerClientId } }
                };
                SendItemToClientClientRpc(st, 1, rpcParams);
                item_menu_controller.OnItemGetChangeUI(false, st);
>>>>>>> c99834a4e5f81487c896c8539c73d8fde6ea283e
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
<<<<<<< HEAD
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
=======
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
>>>>>>> c99834a4e5f81487c896c8539c73d8fde6ea283e
    }
}
