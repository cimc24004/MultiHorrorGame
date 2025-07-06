using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Sirenix.OdinInspector;
public class CameraShakeController : Singleton<CameraShakeController>
{
    [SerializeField] private CinemachineImpulseSource DiscoveriedImpulseSource;
    [SerializeField] private CinemachineImpulseSource DoorAttackImpulseSource;
    public enum CameraImpulseSourceType
    {
        Discoveried,
        DoorAttack
    }
    public CameraImpulseSourceType cameraImpulseSourceType;
    void Start()
    {
        // コンポーネントがアタッチされていない場合は取得
        if (DiscoveriedImpulseSource == null)
            DiscoveriedImpulseSource = GetComponent<CinemachineImpulseSource>();
    }

    // この関数を呼び出すとカメラが揺れる
    public void ShakeCamera(float intensity,CameraImpulseSourceType _cameraImpulseSourceType)
    {
        if (_cameraImpulseSourceType == CameraImpulseSourceType.Discoveried)
        {
            DiscoveriedImpulseSource.GenerateImpulse(intensity);
        }
        else if (_cameraImpulseSourceType == CameraImpulseSourceType.DoorAttack)
        {
            DoorAttackImpulseSource.GenerateImpulse(intensity);
        }
    }

}
