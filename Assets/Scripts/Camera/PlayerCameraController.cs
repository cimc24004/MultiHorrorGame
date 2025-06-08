using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;
using Unity.Netcode;
using UnityEngine;

public class PlayerCameraController : NetworkBehaviour
{
    //カメラの回転に沿って、プレイヤーの向きを変更するスクリプト
    public GameObject DirectionCamera;
    private bool InitPlayer;
    private float camYaw;
    
    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        
    }
    void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InitOnPlayerSpawn;
    }
    void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitOnPlayerSpawn;
    }
    private void InitOnPlayerSpawn(PlayerStatus _playerStatus){
        if(IsOwner){
            DirectionCamera = _playerStatus.virtual_camera.VirtualCameraGameObject;
        }
        InitPlayer = true;
    }
    // Update is called once per frame
    void Update()
    {
        if (!InitPlayer || !IsOwner) return;
        float yaw = DirectionCamera.transform.eulerAngles.y;
        SendYawToServerRpc(yaw);
    }

    [ServerRpc]
    void SendYawToServerRpc(float yaw)
    {
        // サーバー側のみ書き込み権限あるからOK
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }
}

