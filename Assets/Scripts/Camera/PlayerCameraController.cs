using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraController : NetworkSingleton<PlayerCameraController>
{
    //カメラの回転に沿って、プレイヤーの向きを変更するスクリプト
    public GameObject DirectionCamera;
    private bool InitPlayer;
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
        DirectionCamera = _playerStatus.virtual_camera.VirtualCameraGameObject;
        InitPlayer = true;
    }
    // Update is called once per frame
    void Update()
    {
        if(!InitPlayer) return;
        // Y軸のみ、カメラの向きに即時同期
        float camYaw = DirectionCamera.transform.eulerAngles.y;
        transform.rotation = Quaternion.Euler(0f, camYaw, 0f);
    }
}
