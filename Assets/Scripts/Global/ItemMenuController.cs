using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
public class ItemMenuController : SerializedMonoBehaviour
{
    //アイテムのメニューコントローラー　
    //アイテムをセレクトしたり、アイテムを使用したりする。

    public GameObject ItemGroup;
    private PlayerInput player_input;
    public CanvasGroup ItemCanvasGroup;

    //現在選択しているアイテム。
    public GameObject NowSelectItemUIObject;

    //現在の選択インデックス。
    public int NowItemSelectIndexX;
    private float scroll_value_float;
    private ItemBelongingsController item_belongings_controller;
    private int scroll_value;
    private ItemStatus NowSelect_item_status;
    public float WaitHideTimeNow;
    public bool ItemMenuShowUpNow;
    public bool ItemMenuShowUpCommand;

    public bool HaveLight = true;
    [ShowIf("HaveLight", true)]
    [Header("懐中電灯(?)オブジェクトをセット")]
    public GameObject LightObject;
    public TextMeshProUGUI item_description_text_ui;
    public GameObject ItemDescriptionUI;
    
    private bool LightSwitchMode = false;
    private ItemUI before_selected_item_ui;
    [SerializeField] private AudioSource LightOnAudioSource;
    [SerializeField] private AudioSource LightOffAudioSource;
    private void Start()
    {
        player_input = GameControllerOnAllScene.instance.player_input;
        item_belongings_controller = this.gameObject.GetComponent<ItemBelongingsController>();
        player_input.currentActionMap["ItemSelectNumber"].performed += OnNumbericKeyPressed;
    }
    private void Update()
    {
        //ホイールの値を取得する。
        scroll_value_float = player_input.currentActionMap["ItemSelectWheel"].ReadValue<float>();

        if (!item_belongings_controller.ExistsItems())
        {
            ItemDescriptionUI.SetActive(false);
        }
        else
        {
            ItemDescriptionUI.SetActive(true);
        }
        //懐中電灯を持っていたら
        if (HaveLight)
        {
            if (player_input.currentActionMap["LightSwitchAction"].triggered)
            {
                if (!LightSwitchMode)
                {
                    //ライトをつける。
                    LightObject.SetActive(true);
                    LightOnAudioSource.Play();
                    LightSwitchMode = true;
                }
                else
                {
                    //ライトをけす。
                    LightOffAudioSource.Play();
                    LightObject.SetActive(false);
                    LightSwitchMode = false;
                }
            }
        }

        //ホイールの値を1,-1,0で返すようにする為のやつ。
        if (scroll_value_float > 0) scroll_value = 1;
        else if (scroll_value_float == 0) scroll_value = 0;
        else scroll_value = -1;
        
        
        //アイテムを1個以上持っていた場合は？
        if (ItemGroup.transform.childCount > 0)
        {

            if (scroll_value > 0)
            {
                //上にホイールクルクルした場合は
                //Indexを増やす。 
                NowItemSelectIndexX += 1;
                if (ItemGroup.transform.childCount <= NowItemSelectIndexX)
                {
                    //Indexを増やした場合もしアイテム数を超えた場合はいちばん最初のIndexにしておく。
                    NowItemSelectIndexX = 0;
                }
            }
            else if(scroll_value < 0)
            {
                //下にホイールクルクルした場合は
                //Indexを減らす。
                NowItemSelectIndexX -= 1;
                if (NowItemSelectIndexX < 0)
                {
                    //Indexを減らした結果マイナスになった場合は最後のアイテムをセレクトするようにする。
                    NowItemSelectIndexX = ItemGroup.transform.childCount - 1;
                }
            }

 
            if (player_input.currentActionMap["ItemUse"].triggered)
            {
                //アイテム使用ボタンを押したらアイテムを使用する関数を実行。
                if (item_belongings_controller.ExistsItems())
                {
                    if (item_belongings_controller.ExistItemUIItemStatusByFind(NowSelect_item_status))
                    {
                        //選択しているアイテムがあったらアイテムを使用する。
                        if (NowSelect_item_status && NowSelect_item_status.ItemUseFromClick)
                        {
                            ItemUseFromMenu(NowSelectItemUIObject);
                        }
                    }
                }
            }
            if (scroll_value != 0)
            {
                //スクロールしている最中なのであれば アイテムをセレクトします。
                
                ItemSelectByIndex(NowItemSelectIndexX);


                ItemMenuShowUpCommand = true;
            }
            else
            {
                ItemMenuShowUpCommand = false;
            }
            if (ItemMenuShowUpNow)
            {
                //アイテムメニューを表示
                ItemMenuShowUp();
                if (!ItemMenuShowUpCommand)
                {
                    //アイテムメニュー表示モードがずーとfalseでそれが10秒間続いてしまったらメニューを閉じる。
                    WaitHideTimeNow += Time.deltaTime;
                    if (WaitHideTimeNow > 10)
                    {
                        //アイテムメニューを非表示。
                        ItemMenuHide();
                        ItemMenuShowUpNow = false;
                    }
                }
            }
    


        }

    }
    [Button]
    public void ItemMenuShowUp()
    {
        ItemCanvasGroup.alpha = 1;
    }
    [Button]
    public void ItemMenuHide()
    {
        Debug.Log("ItemMenuHide()()()()U)(");
        //ItemCanvasGroup.alpha = 0;
        ItemCanvasGroup.DOFade(0, 0.1f).SetEase(Ease.Linear).SetLink(this.gameObject);
    }
    public void ItemSelectByIndex(int ItemIndex)
    {
        //アイテムをセレクトする時の関数。(セレクトしている状態を示すUIやindex、SelectNowGameObjectを更新する。)
        
        //現在セレクトしているオブジェクトを引数のItemIndexによって取得。 
        //GetChildで何番目のオブジェクトを取得するというコード。
        NowSelectItemUIObject = ItemGroup.transform.GetChild(ItemIndex).gameObject;
        CommonItemSelect(NowSelectItemUIObject);
    }
    public void OnCanItemUseChangeUI(bool can, ItemStatus _item_status)
    {
        //アイテムが使えるようになるときに実行する関数 UIなどの変更も加える必要があるためこの関数を実行すること
    }
    public void ItemSelectByObject(GameObject TargetSelectObj)
    {
        CommonItemSelect(TargetSelectObj);
    }

    private void CommonItemSelect(GameObject selectObj)
    {
        Debug.Log("CommonItemSelect");
        //今洗濯しているやつをButtonコンポーネントで取得し、選択する。
        NowSelectItemUIObject.gameObject.GetComponent<Button>().Select();

        //選択しているアイテムステータスも取得しておく。
        NowSelect_item_status = NowSelectItemUIObject.GetComponent<ItemUI>().item_status;
        
        if(before_selected_item_ui != null)
        {
            //前に選択していたアイテムがあった場合
            before_selected_item_ui.FocusUI.SetActive(false);
        }

        before_selected_item_ui = item_belongings_controller.belonging_item_uis[NowSelect_item_status].ItemUIs.GetComponent<ItemUI>();
        
        before_selected_item_ui.FocusUI.SetActive(true);

        //変数にIndexも入れておくよ。
        NowItemSelectIndexX = selectObj.transform.GetSiblingIndex();

        item_description_text_ui.SetText(NowSelect_item_status.ItemDescription);

        ItemMenuShowUpNow = true;
        //この時間を0にしておくことで選択したら非表示するまでの時間を伸ばせる。
        WaitHideTimeNow = 0;
    }
    public void ItemUseFromMenu(GameObject item_ui_obj)
    {
        //アイテムメニューからアイテムを使う関数。
        ItemUI item_ui = item_ui_obj.GetComponent<ItemUI>();
        //ItemStatusをItemUIクラスから取得する。
        ItemStatus item_status = item_ui.item_status;
        if (!item_belongings_controller.WhichCanUseItem(item_status))
        {
            //この地点でアイテムが使えない状態ならこの関数を実行しない
            Debug.Log($"{item_status.ItemName}というアイテムはロックされていて使えません！");
            return;
        }

        int item_ui_index = item_ui_obj.transform.GetSiblingIndex();
        //ItemSelect関数はIndexを指定しなければならないのでGetSiblingIndex(今子オブジェクトの中では何番目にいるのかを教えてくれるやつ)
        //を使用してindexを取得。
        //アイテムを使用したときはアイテムをセレクトする。
        ItemSelectByIndex(item_ui_index);

        //item_statusを参照してitem_belongings_controllerのアイテムを使用する関数を実行。
        item_belongings_controller.ItemUseFromBelonging(item_status);
        
        if (!item_belongings_controller.belonging_items.ContainsKey(item_status))
        {
            //消費したアイテムがすでに存在しなかったら？？
            //選択していたアイテムの一つ前のアイテムを選択する。
            //一つ前のアイテムがなかった場合(アイテム数が1個だけのとき)は0番目を選択しておく。

            //アイテムが消される前に実行されてしまうのでこのようなコードにしてる。
            if(item_belongings_controller.belonging_items.Count > 0)
            {
                Debug.Log("item_ui_indexの値: " + item_ui_index);
                if ((item_ui_index - 1) >= 0)
                {
                    Debug.Log("item_ui_index-1 >= 0");
                    ItemSelectByIndex(item_ui_index - 1);
                }
                else if ((item_ui_index) == 0)
                {
                    Debug.Log("item_ui_index==0");
                    ItemSelectByIndex(1);
                }
            }


        }
    }
    private void OnNumbericKeyPressed(InputAction.CallbackContext context)
    {
        Debug.Log("KeyName:::::" + context.control.displayName);
        int numberKey = Int32.Parse(context.control.displayName);
        int ChildCount = ItemGroup.transform.childCount;
        if(ChildCount > 0)
        {
            if (numberKey != 0)
            {
                if (numberKey < ChildCount)
                {
                    ItemSelectByIndex(numberKey - 1);
                }
                else
                {
                    ItemSelectByIndex(ChildCount - 1);
                }
            }
            else if (numberKey == 0)
            {
                ItemSelectByIndex(ChildCount - 1);
            }
        }

    }
}
