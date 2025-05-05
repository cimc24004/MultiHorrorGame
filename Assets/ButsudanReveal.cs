using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButsudansReveal : MonoBehaviour
{
    public float ButsudansRevealTime;
    private HousesController houses_controller;
    private PlayerStatus player_status;
    private SubNoticedUIController sub_noticed_ui_controller;
    // Start is called before the first frame update
    void Start()
    {
        houses_controller = HousesController.instance;
        player_status = PlayerStatus.instance;  
        sub_noticed_ui_controller = player_status.sub_noticed_ui_controller;
    }

    public void RevealButsudansAction(){
        sub_noticed_ui_controller.Notice("仏壇の位置が明らかになった..!!");
        houses_controller.RevealButusdans(ButsudansRevealTime);
        sub_noticed_ui_controller.Notice("仏壇の位置がわからなくなった....", ButsudansRevealTime);
    }
}
