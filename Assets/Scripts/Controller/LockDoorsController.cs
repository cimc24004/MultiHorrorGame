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
        PlayerStatus.OnLocalPlayerSpawned += InitOnPlayerSpawn;
    }
    void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitOnPlayerSpawn;
    }
    private void InitOnPlayerSpawn(PlayerStatus _playerStatus){
        if (NetworkManager.Singleton.IsServer)
        {
            foreach (TheDoorController the_door_controller in will_lock_the_doors)
            {
                the_door_controller.DoorLock();
            }
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        

    }

}
