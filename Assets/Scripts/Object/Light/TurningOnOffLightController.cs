using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;  // ← DOTween 用

public class TurningOnOffLightController : SerializedMonoBehaviour
{
    public enum TurnStatus
    {
        TurnOff,
        TurnOn
    }
    public TurnStatus turn_status = TurnStatus.TurnOff;

    [Tooltip("点滅させる Light コンポーネントを持つオブジェクト")]
    public GameObject LightObj;
    [SerializeField]
    private bool IsHimoAudioPlay = true;
    [SerializeField]
    private bool IsLightOnAudio = true;
    [SerializeField]
    private bool IsLightAudio = true;
    [SerializeField]
    private bool IsBlinkingLightOnAnimation = true;
    [ShowIf("IsHimoAudioPlay")]
    [SerializeField] private AudioSource HimoAudioSource;
    [ShowIf("IsLightOnAudio")]
    [SerializeField] private AudioSource LightOnAudioSource;
    [ShowIf("IsLightAudio")]
    [SerializeField] private AudioSource LightAudioSource;
    private Light pointLight;

    [SerializeField]
    private float originalIntensity;
    private Sequence flickerSequence;

    private void Start()
    {
        // Light コンポーネント取得
        pointLight = LightObj.GetComponent<Light>();
        if (pointLight == null)
        {
            Debug.LogError("LightObj に Light コンポーネントがアタッチされていません！");
            return;
        }

        // 元の強度を保存して、一旦消灯状態に
        originalIntensity = pointLight.intensity;
        pointLight.intensity = 0f;
    }

    [Button]
    public void TurnOnOffByStatus()
    {
        if (turn_status == TurnStatus.TurnOff)
        {
            // 引きひも音＋点灯音
            if (IsHimoAudioPlay) HimoAudioSource.Play();
            if (IsLightOnAudio) LightOnAudioSource.Play();

            // 前のシーケンスをクリア
            flickerSequence?.Kill();

            if (IsBlinkingLightOnAnimation)
            {
                // 点滅シーケンス
                flickerSequence = DOTween.Sequence()
                    // 0→original→0→original を 3 回繰り返す（合計6ループのYoyo）
                    .Append(pointLight.DOIntensity(originalIntensity, 0.1f).SetLoops(6, LoopType.Yoyo))
                    .OnComplete(() =>
                    {
                        // 最後は確実にフル点灯
                        if (IsLightAudio) LightAudioSource.Play();
                        pointLight.intensity = originalIntensity;
                        turn_status = TurnStatus.TurnOn;
                    });
            }
            else
            {
                // フェードイン
                if (IsLightAudio) LightAudioSource.Play();
                pointLight.DOIntensity(originalIntensity, 0.5f).OnComplete(() =>
                {
                    turn_status = TurnStatus.TurnOn;
                });
            }
        }
        else
        {
            // 引きひも音
            if (IsHimoAudioPlay) HimoAudioSource.Play();

            // 点滅シーケンスが残ってたら止める
            flickerSequence?.Kill();

            // 通常の消灯（フェードアウト）
            pointLight.DOIntensity(0f, 0.1f).OnComplete(() =>
            {
                if (IsLightAudio) LightAudioSource?.Stop();
                turn_status = TurnStatus.TurnOff;
            });
        }
    }
}
