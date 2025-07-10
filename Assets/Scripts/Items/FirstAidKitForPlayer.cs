using Unity.Entities.UniversalDelegates;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class FirstAidKitForPlayer : MonoBehaviour
{
    public ItemStatus FirstAidKitItemStatus;
    private ItemBelongingsController MyitemBelongingsController;
    private PrayController prayController;
    private PlayerStatus parentPlayerStatus;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        parentPlayerStatus = this.gameObject.transform.parent.gameObject.GetComponent<PlayerStatus>();   
    }
    void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InItOnPlayerSpawn;
        if (PlayerStatus.LocalInstance != null)
        {
            InItOnPlayerSpawn(PlayerStatus.LocalInstance);
        }
    }
    void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InItOnPlayerSpawn;
    }
    private void InItOnPlayerSpawn(PlayerStatus _playerStatus)
    {
        MyitemBelongingsController = _playerStatus.item_menu_controller.item_belongings_controller;
        prayController = this.gameObject.GetComponent<PrayController>();
        prayController.PrayCompletedEvent.AddListener(() =>
        {
            if (parentPlayerStatus.Dead.Value)
            {
                if (_playerStatus == parentPlayerStatus)
                {
                    MyitemBelongingsController.ItemUseFromBelonging(FirstAidKitItemStatus);
                }
            }

        });
    }
    // Update is called once per frame
    void Update()
    {
        if (parentPlayerStatus.Dead.Value)
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
        else
        {
            this.gameObject.tag = "Untagged";
        }

    }
    private bool CheckItem()
    {
        if (MyitemBelongingsController.belonging_items.Count != 0)
        {
            if (MyitemBelongingsController.belonging_items.ContainsKey(FirstAidKitItemStatus))
            {
                return true;
            }
        }
        return false;
    }
}
