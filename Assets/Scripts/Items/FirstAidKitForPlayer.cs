using Unity.Entities.UniversalDelegates;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class FirstAidKitForPlayer : NetworkBehaviour
{
    public ItemStatus FirstAidKitItemStatus;
    private ItemBelongingsController MyitemBelongingsController;
    private PrayController prayController;
    private PlayerStatus parentPlayerStatus;
    public bool CanFirstAid;
    private PlayerStatus myPlayerStatus;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

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
        parentPlayerStatus = this.gameObject.transform.parent.gameObject.GetComponent<PlayerStatus>();
        MyitemBelongingsController = _playerStatus.item_menu_controller.item_belongings_controller;
        prayController = this.gameObject.GetComponent<PrayController>();
        myPlayerStatus = _playerStatus;
        prayController.PrayCompletedEvent.AddListener(() =>
        {
            if (parentPlayerStatus.Dead.Value)
            {
                if (_playerStatus == parentPlayerStatus)
                {
                    OnCompletedPrayRequestServerRpc();
                }
                if (prayController.PrayNowPlayers[0] == myPlayerStatus.OwnerClientId) {
                    Debug.Log("----------------FirstAidKitForPlayer 111111-----------------");
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
                CanFirstAid = true;
            }
            else
            {
                this.gameObject.tag = "CantFirstAidKit";
                CanFirstAid = false;
            }
        }
        else
        {
            this.gameObject.tag = "Untagged";
            CanFirstAid = false;
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

    [ClientRpc]
    private void FirstAidUsedClientRpc()
    {
        myPlayerStatus.sub_noticed_ui_controller.Notice($"{parentPlayerStatus.SteamUserName}");
    }
    [ServerRpc]
    private void OnCompletedPrayRequestServerRpc()
    {
        prayController.PrayReset();
    }
}
