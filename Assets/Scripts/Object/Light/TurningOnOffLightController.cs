using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;
using Unity.Netcode;

public class TurningOnOffLightController : NetworkBehaviour
{
    public enum TurnStatus
    {
        TurnOff,
        TurnOn
    }

    [SerializeField]
    private TurnStatus turn_status = TurnStatus.TurnOff;

    [Tooltip("点滅させる Light コンポーネントを持つオブジェクト")]
    public GameObject LightObj;

    [SerializeField] private bool IsHimoAudioPlay             = true;
    [SerializeField] private bool IsLightOnAudio              = true;
    [SerializeField] private bool IsLightAudio                = true;
    [SerializeField] private bool IsBlinkingLightOnAnimation  = true;

    [ShowIf("IsHimoAudioPlay")]
    [SerializeField] private AudioSource HimoAudioSource;
    [ShowIf("IsLightOnAudio")]
    [SerializeField] private AudioSource LightOnAudioSource;
    [ShowIf("IsLightAudio")]
    [SerializeField] private AudioSource LightAudioSource;

    private Light    pointLight;
    private float    originalIntensity;
    private Sequence flickerSequence;

    private void Start()
    {
        // Light コンポーネントを取得し、初期消灯
        pointLight = LightObj.GetComponent<Light>();
        if (pointLight == null)
        {
            Debug.LogError("LightObj に Light コンポーネントがありません！");
            enabled = false;
            return;
        }
        originalIntensity  = pointLight.intensity;
        pointLight.intensity = 0f;
    }

    // クライアント側で呼ぶトグル用メソッド
    public void ToggleLight()
    {
        // ServerRpc を呼んでサーバーに処理を依頼
        TurnOnOffByStatusServerRequestServerRPC();
    }

    // サーバー側で状態をトグルして、ClientRpc で全クライアントに通知
    [ServerRpc(RequireOwnership = false)]
    public void TurnOnOffByStatusServerRequestServerRPC(ServerRpcParams rpcParams = default)
    {
        // サーバー側で状態を反転
        turn_status = turn_status == TurnStatus.TurnOff
            ? TurnStatus.TurnOn
            : TurnStatus.TurnOff;

        // 反転後の状態を全クライアントで実行
        TurnOnOffByStatusClientRPC(turn_status);
    }

    // サーバー→クライアント RPC：実際のアニメ／音再生
    [ClientRpc]
    public void TurnOnOffByStatusClientRPC(
        TurnStatus newStatus,
        ClientRpcParams rpcParams = default
    )
    {
        // クライアントの状態を更新
        turn_status = newStatus;

        // 既存の flickerSequence があれば停止
        flickerSequence?.Kill();

        if (turn_status == TurnStatus.TurnOn)
        {
            // 点灯処理
            if (IsHimoAudioPlay)    HimoAudioSource?.Play();
            if (IsLightOnAudio)     LightOnAudioSource?.Play();

            if (IsBlinkingLightOnAnimation)
            {
                // 点滅シーケンス（Yoyo 6 ループ＝3回点滅）
                flickerSequence = DOTween.Sequence()
                    .Append(pointLight
                        .DOIntensity(originalIntensity, 0.1f)
                        .SetLoops(6, LoopType.Yoyo))
                    .OnComplete(() =>
                    {
                        // 最後は確実に点灯
                        if (IsLightAudio) LightAudioSource?.Play();
                        pointLight.intensity = originalIntensity;
                    });
            }
            else
            {
                // フェードイン
                if (IsLightAudio) LightAudioSource?.Play();
                pointLight
                    .DOIntensity(originalIntensity, 0.5f)
                    .OnComplete(() => { /* nothing more */ });
            }
        }
        else
        {
            // 消灯処理
            if (IsHimoAudioPlay) HimoAudioSource?.Play();

            pointLight
                .DOIntensity(0f, 0.1f)
                .OnComplete(() =>
                {
                    if (IsLightAudio) LightAudioSource?.Stop();
                });
        }
    }
}
