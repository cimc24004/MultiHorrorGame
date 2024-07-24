using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
public class ItemMenuController : MonoBehaviour
{
    public GameObject ItemGroup;
    public PlayerInput player_input;
    public CanvasGroup ItemCanvasGroup;
    private void Update()
    {
        //Vector2 value = player_input.currentActionMap["ItemSelect"].ReadValue<>
        ItemMenuShowUp();
    }
    public void ItemMenuShowUp()
    {
        try
        {
            EventSystem.current.SetSelectedGameObject(ItemCanvasGroup.transform.GetChild(0).transform.GetChild(0).gameObject);
            Debug.Log(EventSystem.current.currentSelectedGameObject);
            ItemCanvasGroup.alpha = 1;
        }
        catch
        {

        }

    }
    public void ItemMenuHide()
    {
        ItemCanvasGroup.alpha = 0;
    }
    public void ItemSelect()
    {

    }
}
