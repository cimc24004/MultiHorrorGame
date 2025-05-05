using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CMFPSController : Singleton<CMFPSController>
{
    private CinemachineVirtualCamera cinemachine_virtual_camera;
    private CinemachinePOV cinemachine_pov;
    private float DefaultInputSpeedX;
    private float DefaultInputSpeedY;
    private PlayerStatus player_status;

    public float DefaultHorizontalMinValue;
    public float DefaultHorizontalMaxValue;
    public float DefaultVerticalMinValue;
    public float DefaultVerticalMaxValue;

    private void Start()
    {
        player_status = PlayerStatus.instance;
        cinemachine_virtual_camera = player_status.virtual_camera;
        cinemachine_pov = cinemachine_virtual_camera.GetCinemachineComponent<CinemachinePOV>();
        DefaultInputSpeedX = cinemachine_pov.m_HorizontalAxis.m_MaxSpeed;
        DefaultInputSpeedY = cinemachine_pov.m_VerticalAxis.m_MaxSpeed;
        DefaultHorizontalMinValue = cinemachine_pov.m_HorizontalAxis.m_MinValue;
        DefaultHorizontalMaxValue = cinemachine_pov.m_HorizontalAxis.m_MaxValue;
        DefaultVerticalMinValue = cinemachine_pov.m_VerticalAxis.m_MinValue;
        DefaultVerticalMaxValue = cinemachine_pov.m_VerticalAxis.m_MaxValue;

    }
    public void VirtualCameraPOVHorizontalValue(float HorizontalValue){
        this.cinemachine_pov.m_HorizontalAxis.Value = HorizontalValue;
    }
    public void LimitCameraRotation(float HorizontalMinValue, float HorizontalMaxValue, float VerticalMinValue, float VerticalMaxValue){
        //カメラ回転に制限つける
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
    public void POVInputInvalid()
    {
        //無効にする前に一旦感度を保存しておく
        DefaultInputSpeedX = cinemachine_pov.m_HorizontalAxis.m_MaxSpeed;
        DefaultInputSpeedY = cinemachine_pov.m_VerticalAxis.m_MaxSpeed;

        //POVを無効にする。(感度を０にする)
        cinemachine_pov.m_HorizontalAxis.m_MaxSpeed = 0;
        cinemachine_pov.m_VerticalAxis.m_MaxSpeed = 0;

    }
    public void POVInputEnable()
    {
        //POVを有効にする。
        cinemachine_pov.m_HorizontalAxis.m_MaxSpeed = DefaultInputSpeedX;
        cinemachine_pov.m_VerticalAxis.m_MaxSpeed = DefaultInputSpeedY;
    }
}
