using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DG.Tweening;
using System; // Action<T> 用

/// <summary>
/// Volume.weight を DOTween でフェード制御するコントローラ
/// ・FadeIn    : 0 → maxWeight
/// ・FadeOut   : maxWeight → 0
/// ・FadeTo    : 現在値 → 任意の値
/// 各フレームの値をコールバックで受け取れるので、AudioSource.volume などを連動しやすい
/// </summary>
public class PostProcessVolumeController : MonoBehaviour
{
    [Header("操作対象 Volume と目標 Weight")]
    [SerializeField] private Volume volume;              // シーン上の Volume
    [SerializeField] private float maxWeight   = 1f;     // フェードイン時の目標 weight
    [SerializeField] private float defaultDuration = 1f; // デフォルトのフェード時間

    Tween fadeTween;

    /// <summary>
    /// 0 → maxWeight へフェードイン
    /// </summary>
    /// <param name="duration">秒数（負値ならデフォルト）</param>
    /// <param name="onUpdate">現在の weight を毎フレーム渡す</param>
    public void FadeIn(float duration = -1f, Action<float> onUpdate = null)
    {
        FadeTo(maxWeight, duration, onUpdate, resetFromZero: true);
    }

    /// <summary>
    /// maxWeight → 0 へフェードアウト
    /// </summary>
    /// <param name="duration">秒数（負値ならデフォルト）</param>
    /// <param name="onUpdate">現在の weight を毎フレーム渡す</param>
    public void FadeOut(float duration = -1f, Action<float> onUpdate = null)
    {
        FadeTo(0f, duration, onUpdate, resetFromZero: true);
    }

    /// <summary>
    /// 現在の volume.weight → targetValue へフェード
    /// </summary>
    /// <param name="targetValue">目標の 0〜maxWeight 値</param>
    /// <param name="duration">秒数（負なら defaultDuration）</param>
    /// <param name="onUpdate">現在の weight を毎フレーム受け取るコールバック</param>
    /// <param name="resetFromZero">
    ///     開始前に volume.weight を 0 にリセットするか  
    ///     （FadeIn/FadeOut では毎回初期値を揃えたいので true）
    /// </param>
    public void FadeTo(float targetValue, float duration = -1f, Action<float> onUpdate = null, bool resetFromZero = false)
    {
        if (volume == null)
        {
            Debug.LogWarning("[PPVC] Volume がアサインされていません。");
            return;
        }

        if (duration <= 0f) duration = defaultDuration;

        // 既存 Tween をキャンセル
        if (fadeTween != null && fadeTween.IsActive())
            fadeTween.Kill();

        // 必要ならリセット
        if (resetFromZero)
            volume.weight = 0f;

        // Tween 実行
        fadeTween = DOTween.To(
            () => volume.weight,        // getter
            x =>                      // setter
            {
                volume.weight = x;
                onUpdate?.Invoke(x);
            },
            targetValue,               // end value
            duration
        )
        .SetEase(Ease.Linear);
    }

    void OnDestroy()
    {
        if (fadeTween != null && fadeTween.IsActive())
            fadeTween.Kill();
    }
}
