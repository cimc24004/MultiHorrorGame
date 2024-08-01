using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Linq;
public class ItemBelongingsController : SerializedMonoBehaviour
{
    //アイテムの持ち物を管理するコントローラー。

    //アイテムのUI情報
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

    //ItemUIのPrefabをセット。 InstantiateしてアイテムUIを作る。
    public GameObject ItemUIPrefab;
    //ItemGroupの子要素として追加する。
    public Transform ItemGroup;

    public CrossHairEvent cross_hair_event;
    private ItemMenuController item_menu_controller;
    private ItemThrowController item_throw_controller;

    // Start is called before the first frame update
    void Start()
    {
        item_menu_controller = this.gameObject.GetComponent<ItemMenuController>();
        item_throw_controller = this.gameObject.GetComponent<ItemThrowController>();
    }

    //アイテムをこのコントローラーに追加する関数 外部から実行する。　主にCrossHairEvent.csから
    [Button]
    public void ItemAddToBelonging(ItemStatus _item_status)
    {
        if (!belonging_items.ContainsKey(_item_status))
        {
            //所持品に取得したアイテムがなければアイテムメニューに追加する。
            GameObject item_ui_obj = Instantiate(ItemUIPrefab, ItemGroup.transform);
            //アイテムメニューに追加するにはPrefabをInstanteateする。


            //ItemUIとはアイテムメニューにあるアイテムのUI。

            ItemUI item_ui = item_ui_obj.GetComponent<ItemUI>();
            
            item_ui.item_belonging_controller = this;
            item_ui.item_status = _item_status;
            
            //item_uiにUI情報を入れておく。
            ItemStatusUI item_status_ui = new ItemStatusUI();
            item_status_ui.ItemIcon = item_ui.ItemIcon;
            item_status_ui.ItemCountText = item_ui.ItemCountText;
            item_status_ui.ItemUIs = item_ui.gameObject;

            //UI情報たちを集めたDictionaryにUI情報のためだけに追加しておく。
            belonging_item_uis.Add(_item_status, item_status_ui);



            //UI専用Dictionaryに入れたらその入れたやつにまた新しく情報を追加。
            
            //所持品には持っていなかったアイテムなのでカウント数は 1 

            //さらに持ってなかった時に取得した場合のみアイテム画像を指定。 1度だけのコールで済む。
            belonging_item_uis[_item_status].ItemIcon.sprite = _item_status.MenuSprite;
            belonging_item_uis[_item_status].ItemCountText.text = "1";

            belonging_items.Add(_item_status, 1);

            //アイテムを拾ったらアイテムUIを選択するようにする。
            Debug.Log(item_ui_obj.gameObject);
            item_menu_controller.ItemSelectByIndex(item_ui_obj.transform.GetSiblingIndex());
        }
        else
        {
            //アイテムメニューにアイテムが存在するならば。
            
            //指定したアイテムの持っている数を取得。
            int BelongingCountNow = belonging_items[_item_status];
            
            //アイテムを"手に入れる"なのでアイテム数を１増やす。
            BelongingCountNow += 1;

            belonging_items[_item_status] = BelongingCountNow;
            Debug.Log("SetItemStatusToItemMenu アイテムメニューにアイテムが存在する");
            SetItemStatusToItemMenu(_item_status);

            //アイテムを拾ったらアイテムUIを選択するようにする。
            item_menu_controller.ItemSelectByIndex(belonging_item_uis[_item_status].ItemUIs.transform.GetSiblingIndex());
        }
    }
    //コントローラーにあるアイテム所持品からアイテムを消費する関数。
    [Button]
    public void ItemUseFromBelonging(ItemStatus item_status)
    {
        Debug.Log("ItemUseFromBelonging-----");
        int BelongingCountNow = belonging_items[item_status];
        //アイテムを使用するので   所持数を減らす。

        //アイテムのタイプによって、アイテムを使用したときにイベントを発生させる。
        if (item_status.item_type == ItemStatus.ItemType.CanThrow)
        {
            //投げるタイプのアイテムならば投げます。
            item_throw_controller.ThrowStart(item_status.ItemObjectPrefab);
            
            //使用したらItemMenuにあるこのアイテムUIの情報を更新。
            ItemUse(BelongingCountNow, item_status);
        }
        else if (item_status.item_type == ItemStatus.ItemType.Consumable)
        {
            //消費アイテムなら
            ItemUse(BelongingCountNow, item_status);

        }
        else if (item_status.item_type == ItemStatus.ItemType.Equip)
        {
            //装備アイテムなら(懐中電灯など)

        }
        else if (item_status.item_type == ItemStatus.ItemType.Key)
        {
            Debug.Log("RaycasstedObj: " + cross_hair_event.RaycasstedObj);
            if(cross_hair_event.RaycasstedObj != null)
            {
                if (cross_hair_event.RaycasstedObj.tag == "Door")
                {
                    //鍵アイテムを使用ボタンを押すとまずクロスヘアからのレイキャストがドアに当たっているかを確認し、
                    //そのドアのThedoorControllerを取得。 そしてNeedKeyがTrueでかつ、OpenPermitがfalseの時だけ鍵を使用する。
                    //要はドアの鍵は必要。かつドアの鍵がすでに空いていた場合は鍵を消費。
                    DoorOpenCloseController door_open_close_controller = cross_hair_event.RaycasstedObj.GetComponent<DoorOpenCloseController>();
                    TheDoorController the_door_controller = door_open_close_controller.the_door_controller;
                    if (the_door_controller.NeededKey && !the_door_controller.DoorOpenPermitCheck())
                    {
                        the_door_controller.DoorOpenDOPermit();
                        ItemUse(BelongingCountNow, item_status);
                    }
                }
            }

        }

    }
    public bool ExistsItems()
    {
        //一つでもアイテムを持っていたらtrueを返すやつ。
        if(belonging_items.Count > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool ExistItemUIItemStatusByFind(ItemStatus item_status)
    {
        //アイテムメニューに指定したItemStatusが存在しない場合。
        if (belonging_item_uis.ContainsKey(item_status))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private void  ItemUse(int BelongingCountNow, ItemStatus item_status)
    {
        BelongingCountNow -= 1;
        belonging_items[item_status] = BelongingCountNow;

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
        //アイテム数が減ったとき。 アイテム数が増えた時(すでに持っている場合の時だけ)に実行する関数


        //アイテム数を取得。
        int ItemCount = belonging_items[item_status];

        //アイテムのUI専用DictionaryからTextUIを取得し、個数をテキストで表示させる。
        Text item_text = belonging_item_uis[item_status].ItemCountText;
        Debug.Log("ItemCount-----:??? " + ItemCount);
        if (ItemCount > 0)
        {
            //item_icon.sprite = item_status.MenuSprite;
            item_text.text = ItemCount.ToString();
        }
        else
        {
            //アイテムを使用したのち、-1となるとアイテムをこのアイテムメニューから完全消去。
            Destroy(belonging_item_uis[item_status].ItemUIs);
            belonging_item_uis.Remove(item_status);

        }
    }
}
