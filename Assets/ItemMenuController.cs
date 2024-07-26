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
public class ItemMenuController : SerializedMonoBehaviour
{
    //アイテムのメニューコントローラー　
    //アイテムをセレクトしたり、アイテムを使用したりする。

    public GameObject ItemGroup;
    public PlayerInput player_input;
    public CanvasGroup ItemCanvasGroup;

    //現在選択しているアイテム。
    public GameObject NowSelectItemUIObject;

    //現在の選択インデックス。
    public int NowItemSelectIndexX;
    private float scroll_value_float;
    private ItemBelongingsController item_belongings_controller;
    private int scroll_value;
    public float WaitHideTimeNow;
    public bool ItemMenuShowUpNow;
    public bool ItemMenuShowUpCommand;
    private void Start()
    {
        item_belongings_controller = this.gameObject.GetComponent<ItemBelongingsController>();
        player_input.currentActionMap["ItemSelectNumber"].performed += OnNumbericKeyPressed;
        
    }
    private void Update()
    {
        //ホイールの値を取得する。
        scroll_value_float = player_input.currentActionMap["ItemSelectWheel"].ReadValue<float>();

        Debug.Log(player_input.currentControlScheme);


        //ホイールの値を1,-1,0で返すようにする為のやつ。
        if (scroll_value_float > 0) scroll_value = 1;
        else if (scroll_value_float == 0) scroll_value = 0;
        else scroll_value = -1;
        

        Debug.Log("VECTOR X : " + scroll_value);
        
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
            //InputSystem.

            if (player_input.currentActionMap["ItemUse"].triggered)
            {
                //アイテム使用ボタンを押したらアイテムを使用する関数を実行。
                ItemUseFromMenu(NowSelectItemUIObject);
            }
            if (scroll_value != 0)
            {
                Debug.Log("scroll_value:::::: ??" + scroll_value);
                //スクロールしている最中なのであれば アイテムをセレクトします。
                
                ItemSelect(NowItemSelectIndexX);


                ItemMenuShowUpCommand = true;
            }
            else
            {
                ItemMenuShowUpCommand = false;
            }
            Debug.Log("ItemMenuShowUpNow: " + ItemMenuShowUpNow);
            Debug.Log("ItemMenuShowUpCommand: " + ItemMenuShowUpCommand);
            if (ItemMenuShowUpNow)
            {
                //アイテムメニューを表示
                ItemMenuShowUp();
                if (!ItemMenuShowUpCommand)
                {
                    //アイテムメニュー表示モードがずーとfalseでそれが10秒間続いてしまったらメニューを閉じる。
                    WaitHideTimeNow += TimeManager.instance.deltaTime;
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
    public void ItemSelect(int ItemIndex)
    {
        //アイテムをセレクトする時の関数。(セレクトしている状態を示すUIやindex、SelectNowGameObjectを更新する。)
        Debug.Log("EventSystem.current.???: " + EventSystem.current.currentSelectedGameObject);
        
        //現在セレクトしているオブジェクトを引数のItemIndexによって取得。 
        //GetChildで何番目のオブジェクトを取得するというコード。
        NowSelectItemUIObject = ItemGroup.transform.GetChild(ItemIndex).gameObject;
        
        //今洗濯しているやつをButtonコンポーネントで取得し、選択する。
        NowSelectItemUIObject.gameObject.GetComponent<Button>().Select();
        
        //変数にIndexも入れておくよ。
        NowItemSelectIndexX = ItemIndex;

        ItemMenuShowUpNow = true;
        //この時間を0にしておくことで選択したら非表示するまでの時間を伸ばせる。
        WaitHideTimeNow = 0;
    }
    private void ItemUseFromMenu(GameObject item_ui_obj)
    {
        //アイテムメニューからアイテムを使う関数。
        ItemUI item_ui = item_ui_obj.GetComponent<ItemUI>();
        ItemStatus item_status = item_ui.item_status;
        //ItemStatusをItemUIクラスから取得する。

        //ItemSelect関数はIndexを指定しなければならないのでGetSiblingIndex(今子オブジェクトの中では何番目にいるのかを教えてくれるやつ)
                                                           //を使用してindexを取得。
        
        ItemSelect(item_ui_obj.transform.GetSiblingIndex());

        //item_statusを参照してitem_belongings_controllerのアイテムを使用する関数を実行。
        item_belongings_controller.ItemUseFromBelonging(item_status);
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
                    ItemSelect(numberKey - 1);
                }
                else
                {
                    ItemSelect(ChildCount - 1);
                }
            }
            else if (numberKey == 0)
            {
                ItemSelect(ChildCount - 1);
            }
        }

    }
}
