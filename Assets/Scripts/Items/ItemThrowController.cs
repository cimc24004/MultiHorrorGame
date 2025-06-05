using System.Collections;
using System.Collections.Generic;
using MathNet.Numerics.Optimization.TrustRegion;
using UnityEngine;
using UnityEngine.InputSystem;

using Unity.Netcode;
public class ItemThrowController : NetworkBehaviour
{
    //Ȃɂ𓊂ׂ̃XNvgB 

    //ɓACeZbgĂ邾B
    public GameObject MainCamera;
    public PlayerInput player_input;
    public Transform StartFiringPos;
    public float ThrowFirstSpeed = 1;
    public ItemStatus item_status;
    private PlayerStatus player_status;
    private bool InitPlayer;
    void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InitOnPlayerSpawn;
    }
    void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitOnPlayerSpawn;
    }
    private void InitOnPlayerSpawn(PlayerStatus _playerStatus){
        player_status = _playerStatus;
        player_input = player_status.player_input;
        MainCamera = player_status.virtual_camera.VirtualCameraGameObject;
        InitPlayer = true;
    }
    // Update is called once per frame
    void Update()
    {
        if(!InitPlayer) return;
        //vC[̃RC_[ɂԂď肭邱ƂłȂ̂
        //ItemThrowConroller̎qvfɂIuWFNg甭˂邱Ƃŏ肭Ă
        
        //CamerǎĂɌ 悤ɂB
        this.transform.forward = new Vector3(MainCamera.transform.forward.x, MainCamera.transform.forward.y, MainCamera.transform.forward.z);
        

    }
    public void ThrowStart(ItemStatus status)
    {
        if (!IsServer)
        {
            ThrowStartServerRpc(status.GlobalId, StartFiringPos.position, transform.forward);
        }
        else
        {
            SpawnAndThrow(status, StartFiringPos.position, transform.forward);
        }
    }

    [ServerRpc]
    private void ThrowStartServerRpc(uint globalId, Vector3 position, Vector3 forward)
    {
        ItemStatus stat = ItemDatabases.instance.GetStatusByGlobalId(globalId);
        if (stat == null) return;
        SpawnAndThrow(stat, position, forward);
    }

    private void SpawnAndThrow(ItemStatus status, Vector3 position, Vector3 forward)
    {
        GameObject obj = Instantiate(status.ItemThrowPrefab, position, Quaternion.identity);
        NetworkObject netObj = obj.GetComponent<NetworkObject>();
        if (netObj != null)
            netObj.Spawn(true);
        HorizontalProjectile horizontal_projectile = obj.GetComponent<HorizontalProjectile>();
        horizontal_projectile.Throw(ThrowFirstSpeed, forward);
    }

}
