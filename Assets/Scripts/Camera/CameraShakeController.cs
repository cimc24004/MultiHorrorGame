using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Sirenix.OdinInspector;
public class CameraShakeController : Singleton<CameraShakeController>
{
    [SerializeField] private CinemachineImpulseSource impulseSource;

    void Start()
    {
        // コンポーネントがアタッチされていない場合は取得
        if (impulseSource == null)
            impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    // この関数を呼び出すとカメラが揺れる
    public void ShakeCamera()
    {
        impulseSource.GenerateImpulse();
    }

    // 強度を指定して揺らす
    public void ShakeCamera(float intensity)
    {
        impulseSource.GenerateImpulse(intensity);
    }

    // 方向と強度を指定して揺らす
    public void ShakeCamera(Vector3 velocity)
    {
        impulseSource.GenerateImpulse(velocity);
    }
    // [Button]
    // public void ShakeCameraWithDuration(float duration, float intensity)
    // {
    //     // 地震用の長い揺れ
    //     StartCoroutine(ContinuousShake(duration, intensity));
    // }
    
    // private IEnumerator ContinuousShake(float duration, float interval)
    // {
    //     float elapsed = 0f;
    //     while (elapsed < duration)
    //     {
    //         impulseSource.GenerateImpulse();
    //         yield return new WaitForSeconds(interval);
    //         elapsed += interval;
    //     }
    // }
}
