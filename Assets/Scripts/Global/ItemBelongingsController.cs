using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

/// <summary>
/// プレイヤーが所持しているアイテムをまとめて管理するコンポーネント。
///   ・サーバー側：所持数の真実を保持  
///   ・クライアント側：UI だけ更新  
/// ※NetworkBehaviour 継承に注意
/// </summary>
public class ItemBelongingsController : NetworkBehaviour
{
    /*────────────────────────────────────────────
     * 内部構造体
     *───────────────────────────────────────────*/
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

    /*────────────────────────────────────────────
     * インスペクター
     *───────────────────────────────────────────*/
    [Header("一度に持てるアイテムの最大種類数")]
    public int maxCarryableItems = 6;

    [Header("UI 用プレハブ & 親")]
    public GameObject ItemUIPrefab;
    public Transform  ItemGroup;

    [Header("開始時に強制で持つアイテム")]
    [SerializeField] private List<ItemStatus> InitialItemStatuses = new();

    /*────────────────────────────────────────────
     * 依存コンポーネント & 変数
     *───────────────────────────────────────────*/
    public CrossHairEvent         cross_hair_event;
    [HideInInspector] public ItemMenuController   item_menu_controller;

    private ItemThrowController   item_throw_controller;
    private PlayerStatus          player_status;
    private SubNoticedUIController sub_noticed_ui_controller;
    private ItemDatabases         item_databases;

    public  Dictionary<ItemStatus, ItemCountWithPermit> belonging_items    = new();
    public  Dictionary<ItemStatus, ItemStatusUI>        belonging_item_uis = new();

    /*────────────────────────────────────────────
     * イベント購読
     *───────────────────────────────────────────*/
    void OnEnable()  => PlayerStatus.OnLocalPlayerSpawned += InitOnPlayerSpawn;
    void OnDisable() => PlayerStatus.OnLocalPlayerSpawned -= InitOnPlayerSpawn;

    void Start()
    {
        item_throw_controller = GetComponent<ItemThrowController>();
        item_databases        = ItemDatabases.instance;
    }

    /*────────────────────────────────────────────
     * プレイヤー生成後に初期化
     *───────────────────────────────────────────*/
    private void InitOnPlayerSpawn(PlayerStatus ps)
    {
        player_status             = ps;
        sub_noticed_ui_controller = player_status.sub_noticed_ui_controller;
        item_menu_controller      = GetComponent<ItemMenuController>();

        if (IsServer)
        {
            // サーバーが真実の所持リストを作り、
            // その内容を全クライアントに流す
            foreach (var st in InitialItemStatuses)
            {
                ItemAddToBelonging(st);
            }
            SendInitialItemsClientRpc(
                InitialItemStatuses.Select(s => s.GlobalId).ToArray());
        }
    }

    [ClientRpc]
    private void SendInitialItemsClientRpc(uint[] gids, ClientRpcParams _ = default)
    {
        if (IsServer) return;   // ホストは既に処理済み

        foreach (uint gid in gids)
        {
            var st = item_databases.GetStatusByGlobalId(gid);
            ForceSyncFromServer(st, 1);
        }
    }

    /*────────────────────────────────────────────
     * アイテム追加（サーバー専用）
     *───────────────────────────────────────────*/
    [Button]
    public void ItemAddToBelonging(ItemStatus st)
    {
        // クライアント側では直接いじらない
        if (NetworkManager.Singleton && !NetworkManager.Singleton.IsServer) return;

        if (!belonging_items.TryGetValue(st, out var entry))
        {
            entry = new ItemCountWithPermit { item_count = 1, CanUse = true };
            belonging_items.Add(st, entry);

            // UI はローカル・オーナーにだけ生成
            if (st.item_type != ItemStatus.ItemType.Event && item_menu_controller)
                CreateOrUpdateUI(st, 1);
        }
        else
        {
            entry.item_count += 1;
            if (item_menu_controller) SetItemStatusToItemMenu(st);
        }
    }

    /*────────────────────────────────────────────
     * アイテム消費 / 使用 など（サーバー実行）
     *───────────────────────────────────────────*/
    [Button]
    public void ItemUseFromBelonging(ItemStatus st)
    {
        if (!belonging_items.TryGetValue(st, out var entry)) return;

        entry.item_count--;
        if (entry.item_count <= 0)
        {
            belonging_items.Remove(st);
            if (item_menu_controller && belonging_item_uis.ContainsKey(st))
            {
                Destroy(belonging_item_uis[st].ItemUI);
                belonging_item_uis.Remove(st);
            }
        }
        else
        {
            if (item_menu_controller) SetItemStatusToItemMenu(st);
        }
    }

    public void ItemDump(ItemStatus st)
    {
        if (!belonging_items.TryGetValue(st, out var entry)) return;
        if (!st.CanItemDump) return;

        entry.item_count--;
        sub_noticed_ui_controller.Notice($"{st.ItemName} を捨てた。");

        if (entry.item_count <= 0)
        {
            belonging_items.Remove(st);
            if (item_menu_controller && belonging_item_uis.ContainsKey(st))
            {
                Destroy(belonging_item_uis[st].ItemUI);
                belonging_item_uis.Remove(st);
            }
        }
        else
        {
            if (item_menu_controller) SetItemStatusToItemMenu(st);
        }

        // ワールドに落とす
        Instantiate(st.ItemPutObjectPrefab, player_status.HeadPos.position, Quaternion.identity);
    }

    /*────────────────────────────────────────────
     * クライアント同期専用
     *───────────────────────────────────────────*/
    public void ForceSyncFromServer(ItemStatus st, int newCount)
    {
        if (!belonging_items.TryGetValue(st, out var entry))
        {
            // 初追加
            entry = new ItemCountWithPermit { item_count = newCount, CanUse = true };
            belonging_items.Add(st, entry);

            if (st.item_type != ItemStatus.ItemType.Event && item_menu_controller)
                CreateOrUpdateUI(st, newCount);
        }
        else
        {
            entry.item_count = newCount;
            if (item_menu_controller) SetItemStatusToItemMenu(st);
        }
    }

    /*────────────────────────────────────────────
     * UI ヘルパ
     *───────────────────────────────────────────*/
    private void CreateOrUpdateUI(ItemStatus st, int count)
    {
        if (!belonging_item_uis.ContainsKey(st))
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
        }

        belonging_item_uis[st].ItemIcon.sprite      = st.MenuSprite;
        belonging_item_uis[st].ItemCountTextUI.text = count.ToString();
        item_menu_controller.ItemSelectByIndex(
            belonging_item_uis[st].ItemUI.transform.GetSiblingIndex());
    }

    private void SetItemStatusToItemMenu(ItemStatus st)
    {
        if (!belonging_item_uis.TryGetValue(st, out var ui)) return;
        ui.ItemCountTextUI.text = belonging_items[st].item_count.ToString();
    }

    /*────────────────────────────────────────────
     * 補助系
     *───────────────────────────────────────────*/
    public bool ExistsItems()                               => belonging_items.Count > 0;
    public bool WhichCanUseItem(ItemStatus st)              => belonging_items[st].CanUse;
    public void  CanUseItem(bool can, ItemStatus st)
    {
        belonging_items[st].CanUse = can;
        if (item_menu_controller) item_menu_controller.OnCanItemUseChangeUI(can, st);
    }

    public List<ItemStatus> ItemFindByItemType(ItemStatus.ItemType t)
        => belonging_items.Where(x => x.Key.item_type == t).Select(x => x.Key).ToList();
}
