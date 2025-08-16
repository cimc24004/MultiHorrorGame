// ============================
// NetworkLoadingScreen.cs  (UniTask / クリーン版)
// ============================
using System;
using System.Threading;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

[DefaultExecutionOrder(-10000)]
public class NetworkLoadingScreen : MonoBehaviour
{
    public static NetworkLoadingScreen Instance { get; private set; }

    [Header("UI (必須参照)")]
    [SerializeField] private GameObject loadingRoot;    // Canvas等
    [SerializeField] private Slider loadingSlider;      // Min=0, Max=1
    [SerializeField] private TMP_Text percentText;

    [Header("Tuning")]
    [SerializeField] private float fakeProgressSpeed = 0.25f; // /sec → 0.9
    [SerializeField] private float stuckTimeout      = 0.5f;   // 実進捗が止まったらフェイクへ

    [Header("UX")]
    [SerializeField] private float minShowTime          = 0.6f; // 最低表示時間(秒)
    [SerializeField] private float holdFullTime         = 0.25f;// 100%表示キープ
    [SerializeField] private float resumeTweenDuration  = 0.35f;// 復帰時の追いつき
    [SerializeField] private float bigGapThreshold      = 0.20f;// 大きい隙間ならTween

    private bool  isShowing;
    private float showStartRealtime;
    private float lastUpdateRealtime;
    private float displayed; // 表示上の値 0..1

    // タスク用CT
    private CancellationTokenSource sessionCts; // セッション全体
    private CancellationTokenSource tweenCts;   // Tween
    private CancellationTokenSource fakeCts;    // フェイク

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (loadingRoot) loadingRoot.SetActive(false);

        SceneManager.activeSceneChanged += OnActiveSceneChanged;

        var nm = NetworkManager.Singleton;
        if (nm != null && nm.SceneManager != null)
        {
            nm.SceneManager.OnSceneEvent += OnSceneEventHandler; // これ一本
        }
    }

    void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        var nm = NetworkManager.Singleton;
        if (nm != null && nm.SceneManager != null)
        {
            nm.SceneManager.OnSceneEvent -= OnSceneEventHandler;
        }
        CancelAllTasks();
    }

    private void OnActiveSceneChanged(Scene prev, Scene next)
    {
        if (!isShowing) return;
        _ = CloseAfterDelayAsync(); // fire-and-forget
    }

    // ← ClientIdでフィルタしない。Load以外にSynchronize/ReSynchronizeも拾う
    private void OnSceneEventHandler(SceneEvent e)
    {
        switch (e.SceneEventType)
        {
            case SceneEventType.Load:
            case SceneEventType.Synchronize:
            case SceneEventType.ReSynchronize:
            {
                Show();

                var op = e.AsyncOperation;
                if (op != null)
                {
                    _ = TrackProgressWithFallbackAsync(op, sessionCts.Token);
                }
                else
                {
                    // クライアントはnullなことがある → フェイクで0.9まで
                    StartFakeProgress();
                }
                break;
            }

            case SceneEventType.LoadComplete:
            {
                var nm = NetworkManager.Singleton;
                if (nm != null && e.ClientId == nm.LocalClientId)
                {
                    SmoothTo(1f); // 自分のロード完了 → なめらかに1.0へ
                }
                break;
            }

            case SceneEventType.LoadEventCompleted:
            {
                _ = CloseAfterDelayAsync(); // 全員完了 → 閉じる
                break;
            }
        }
    }

    // ───────── 表示/非表示 ─────────
    public void Show()
    {
        if (loadingRoot == null || loadingSlider == null || percentText == null)
        {
            // 参照未設定なら何もしない（要求通り非常用UIは作らない）
            return;
        }

        if (!isShowing)
        {
            isShowing = true;
            loadingRoot.SetActive(true);
        }

        sessionCts?.Cancel();
        sessionCts?.Dispose();
        sessionCts = new CancellationTokenSource();

        showStartRealtime = Time.realtimeSinceStartup;
        lastUpdateRealtime = showStartRealtime;
        displayed = 0f;
        ApplyUI();

        StartFakeProgress(); // 実進捗が来たらSmoothToで上書きされる
    }

    private void Hide()
    {
        if (!isShowing) return;
        CancelAllTasks();
        if (loadingRoot) loadingRoot.SetActive(false);
        isShowing = false;
    }

    private void CancelAllTasks()
    {
        tweenCts?.Cancel(); tweenCts?.Dispose(); tweenCts = null;
        fakeCts ?.Cancel(); fakeCts ?.Dispose();  fakeCts  = null;
        sessionCts?.Cancel(); sessionCts?.Dispose(); sessionCts = null;
    }

    // ───────── 進捗/フェイク(UniTask) ─────────
    private async UniTaskVoid TrackProgressWithFallbackAsync(AsyncOperation op, CancellationToken token)
    {
        float theoretical = displayed;
        while (op != null && !op.isDone && !token.IsCancellationRequested)
        {
            // UnityのAsyncOperation.progressは0..0.9
            float target = Mathf.Clamp01(op.progress / 0.9f);

            if (Mathf.Abs(target - theoretical) < 0.0001f)
            {
                theoretical = target;
                await MonitorStuckOrAdvanceAsync(op, token);
            }
            else
            {
                theoretical = target;
                SmoothTo(theoretical);
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }
        if (!token.IsCancellationRequested) SmoothTo(1f);
    }

    private async UniTask MonitorStuckOrAdvanceAsync(AsyncOperation op, CancellationToken token)
    {
        float start = Time.realtimeSinceStartup;
        while (op != null && !op.isDone && !token.IsCancellationRequested)
        {
            float target = Mathf.Clamp01(op.progress / 0.9f);
            if (Mathf.Abs(target - displayed) > 0.0001f)
            {
                SmoothTo(target);
                return;
            }

            if (Time.realtimeSinceStartup - start >= stuckTimeout)
            {
                StartFakeProgress();
                return;
            }
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
    }

    private void StartFakeProgress()
    {
        fakeCts?.Cancel();
        fakeCts?.Dispose();
        if (sessionCts == null) return;

        fakeCts = CancellationTokenSource.CreateLinkedTokenSource(sessionCts.Token);
        _ = IndeterminateToNinetyAsync(displayed, fakeCts.Token);
    }

    private async UniTaskVoid IndeterminateToNinetyAsync(float startFrom, CancellationToken token)
    {
        float theoretical = Mathf.Clamp(startFrom, 0f, 0.9f);
        while (theoretical < 0.9f && !token.IsCancellationRequested)
        {
            theoretical = Mathf.MoveTowards(theoretical, 0.9f, fakeProgressSpeed * Time.unscaledDeltaTime);
            SmoothTo(theoretical);
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
    }

    private async UniTaskVoid CloseAfterDelayAsync()
    {
        if (!isShowing) return;

        float elapsed = Time.realtimeSinceStartup - showStartRealtime;
        if (elapsed < minShowTime)
            await UniTask.Delay(TimeSpan.FromSeconds(minShowTime - elapsed), DelayType.Realtime);

        await TweenToAsync(1f, resumeTweenDuration);
        if (holdFullTime > 0f)
            await UniTask.Delay(TimeSpan.FromSeconds(holdFullTime), DelayType.Realtime);

        Hide();
    }

    // ───────── スムージング/ツイーン ─────────
    private void SmoothTo(float target)
    {
        target = Mathf.Clamp01(Mathf.Max(target, displayed)); // 後退禁止

        float now = Time.realtimeSinceStartup;
        float gap = now - lastUpdateRealtime;
        lastUpdateRealtime = now;

        if (gap > bigGapThreshold)
        {
            tweenCts?.Cancel();
            tweenCts?.Dispose();
            if (sessionCts == null) return;

            tweenCts = CancellationTokenSource.CreateLinkedTokenSource(sessionCts.Token);
            _ = TweenToAsync(target, resumeTweenDuration, tweenCts.Token);
        }
        else
        {
            displayed = Mathf.MoveTowards(displayed, target, Time.unscaledDeltaTime * 0.8f);
            ApplyUI();
        }
    }

    private async UniTask TweenToAsync(float target, float duration, CancellationToken token = default)
    {
        float start = displayed;
        float t = 0f;
        while (t < duration && !token.IsCancellationRequested)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / duration);
            displayed = Mathf.Lerp(start, target, u);
            ApplyUI();
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
        if (!token.IsCancellationRequested)
        {
            displayed = target;
            ApplyUI();
        }
    }

    private void ApplyUI()
    {
        if (loadingSlider) loadingSlider.value = displayed;
        if (percentText)  percentText.text  = Mathf.RoundToInt(displayed * 100f) + "%";
    }
}
