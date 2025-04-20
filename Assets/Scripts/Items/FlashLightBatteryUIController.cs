using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class FlashLightBatteryUIController : Singleton<FlashLightBatteryUIController>
{
    public GameObject FlashLightBatteryUI;
    [SerializeField] private int BatteryCount = 4;
    [SerializeField] private List<GameObject> BatteryImageObjs;
    [Button]
    public void UIChangeOnBatteryToMax(){
        BatteryCount = 4;
        foreach(GameObject BatteryImageObj in BatteryImageObjs){
            BatteryImageObj.SetActive(true);
        }
    }
    [Button]
    public void UIChangeOnBatteryDecrease(){
        BatteryCount -= 1;
        if(BatteryCount == 0){
            BatteryImageObjs[0].SetActive(false);
        }else{
            BatteryImageObjs[(BatteryCount)].SetActive(false);
        }
    }
}
