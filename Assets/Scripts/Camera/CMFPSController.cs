using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CMFPSController : Singleton<CMFPSController>
{
    private CinemachineVirtualCamera cinemachine_virtual_camera;
    private CinemachinePOV cinemachine_pov;
    public float DefaultHorizontalMinValue;
    public float DefaultHorizontalMaxValue;
    public float DefaultVerticalMinValue;
    public float DefaultVerticalMaxValue;
    private PlayerStatus player_status;

    private void Start()
    {
        cinemachine_virtual_camera = this.gameObject.GetComponent<CinemachineVirtualCamera>();
        cinemachine_pov = cinemachine_virtual_camera.GetCinemachineComponent<CinemachinePOV>();
        DefaultHorizontalMinValue = cinemachine_pov.m_HorizontalAxis.m_MinValue;
        DefaultHorizontalMaxValue = cinemachine_pov.m_HorizontalAxis.m_MaxValue;
        DefaultVerticalMinValue = cinemachine_pov.m_VerticalAxis.m_MinValue;
        DefaultVerticalMaxValue = cinemachine_pov.m_VerticalAxis.m_MaxValue;

    }
    private void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InitWhenPlayerSpawned;
    }
    private void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitWhenPlayerSpawned;
    }
    private void InitWhenPlayerSpawned(PlayerStatus _player_status){
        player_status = _player_status;
        cinemachine_virtual_camera.m_Follow = player_status.CameraRoot;
        cinemachine_virtual_camera.m_LookAt = player_status.CameraRoot;
    }
    public void VirtualCameraPOVHorizontalValue(float HorizontalValue){
        this.cinemachine_pov.m_HorizontalAxis.Value = HorizontalValue;
    }
    public void LimitCameraRotation(float HorizontalMinValue, float HorizontalMaxValue, float VerticalMinValue, float VerticalMaxValue){
        //ÉJÉÅÉââÒì]Ç…êßå¿Ç¬ÇØÇÈ
        cinemachine_pov.m_HorizontalAxis.m_MinValue = HorizontalMinValue;
        cinemachine_pov.m_HorizontalAxis.m_MaxValue = HorizontalMaxValue;
        cinemachine_pov.m_HorizontalAxis.m_Wrap = false;
        cinemachine_pov.m_VerticalAxis.m_MinValue = VerticalMinValue;
        cinemachine_pov.m_VerticalAxis.m_MaxValue = VerticalMaxValue;
    }
    public void LimitCameraRotationReset(){
        cinemachine_pov.m_HorizontalAxis.m_MinValue = DefaultHorizontalMinValue;
        cinemachine_pov.m_HorizontalAxis.m_MaxValue = DefaultHorizontalMaxValue;
        cinemachine_pov.m_VerticalAxis.m_MinValue = DefaultVerticalMinValue;
        cinemachine_pov.m_VerticalAxis.m_MaxValue = DefaultVerticalMaxValue;
        cinemachine_pov.m_HorizontalAxis.m_Wrap = true;
    }
}
