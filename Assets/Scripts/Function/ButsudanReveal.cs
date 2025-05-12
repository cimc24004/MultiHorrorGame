using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class ButsudansReveal : NetworkBehaviour
{
    public float ButsudansRevealTime;
    private HousesController houses_controller;
    private PlayerStatus player_status;
    private SubNoticedUIController sub_noticed_ui_controller;
    // Start is called before the first frame update
    // void Start()
    // {

    // }
    public override void OnNetworkSpawn(){
        houses_controller = HousesController.instance;
        player_status = PlayerStatus.LocalInstance;  
        sub_noticed_ui_controller = player_status.sub_noticed_ui_controller;
    }
    public void RevealButsudansAction(){
        sub_noticed_ui_controller.Notice("仏壇の位置が明らかになった..!!");
        houses_controller.RevealButusdans(ButsudansRevealTime);
        sub_noticed_ui_controller.Notice("仏壇の位置がわからなくなった....", ButsudansRevealTime);
    }
}
