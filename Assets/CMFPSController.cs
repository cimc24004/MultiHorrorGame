using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CMFPSController : MonoBehaviour
{
    private CinemachineVirtualCamera cinemachine_virtual_camera;
    private CinemachinePOV cinemachine_pov;
    private float DefaultInputSpeedX;
    private float DefaultInputSpeedY;
    private PlayerStatus player_status;
    private void Start()
    {
        player_status = PlayerStatus.instance;
        cinemachine_virtual_camera = player_status.virtual_camera;
        cinemachine_pov = cinemachine_virtual_camera.GetCinemachineComponent<CinemachinePOV>();
        DefaultInputSpeedX = cinemachine_pov.m_HorizontalAxis.m_MaxSpeed;
        DefaultInputSpeedY = cinemachine_pov.m_VerticalAxis.m_MaxSpeed;
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
