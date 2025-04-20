using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashLightEquip : MonoBehaviour
{
    private FlashLightBatteryUIController flash_light_battery_ui_controller;
    
    // Start is called before the first frame update
    void OnEnable()
    {
        if(flash_light_battery_ui_controller == null){
            flash_light_battery_ui_controller = FlashLightBatteryUIController.instance;
        }
        flash_light_battery_ui_controller.FlashLightBatteryUI.SetActive(true);
    }
    void OnDisable()
    {
        flash_light_battery_ui_controller.FlashLightBatteryUI.SetActive(false);
    }
}
