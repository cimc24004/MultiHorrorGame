using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    public Image ItemIcon;
    public Text ItemCountText;
    public ItemBelongingsController item_belonging_controller;
    public ItemStatus item_status;
    public GameObject FocusUI;

    public GameObject LockedItemUI;
    public bool CanItemUse = true;
    public void OnClick()
    {
    }
}
