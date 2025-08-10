using Unity.Entities.UniversalDelegates;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class FirstAidKitForPlayer : NetworkBehaviour
{
    public ItemStatus FirstAidKitItemStatus;
    private ItemBelongingsController PrayerItemBelongingsController;
    private ItemBelongingsController MyItemBelongingsController;
    private PrayController prayController;
    private PlayerStatus parentPlayerStatus;
    public bool CanFirstAid;
    private PlayerStatus myPlayerStatus;
    private PlayerAllController playerAllController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public override void OnNetworkSpawn()
    {
        PlayerStatus.OnLocalPlayerSpawned += InItOnPlayerSpawn;
        if (PlayerStatus.LocalInstance != null)
        {
            InItOnPlayerSpawn(PlayerStatus.LocalInstance);
        }
    }
    public override void OnNetworkDespawn()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InItOnPlayerSpawn;
    }
    private void InItOnPlayerSpawn(PlayerStatus _playerStatus)
    {
        parentPlayerStatus = this.gameObject.transform.parent.gameObject.GetComponent<PlayerStatus>();
        prayController = this.gameObject.GetComponent<PrayController>();
        myPlayerStatus = _playerStatus;
        playerAllController = PlayerAllController.instance;
        MyItemBelongingsController = myPlayerStatus.item_menu_controller.item_belongings_controller;
        Debug.Log("InitOnPlayerSpawn: " + _playerStatus.OwnerClientId);
        prayController.PrayCompletedEvent.AddListener(() =>
        {
            if (parentPlayerStatus.Dead.Value)
            {
                if (_playerStatus == parentPlayerStatus)
                {
                    OnCompletedPrayRequestServerRpc();
                }
                Debug.Log("prayController.PrayNowPlayers.Count: " + prayController.PrayNowPlayers.Count);
                Debug.Log("myPlayerStatus.OwnerClientId: " + myPlayerStatus.OwnerClientId);
                Debug.Log("PrayNowPlayers[0]: " + prayController.PrayNowPlayers[0]);
                Debug.Log("parentPlayerStatus: " + parentPlayerStatus.OwnerClientId);

                Debug.Log("----------------FirstAidKitForPlayer 111111-----------------");
                NetworkObject playerNetworkObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(prayController.PrayNowPlayers[0]);
                PrayerItemBelongingsController = playerAllController.SpawnedPlayersWithPlayerStatuses[playerNetworkObject.gameObject].item_menu_controller.item_belongings_controller;
                PrayerItemBelongingsController.ItemUseFromBelonging(FirstAidKitItemStatus);
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
        if (MyItemBelongingsController.belonging_items.Count != 0)
        {
            if (MyItemBelongingsController.belonging_items.ContainsKey(FirstAidKitItemStatus))
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
