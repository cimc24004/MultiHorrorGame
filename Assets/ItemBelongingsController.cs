using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Linq;
public class ItemBelongingsController : SerializedMonoBehaviour
{
    public class ItemStatusUI
    {
        public Image ItemIcon;
        public Text ItemCountText;
        public GameObject ItemUIs;
    }
    //アイテム持ち物コントローラー 

    //持っているアイテムの情報。
    public Dictionary<ItemStatus, int> belonging_items = new Dictionary<ItemStatus, int>();
    public Dictionary<ItemStatus, ItemStatusUI> belonging_item_uis = new Dictionary<ItemStatus, ItemStatusUI>();

    public GameObject ItemUIPrefab;
    public Transform ItemGroup;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //アイテムをこのコントローラーに追加する関数 外部から実行する。　主にCrossHairEvent.csから
    [Button]
    public void ItemAddToBelonging(ItemStatus item_status)
    {
        if (!belonging_items.ContainsKey(item_status))
        {
            GameObject item_ui_obj = Instantiate(ItemUIPrefab, ItemGroup.transform);
            ItemUI item_ui = item_ui_obj.GetComponent<ItemUI>();
            
            item_ui.item_belonging_controller = this;

            ItemStatusUI item_status_ui = new ItemStatusUI();
            item_status_ui.ItemIcon = item_ui.ItemIcon;
            item_status_ui.ItemCountText = item_ui.ItemCountText;
            item_status_ui.ItemUIs = item_ui.gameObject;

            belonging_item_uis.Add(item_status, item_status_ui);

            belonging_item_uis[item_status].ItemIcon.sprite = item_status.MenuSprite;
            belonging_item_uis[item_status].ItemCountText.text = "1";

            belonging_items.Add(item_status, 1);
        }
        else
        {
            int BelongingCountNow = belonging_items[item_status];
            BelongingCountNow += 1;
            belonging_items[item_status] = BelongingCountNow;

            SetItemStatusToItemMenu(item_status);
        }
    }
    //コントローラーにあるアイテム所持品からアイテムを消費する関数。
    [Button]
    public void ItemUseFromBelonging(ItemStatus item_status)
    {
        int BelongingCountNow = belonging_items[item_status];
        //アイテムを使用するので   所持数を減らす。
        BelongingCountNow -= 1;
        belonging_items[item_status] = BelongingCountNow;
        //使用したらItemMenuにあるこのアイテムの情報を更新。
        if (BelongingCountNow > 0)
        {
            SetItemStatusToItemMenu(item_status);
        }
        else
        {
            //所持数が0になったらアイテムメニューから消す。
            SetItemStatusToItemMenu(item_status);
            belonging_items.Remove(item_status);
        }
    }

    private void SetItemStatusToItemMenu(ItemStatus item_status)
    {
        int ItemCount = belonging_items[item_status];
        Image item_icon = belonging_item_uis[item_status].ItemIcon;
        Text item_text = belonging_item_uis[item_status].ItemCountText;
        Debug.Log("ItemCount-----:??? " + ItemCount);
        if (ItemCount > 0)
        {
            item_icon.sprite = item_status.MenuSprite;
            item_text.text = ItemCount.ToString();
        }
        else
        {
            Destroy(belonging_item_uis[item_status].ItemUIs);
            belonging_item_uis.Remove(item_status);

        }
    }
}
