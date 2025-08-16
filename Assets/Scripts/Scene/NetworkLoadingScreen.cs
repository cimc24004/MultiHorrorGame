// ============================
// File: NetworkLoadingScreen.cs (UniTask版)
// ============================
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;

[DefaultExecutionOrder(-10000)]
public class NetworkLoadingScreen : MonoBehaviour
{
    public static NetworkLoadingScreen Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject loadingRoot;
    [SerializeField] private Slider loadingSlider;   // Min=0, Max=1
    [SerializeField] private TMP_Text percentText;

    [Header("Tuning")]
    [SerializeField] private float fakeProgressSpeed = 0.25f; // /sec →0.9
    [SerializeField] private float stuckTimeout = 0.5f;       // 実進捗が止まったらfakeへ

    [Header("UX")]
    [SerializeField] private float minShowTime = 0.6f;        // 最低表示時間
    [SerializeField] private float holdFullTime = 0.25f;      // 100%表示キープ
    [SerializeField] private float resumeTweenDuration = 0.35f; // 復帰時の追いつき
    [SerializeField] private float bigGapThreshold = 0.20f;     // 大きい隙間ならTween

    private bool isShowing;
    private float showStartRealtime;
    private float lastUpdateRealtime;
    private float displayed; // 実際に表示している値 (0..1)

    // タスク制御
    private CancellationTokenSource _sessionCts;   // この表示サイクル全体のCT
    private CancellationTokenSource _tweenCts;     // 追いつきTween用
    private CancellationTokenSource _fakeCts;      // フェイク進捗用

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
            nm.SceneManager.OnSceneEvent += OnSceneEventHandler;  // ←これ一本でOK
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

    // Unityフォールバック：アクティブシーンが切り替わったら閉じる方向へ
    private void OnActiveSceneChanged(Scene prev, Scene next)
    {
        if (!isShowing) return;
        _ = CloseAfterDelayAsync(); // fire-and-forget
    }

    // ★ポイント：ClientIdでフィルタしない（全端末でShowする）
    private void OnSceneEventHandler(SceneEvent e)
    {
        switch (e.SceneEventType)
        {
            case SceneEventType.Load:
            {
                Show(); // 全員表示
                var op = e.AsyncOperation;
                if (op != null)
                {
                    _ = TrackProgressWithFallbackAsync(op, _sessionCts.Token);
                }
                else
                {
                    // AsyncOperation取れないケースはフェイクで0.9まで
                    StartFakeProgress();
                }
                break;
            }

            case SceneEventType.LoadComplete:
            {
                // 自分のロードが完了したら1.0へ寄せる
                var nm = NetworkManager.Singleton;
                if (nm != null && e.ClientId == nm.LocalClientId)
                {
                    SmoothTo(1f);
                }
                break;
            }

            case SceneEventType.LoadEventCompleted:
            {
                // 全員完了でクローズ
                _ = CloseAfterDelayAsync();
                break;
            }
        }
    }

    // ──────────────────────────────────────
    // 表示/非表示
    // ──────────────────────────────────────
    public void Show()
    {
        if (!isShowing)
        {
            isShowing = true;
            if (loadingRoot && !loadingRoot.activeSelf) loadingRoot.SetActive(true);
        }

        // セッション更新
        _sessionCts?.Cancel();
        _sessionCts?.Dispose();
        _sessionCts = new CancellationTokenSource();

        // リセット
        showStartRealtime = Time.realtimeSinceStartup;
        lastUpdateRealtime = showStartRealtime;
        displayed = 0f;
        ApplyUI();

        // フェイク進捗開始（実進捗が来たらSmoothToが上書き）
        StartFakeProgress();
    }

    private void Hide()
    {
        if (!isShowing) return;
        CancelAllTasks();
        if (loadingRoot && loadingRoot.activeSelf) loadingRoot.SetActive(false);
        isShowing = false;
    }

    private void CancelAllTasks()
    {
        _tweenCts?.Cancel(); _tweenCts?.Dispose(); _tweenCts = null;
        _fakeCts ?.Cancel(); _fakeCts ?.Dispose(); _fakeCts  = null;
        _sessionCts?.Cancel(); _sessionCts?.Dispose(); _sessionCts = null;
    }

    // ──────────────────────────────────────
    // 進捗トラッキング（UniTask）
    // ──────────────────────────────────────
    private async UniTaskVoid TrackProgressWithFallbackAsync(AsyncOperation op, CancellationToken token)
    {
        float theoretical = displayed;

        while (op != null && !op.isDone && !token.IsCancellationRequested)
        {
            // Unityのop.progressは0..0.9まで
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

        if (!token.IsCancellationRequested)
        {
            SmoothTo(1f);
        }
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

            // 一定時間止まったらフェイク進捗
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
        // 既存フェイクをキャンセル
        _fakeCts?.Cancel();
        _fakeCts?.Dispose();
        if (_sessionCts == null) return;

        _fakeCts = CancellationTokenSource.CreateLinkedTokenSource(_sessionCts.Token);
        _ = IndeterminateToNinetyAsync(displayed, _fakeCts.Token);
    }

    // フェイク進捗：0.9まで
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

        // 最低表示時間を保証（リアルタイム）
        float elapsed = Time.realtimeSinceStartup - showStartRealtime;
        if (elapsed < minShowTime)
            await UniTask.Delay(TimeSpan.FromSeconds(minShowTime - elapsed), DelayType.Realtime);

        // 100%まで滑らかに到達して、少し見せてから閉じる
        await TweenToAsync(1f, resumeTweenDuration);
        if (holdFullTime > 0f)
            await UniTask.Delay(TimeSpan.FromSeconds(holdFullTime), DelayType.Realtime);

        Hide();
    }

    // ──────────────────────────────────────
    // スムージング/ツイーン
    // ──────────────────────────────────────
    private void SmoothTo(float target)
    {
        target = Mathf.Clamp01(Mathf.Max(target, displayed)); // 後退しない

        float now = Time.realtimeSinceStartup;
        float gap = now - lastUpdateRealtime;
        lastUpdateRealtime = now;

        if (gap > bigGapThreshold)
        {
            // 大きく止まってた → 短時間Tweenで追いつく
            _tweenCts?.Cancel();
            _tweenCts?.Dispose();
            if (_sessionCts == null) return;

            _tweenCts = CancellationTokenSource.CreateLinkedTokenSource(_sessionCts.Token);
            _ = TweenToAsync(target, resumeTweenDuration, _tweenCts.Token);
        }
        else
        {
            // その場で少しずつ追従
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
        if (percentText) percentText.text = Mathf.RoundToInt(displayed * 100f) + "%";
    }
}
