using UnityEngine;
using DG.Tweening;
using System;   // ← Action 用

/// <summary>
/// AudioSource の音量を DOTween でフェードさせる
/// </summary>
public class AudioFadeController : MonoBehaviour
{
    [Header("操作対象")]
    [SerializeField] private AudioSource audioSource;

    [Header("フェード設定")]
    public float fadeDuration = 1f;

    Tween fadeTween;   // いま走ってるフェードを掴んでおく

    // ────────── 外から呼ぶのはここだけ ──────────

    /// <summary>再生してフェードイン</summary>
    public void PlayWithFadeIn(float maxVolume, Action onComplete = null)
    {
        if (!audioSource) return;

        KillTween();
        audioSource.volume = 0f;
        audioSource.Play();

        fadeTween = audioSource
            .DOFade(maxVolume, fadeDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() => onComplete?.Invoke());
    }

    /// <summary>フェードアウトして Stop</summary>
    public void FadeOutAndStop(float minVolume, Action onComplete = null)
    {
        if (!audioSource) return;

        KillTween();
        fadeTween = audioSource
            .DOFade(minVolume, fadeDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                audioSource.Stop();
                onComplete?.Invoke();
            });
    }

    /// <summary>任意の音量へフェードしたい時用</summary>
    public void FadeTo(float targetVolume, Action onComplete = null)
    {
        if (!audioSource) return;

        KillTween();
        fadeTween = audioSource
            .DOFade(Mathf.Clamp01(targetVolume), fadeDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() => onComplete?.Invoke());
    }

    // ────────── 内部ヘルパー ──────────

    void KillTween()
    {
        if (fadeTween != null && fadeTween.IsActive())
            fadeTween.Kill();
    }

    void OnDestroy() => KillTween();
}
