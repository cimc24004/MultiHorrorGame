using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HutonHideEvent : MonoBehaviour
{
    private CMFPSController cm_fps_controller;
    [SerializeField] private float HorizontalMinValue;
    [SerializeField] private float HorizontalMaxValue;
    [SerializeField] private float VerticalMinValue;
    [SerializeField] private float VerticalMaxValue;
    private PlayerStatus player_status;
    private AudioSource HideActionSource;

    void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InitOnPlayerSpawn;
    }
    void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitOnPlayerSpawn;
    }
    private void InitOnPlayerSpawn(PlayerStatus _playerStatus){
        cm_fps_controller = CMFPSController.instance;
        player_status = _playerStatus;
    }
    public void OnHide(){
        // cm_fps_controller.VirtualCameraPOVHorizontalValue(this.transform.rotation.y);
        cm_fps_controller.LimitCameraRotation(this.transform.eulerAngles.y + HorizontalMinValue, this.transform.eulerAngles.y + HorizontalMaxValue,VerticalMinValue,VerticalMaxValue);
    }
    public void OnUnHide(){
        cm_fps_controller.LimitCameraRotationReset();
    }
}
