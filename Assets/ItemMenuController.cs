using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
public class ItemMenuController : MonoBehaviour
{
    public GameObject ItemGroup;
    public PlayerInput player_input;
    public CanvasGroup ItemCanvasGroup;

    public GameObject NowSelectItemUI;

    private Vector2 item_select_vector;
    public int NowItemSelectIndexX;
    private float scroll_value_float;
    private int scroll_value;
    private bool once;
    private void Update()
    {
        //Vector2 value = player_input.currentActionMap["ItemSelect"].ReadValue<>
        scroll_value_float = player_input.currentActionMap["ItemSelect"].ReadValue<float>();
        if (scroll_value_float > 0) scroll_value = 1;
        else if (scroll_value_float == 0) scroll_value = 0;
        else scroll_value = -1;
        Debug.Log("VECTOR X : " + scroll_value);
        
        if (ItemGroup.transform.childCount > 0)
        {
            if (EventSystem.current.currentSelectedGameObject == null)
            {
                ItemSelect(0);
            }
            if (scroll_value > 0)
            {
                NowItemSelectIndexX += 1;
                if (ItemGroup.transform.childCount <= NowItemSelectIndexX)
                {
                    NowItemSelectIndexX = 0;
                }
            }
            else if(scroll_value < 0)
            {
                NowItemSelectIndexX -= 1;
                if (NowItemSelectIndexX < 0)
                {
                    NowItemSelectIndexX = ItemGroup.transform.childCount - 1;
                }
            }
            else
            {

            }
            if (scroll_value != 0)
            {
                ItemSelect(NowItemSelectIndexX);
            }
        }

        //ItemMenuShowUp();
    }
    public void ItemMenuShowUp()
    {


        ItemCanvasGroup.alpha = 1;


    }
    public void ItemMenuHide()
    {
        ItemCanvasGroup.alpha = 0;
    }
    public void ItemSelect(int ItemIndex)
    {
        EventSystem.current.SetSelectedGameObject(ItemGroup.transform.GetChild(ItemIndex).gameObject);
        NowSelectItemUI = ItemGroup.transform.GetChild(ItemIndex).gameObject;
        NowItemSelectIndexX = ItemIndex;
        //Debug.Log("Sprite: : :: : " + NowSelectItemUI.GetComponent<ItemUI>().ItemIcon.sprite);
    }
}
