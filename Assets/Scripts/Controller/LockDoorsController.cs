using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LockDoorsController : MonoBehaviour
{
    [Header("ロックする予定のドア")]
    [SerializeField] private List<TheDoorController> will_lock_the_doors;
    void OnEnable()
    {
        HousesController.OnHousesControllerInited += OnHousesControllerInited;
    }
    void OnDisable()
    {
        HousesController.OnHousesControllerInited -= OnHousesControllerInited;
    }
    private void OnHousesControllerInited(HousesController houses_controller){
        if (NetworkManager.Singleton.IsServer)
        {
            foreach (TheDoorController the_door_controller in will_lock_the_doors)
            {
                the_door_controller.DoorLock();
            }
        }

    }
}
