using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DenchiItemController : MonoBehaviour
{
    //電池を使用したときに実行されるスクリプト
    private FlashLightController flash_light_controller;
    private PlayerStatus player_status;
    private SubNoticedUIController sub_noticed_ui_controller;
    
    // Start is called before the first frame update
    void Start()
    {
        //このprefabが生成された時に懐中電灯のバッテリーをマックスにする。
        player_status = PlayerStatus.LocalInstance;
        sub_noticed_ui_controller = player_status.sub_noticed_ui_controller;
        flash_light_controller = player_status.flash_light_controller;
        flash_light_controller.LightPowerToMax();
        sub_noticed_ui_controller.Notice("バッテリーが復活した");
    }

}
