using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Linq;
using TMPro;
public class ItemBelongingsController : MonoBehaviour
{
    //アイテムの持ち物を管理するコントローラー。

    //アイテムのUI情報
    public class ItemStatusUI
    {
        public Image ItemIcon;
        public TextMeshProUGUI ItemCountTextUI;
        public GameObject ItemUI;
        public TextMeshProUGUI ItemNameTextUI;
    }
    
    public class ItemCountWithPermit
    {
        public int item_count;
        public bool CanUse = true;
    }
    //アイテム持ち物コントローラー 

    //マックスで持てる数
    public int maxCarryableItems = 6;
    //持っているアイテムの情報。
    public Dictionary<ItemStatus, ItemCountWithPermit> belonging_items = new Dictionary<ItemStatus, ItemCountWithPermit>();
    public Dictionary<ItemStatus, ItemStatusUI> belonging_item_uis = new Dictionary<ItemStatus, ItemStatusUI>();

    //ItemUIのPrefabをセット。 InstantiateしてアイテムUIを作る。
    public GameObject ItemUIPrefab;
    //ItemGroupの子要素として追加する。
    public Transform ItemGroup;

    public CrossHairEvent cross_hair_event;
    [HideInInspector]
    public ItemMenuController item_menu_controller;
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
    // Start is called before the first frame update
    void Start()
    {
        item_throw_controller = this.gameObject.GetComponent<ItemThrowController>();
        item_databases = ItemDatabases.instance;
        enemy_all_controller = EnemyDatabases.instance.gameObject.GetComponent<EnemyAllController>();

    }
    private void InitOnPlayerSpawn(PlayerStatus _playerStatus){
        player_status = _playerStatus;
        sub_noticed_ui_controller = player_status.sub_noticed_ui_controller;
        item_menu_controller = this.gameObject.GetComponent<ItemMenuController>();
        foreach(ItemStatus _itemStatus in InitialItemStatuses){
            ItemAddToBelonging(_itemStatus);
            this.item_menu_controller.OnItemGetChangeUI(false, _itemStatus);

        }
        
    }
    //アイテムをこのコントローラーに追加する関数 外部から実行する。　主にCrossHairEvent.csから
    [Button]
    public void ItemAddToBelonging(ItemStatus _item_status)
    {
        if (!belonging_items.ContainsKey(_item_status))
        {
            ItemCountWithPermit item_count_with_permit = new ItemCountWithPermit();
            item_count_with_permit.item_count = 1;
            item_count_with_permit.CanUse = true;

            belonging_items.Add(_item_status, item_count_with_permit);
            if(_item_status.item_type != ItemStatus.ItemType.Event){
                //イベント用アイテムでなければアイテムを追加
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

                item_status_ui.ItemCountTextUI = item_ui.ItemCountTextUI;
                item_status_ui.ItemUI = item_ui.gameObject;
                //UI情報たちを集めたDictionaryにUI情報のためだけに追加しておく。
                belonging_item_uis.Add(_item_status, item_status_ui);
                //UI専用Dictionaryに入れたらその入れたやつにまた新しく情報を追加。
                
                //所持品には持っていなかったアイテムなのでカウント数は 1 

                //さらに持ってなかった時に取得した場合のみアイテム画像を指定。 1度だけのコールで済む。
                belonging_item_uis[_item_status].ItemIcon.sprite = _item_status.MenuSprite;
                belonging_item_uis[_item_status].ItemCountTextUI.text = "1";
                //アイテムを拾ったらアイテムUIを選択するようにする。
                item_menu_controller.ItemSelectByIndex(item_ui_obj.transform.GetSiblingIndex());
            }
        }
        else
        {
            //アイテムメニューにアイテムが存在するならば。
            
            //指定したアイテムの持っている数を取得。
            int BelongingCountNow = belonging_items[_item_status].item_count;
            
            //アイテムを"手に入れる"なのでアイテム数を１増やす。
            BelongingCountNow += 1;

            belonging_items[_item_status].item_count = BelongingCountNow;
            Debug.Log("SetItemStatusToItemMenu アイテムメニューにアイテムが存在する");
            SetItemStatusToItemMenu(_item_status);

            //アイテムを拾ったらアイテムUIを選択するようにする。
            item_menu_controller.ItemSelectByIndex(belonging_item_uis[_item_status].ItemUI.transform.GetSiblingIndex());
        }
    }
    //コントローラーにあるアイテム所持品からアイテムを消費する関数。
    [Button]
    public void ItemUseFromBelonging(ItemStatus item_status)
    {
        Debug.Log("ItemUseFromBelonging-----");
        int BelongingCountNow = belonging_items[item_status].item_count;
        //アイテムを使用するので   所持数を減らす。

        //アイテムのタイプによって、アイテムを使用したときにイベントを発生させる。
        if (item_status.item_type == ItemStatus.ItemType.CanThrow)
        {
            //投げるタイプのアイテムならば投げます。
            item_throw_controller.ThrowStart(item_status.ItemThrowPrefab);
            item_throw_controller.item_status = item_status;
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
                        //NeededKeyにチェックついてるってことは親かなにかにOpenPermitByKeyがあるので、OpenPermiByeyControllerのKeyUse関数を実行
                        OpenPermitByKeyController open_permit_by_key_controller = the_door_controller.open_permit_by_key_controller;
                        
                        open_permit_by_key_controller.KeyUse();
                        ItemUse(BelongingCountNow, item_status);
                    }
                }
            }

        }else if(item_status.item_type == ItemStatus.ItemType.Event)
        {
            //思い出の品を納品したら
            ItemUse(BelongingCountNow, item_status);
        }
        else if(item_status.item_type == ItemStatus.ItemType.EmergencyAttack)
        {
            //緊急攻撃!

            ItemUse(BelongingCountNow, item_status);
        }
        //スクリプト用Prefabを生成する。 数秒で消えるやつ
        GameObject InstantiateItemScriptPrefab = Instantiate(item_status.InstantiateItemScriptPrefab);
        InstantiateItemScriptPrefab.GetComponent<InstantiateItemScript>().item_status = item_status;
    }

    //アイテムを捨てる
    public void ItemDump(ItemStatus _item_status){
        if (!ExistsItems())
        {
            return;
        }
        //捨てられないアイテムなら捨てません。
        if(_item_status.CanItemDump == false){
            return;
        }
        // 選択中アイテムがnull、あるいは所持品に含まれていなければ何もしない
        if (_item_status == null || !belonging_items.ContainsKey(_item_status))
        {
            return;
        }

        // 所持数を1減らす
        int currentCount = belonging_items[_item_status].item_count;
        currentCount--;

        // UIなどで「捨てた」ことを通知したい場合はここで
        sub_noticed_ui_controller.Notice($"{_item_status.ItemName} を捨てた。");

        // もし所持数が 0 になれば辞書から削除する
        if (currentCount <= 0)
        {
            item_menu_controller.item_equip_controller.EquipChange(_item_status, false);
            // UIアイコンを破壊してから辞書から削除
            if (belonging_items.Count > 1)
            {
                item_menu_controller.ItemSelectAuto(_item_status);
            }
            Destroy(belonging_item_uis[_item_status].ItemUI);
            belonging_item_uis.Remove(_item_status);
            belonging_items.Remove(_item_status);
            // 何かワールド上にアイテムをスポーンさせたければここで
            // Instantiate(...);

            // まだ他のアイテムを持っていれば一番前のアイテムを再選択する（例）

            // else
            // {
            //     // アイテムが0個なら選択中アイテムを null にしておく
            //     item_menu_controller.NowSelectItemUIObject = null;
            // }
        }
        else
        {
            // 数を減らしただけの場合は、UIの表示だけ更新しておく
            belonging_items[_item_status].item_count = currentCount;
            SetItemStatusToItemMenu(_item_status);
        }
        // ワールド上に落とす処理が必要ならここ
        Instantiate(_item_status.ItemPutObjectPrefab, player_status.HeadPos.position, Quaternion.identity);
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
    public bool ExistBelongingItemByFind(ItemStatus item_status){
        if (belonging_items.ContainsKey(item_status))
        {
            return true;
        }else{
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
    public bool WhichCanUseItem(ItemStatus _item_status)
    {
        if (belonging_items[_item_status].CanUse)
        {
            return true;
        }
        return false;
    }
    public void CanUseItem(bool can, ItemStatus _item_status)
    {
        belonging_items[_item_status].CanUse = can;
        item_menu_controller.OnCanItemUseChangeUI(can, _item_status);
    }
    public List<ItemStatus> ItemFindByItemType(ItemStatus.ItemType _item_type)
    {
        //var item_type_belonging_items = belonging_items.Where(x => x.Key.item_type == ItemStatus.ItemType.Equip);
        Dictionary<ItemStatus, ItemCountWithPermit> itemsDictionary = belonging_items
            .Where(x => x.Key.item_type == _item_type)
            .ToDictionary(x => x.Key, x => x.Value);

        List<ItemStatus> founded_items = new List<ItemStatus>();
        foreach (KeyValuePair<ItemStatus, ItemCountWithPermit> item in itemsDictionary)
        {
            if(_item_type == item.Key.item_type)
            {
                founded_items.Add(item.Key);
            }
        }
        if (founded_items.Count > 0)
        {
            return founded_items;
        }
        else
        {
            return null;
        }
    }
    private void  ItemUse(int BelongingCountNow, ItemStatus item_status)
    {
        BelongingCountNow -= 1;
        belonging_items[item_status].item_count = BelongingCountNow;

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
        int ItemCount = belonging_items[item_status].item_count;

        //アイテムのUI専用DictionaryからTextUIを取得し、個数をテキストで表示させる。
        TextMeshProUGUI item_count_text_ui = belonging_item_uis[item_status].ItemCountTextUI;
        Debug.Log("ItemCount-----:??? " + ItemCount);
        if (ItemCount > 0)
        {
            //item_icon.sprite = item_status.MenuSprite;
            item_count_text_ui.text = ItemCount.ToString();
        }
        else
        {
            //アイテムを使用したのち、-1となるとアイテムをこのアイテムメニューから完全消去。
            Destroy(belonging_item_uis[item_status].ItemUI);
            belonging_item_uis.Remove(item_status);

        }
    }

}
