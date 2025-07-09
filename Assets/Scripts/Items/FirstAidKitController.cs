using Unity.Entities.UniversalDelegates;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class FirstAidKitController : NetworkBehaviour
{
    private ItemBelongingsController itemBelongingsController;
    [SerializeField] private ItemStatus FirstAidKitItemStatus;
    [SerializeField] private float ResuscitationMaxValue;
    private PrayController prayController;
    private CrossHairEvent crossHairEvent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        prayController = this.gameObject.GetComponent<PrayController>();
        crossHairEvent = PlayerStatus.LocalInstance.cross_hair_event;
        itemBelongingsController = PlayerStatus.LocalInstance.item_menu_controller.item_belongings_controller;
    }
    // Update is called once per frame
    void Update()
    {
        if (CanUseFirstAidKit())
        {
            if (crossHairEvent.RaycasstedObj == this.gameObject)
            {
                prayController.enabled = true;
            }
            else
            {
                prayController.enabled = false;
            }
        }
        else
        {
            prayController.enabled = false;
        }

    }

    public bool CanUseFirstAidKit()
    {
        if (itemBelongingsController.belonging_items.Count != 0)
        {
            if (itemBelongingsController.belonging_items[FirstAidKitItemStatus].item_count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }

    }
}
