using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameProgressController : Singleton<GameProgressController>
{
    [Header("????????")]
    public int NeedPrayCount = 5;

    [Header("????????")]
    public int PrayCountNow = 0;

    public bool DeliverItemGeted;
    
    public bool DeliveredToSakura;

    private ProgressWithMapUIController progress_with_map_controller;

    // Start is called before the first frame update
    void Start()
    {
        progress_with_map_controller = this.gameObject.GetComponent<ProgressWithMapUIController>();
        progress_with_map_controller.ButsudanPrayCountTextUI.text = $"{PrayCountNow}/{NeedPrayCount}";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPrayedButsudan()
    {
        PrayCountNow++;
        progress_with_map_controller.ButsudanPrayCountTextUI.text = $"{PrayCountNow}/{NeedPrayCount}";

        if (PrayCountNow >= NeedPrayCount)
        {
            progress_with_map_controller.ButsudanAllPrayedToggleUI.isOn = true;
        }
    }

    public void OnItemGeted()
    {
        progress_with_map_controller.DeliverItemGetedToggleUI.isOn = true;
    }

    public void OnDeliverSakura(){
        
    }
}
