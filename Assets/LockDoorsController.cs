using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockDoorsController : MonoBehaviour
{
    [Header("ロックする予定のドア")]
    [SerializeField] private List<TheDoorController> will_lock_the_doors;
    // Start is called before the first frame update
    void Start()
    {
        foreach(TheDoorController the_door_controller in will_lock_the_doors){
            the_door_controller.DoorLock();
        }
    }

}
