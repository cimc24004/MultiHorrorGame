using Unity.Entities.UniversalDelegates;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class FirstAidKitForPlayer : NetworkBehaviour
{
    public ItemStatus FirstAidKitItemStatus;
    private PlayerStatus playerStatus;
    private ItemBelongingsController itemBelongingsController;
    private CrossHairEvent playerCrossHairEvent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        playerStatus = PlayerStatus.LocalInstance;
        playerCrossHairEvent = playerStatus.cross_hair_event;
        itemBelongingsController = playerStatus.item_menu_controller.item_belongings_controller;
    }
    // Update is called once per frame
    void Update()
    {
        if (CheckItem())
        {
            this.gameObject.tag = "Pray";
        }
        else
        {
            this.gameObject.tag = "Untagged";
        }
    }
    private bool CheckItem()
    {
        if (itemBelongingsController.belonging_items.Count != 0)
        {
            if (itemBelongingsController.belonging_items.ContainsKey(FirstAidKitItemStatus))
            {
                return true;
            }
        }
        return false;
    }
}
