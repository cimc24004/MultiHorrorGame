using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HousesRevealController : MonoBehaviour
{
    private HousesController houses_controller;
    [SerializeField] private float silhouette_change_reset_interval_time;
    // Start is called before the first frame update
    void Start()
    {
        houses_controller = HousesController.instance;

    }

    // public void Reveal(){
    //     foreach(SilhouetteChangeController silhouette_change_controller in houses_controller.silhouette_change_controllers){
    //         silhouette_change_controller.SilhouetteChangeWithAutoReset(silhouette_change_reset_interval_time);
    //     }
    // }
}
