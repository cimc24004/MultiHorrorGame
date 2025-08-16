// ============================
// File: NetworkLoadingScreen.cs
// ============================
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System;
[DefaultExecutionOrder(-10000)]
public class NetworkLoadingScreen : MonoBehaviour
{
    public static NetworkLoadingScreen Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject loadingRoot;
    [SerializeField] private Slider loadingSlider;   // Min=0, Max=1
    [SerializeField] private TMP_Text percentText;

    [Header("Tuning")]
    [SerializeField] private float fadeOutDelay = 0.15f;      // realtime
    [SerializeField] private float fakeProgressSpeed = 0.25f; // /sec →0.9
    [SerializeField] private float stuckTimeout = 0.5f;       // 実進捗が止まったらfakeへ

    [Header("UX")]
    [SerializeField] private float minShowTime = 0.6f;        // 最低表示時間
    [SerializeField] private float holdFullTime = 0.25f;      // 100%表示キープ
    [SerializeField] private float finishFullSpeed = 3f;      // 100%への仕上げ速度
    [SerializeField] private float resumeTweenDuration = 0.35f; // フリーズ復帰時の追いつき時間
    [SerializeField] private float bigGapThreshold = 0.20f;     // これ以上フレームが空いたらTweenで追いつく

    private Coroutine trackRoutine;
    private Coroutine fakeRoutine;
    private Coroutine tweenRoutine;
    private bool isShowing;
    private float showStartRealtime;
    private float lastUpdateRealtime;
    private float displayed; // 実際に表示している値 (0..1)

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
            nm.SceneManager.OnSceneEvent += HandleOnSceneEvent;
            nm.SceneManager.OnLoadComplete += OnLoadComplete;
            nm.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted; // 旧NGO 4引数版
        }
    }

    void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        var nm = NetworkManager.Singleton;
        if (nm != null && nm.SceneManager != null)
        {
            nm.SceneManager.OnSceneEvent -= HandleOnSceneEvent;
            nm.SceneManager.OnLoadComplete -= OnLoadComplete;
            nm.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
        }
    }

    // Unity フォールバック：シーン切替検知でクローズへ
    private void OnActiveSceneChanged(Scene prev, Scene next)
    {
        if (!isShowing) return;
        StartCoroutine(CloseAfterDelay());
    }

    // NGO: ネットワーク遷移で開始
    // private void OnSceneEvent(SceneEvent e)
    // {
    //     if (e.SceneEventType != SceneEventType.Load /* && e.SceneEventType != SceneEventType.Synchronize */) return;

    //     if (trackRoutine != null) { StopCoroutine(trackRoutine); trackRoutine = null; }

    //     var op = e.AsyncOperation;
    //     if (op != null)
    //     {
    //         trackRoutine = StartCoroutine(TrackProgressWithFallback(op));
    //     }
    //     // op==null のときは Show() 側の fakeRoutine が動いている
    // }
    private async void HandleOnSceneEvent(SceneEvent sceneEvent)
    {
        if (NetworkManager.Singleton.LocalClientId != sceneEvent.ClientId)
        {
            return;
        }

        if (sceneEvent.SceneEventType == SceneEventType.Load)
        {
            Show();
            while (!sceneEvent.AsyncOperation.isDone)
            {
                await UniTask.Yield();
                loadingSlider.value = sceneEvent.AsyncOperation.progress;
            }
        }

        if (sceneEvent.SceneEventType == SceneEventType.LoadComplete)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(2));
            Hide();
        }
    }
    // 自分のロードが終わったらいずれ100%へ
    private void OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode mode)
    {
        if (NetworkManager.Singleton != null && clientId == NetworkManager.Singleton.LocalClientId)
        {
            SmoothTo(1f); // いきなり跳ねないで必ず滑らかに1.0へ
        }
    }

    // 全員完了（旧NGOのシグネチャ）
    private void OnLoadEventCompleted(string sceneName, LoadSceneMode mode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        StartCoroutine(CloseAfterDelay());
    }

    // NGOを使わないローカル遷移だけこれを使う
    public void ShowAndLoadLocally(string sceneName)
    {
        Show();
        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        if (op != null)
        {
            if (trackRoutine != null) { StopCoroutine(trackRoutine); trackRoutine = null; }
            trackRoutine = StartCoroutine(TrackProgressWithFallback(op));
            StartCoroutine(CloseAfterDelayWhenDone(op));
        }
    }

    // 実進捗 + フリーズ検知 → fakeへ、さらに復帰時の“追いつきTween”
    private IEnumerator TrackProgressWithFallback(AsyncOperation op)
    {
        float theoretical = displayed; // 目標値（理論値）

        while (op != null && !op.isDone)
        {
            float target = Mathf.Clamp01(op.progress / 0.9f);

            // 実進捗が一定時間変化しない → fakeに切替
            if (Mathf.Abs(target - theoretical) < 0.0001f)
            {
                theoretical = target;
                yield return MonitorStuckOrAdvance(op);
                continue;
            }

            theoretical = target;
            SmoothTo(theoretical);
            yield return null;
        }

        SmoothTo(1f);
    }

    private IEnumerator MonitorStuckOrAdvance(AsyncOperation op)
    {
        float timer = 0f;
        while (op != null && !op.isDone)
        {
            float target = Mathf.Clamp01(op.progress / 0.9f);
            if (Mathf.Abs(target - displayed) > 0.0001f) // 動いた
            {
                SmoothTo(target);
                yield break;
            }

            timer += Time.unscaledDeltaTime;
            if (timer >= stuckTimeout)
            {
                // fakeへ（0.9までゆっくり、復帰時はSmoothに追いつく）
                if (fakeRoutine == null) fakeRoutine = StartCoroutine(IndeterminateToNinety(displayed));
                yield break;
            }

            yield return null;
        }
    }

    // fake進捗：0.9まで
    private IEnumerator IndeterminateToNinety(float startFrom)
    {
        float theoretical = Mathf.Clamp(startFrom, 0f, 0.9f);
        while (theoretical < 0.9f)
        {
            theoretical = Mathf.MoveTowards(theoretical, 0.9f, fakeProgressSpeed * Time.unscaledDeltaTime);
            SmoothTo(theoretical);
            yield return null;
        }
    }

    private IEnumerator CloseAfterDelayWhenDone(AsyncOperation op)
    {
        while (op != null && !op.isDone) yield return null;
        yield return StartCoroutine(CloseAfterDelay());
    }

    private IEnumerator CloseAfterDelay()
    {
        // 最低表示時間を保証
        float elapsed = Time.realtimeSinceStartup - showStartRealtime;
        if (elapsed < minShowTime)
            yield return new WaitForSecondsRealtime(minShowTime - elapsed);

        // 進捗コルーチン停止
        if (trackRoutine != null) { StopCoroutine(trackRoutine); trackRoutine = null; }
        if (fakeRoutine  != null) { StopCoroutine(fakeRoutine);  fakeRoutine  = null; }
        if (tweenRoutine != null) { StopCoroutine(tweenRoutine); tweenRoutine = null; }

        // 100%まで滑らかに到達して、少し見せてから閉じる
        yield return TweenTo(1f, resumeTweenDuration);
        if (holdFullTime > 0f)
            yield return new WaitForSecondsRealtime(holdFullTime);

        Hide();
    }

    public void Show()
    {
        if (!isShowing)
        {
            isShowing = true;
            if (loadingRoot && !loadingRoot.activeSelf) loadingRoot.SetActive(true);
            showStartRealtime = Time.realtimeSinceStartup;
            lastUpdateRealtime = showStartRealtime;
            displayed = 0f;
            ApplyUI();

            if (fakeRoutine != null) { StopCoroutine(fakeRoutine); }
            fakeRoutine = StartCoroutine(IndeterminateToNinety(displayed));
        }
        else
        {
            showStartRealtime = Time.realtimeSinceStartup;
            lastUpdateRealtime = showStartRealtime;
            displayed = 0f;
            ApplyUI();
        }
    }

    private void Hide()
    {
        if (!isShowing) return;
        if (trackRoutine != null) { StopCoroutine(trackRoutine); trackRoutine = null; }
        if (fakeRoutine  != null) { StopCoroutine(fakeRoutine);  fakeRoutine  = null; }
        if (tweenRoutine != null) { StopCoroutine(tweenRoutine); tweenRoutine = null; }
        if (loadingRoot && loadingRoot.activeSelf) loadingRoot.SetActive(false);
        isShowing = false;
    }

    // 大きいフレーム隙間があっても瞬間ジャンプさせず、短時間で追いつく
    private void SmoothTo(float target)
    {
        target = Mathf.Clamp01(Mathf.Max(target, displayed)); // 後退しない

        float now = Time.realtimeSinceStartup;
        float gap = now - lastUpdateRealtime;
        lastUpdateRealtime = now;

        if (gap > bigGapThreshold)
        {
            if (tweenRoutine != null) StopCoroutine(tweenRoutine);
            tweenRoutine = StartCoroutine(TweenTo(target, resumeTweenDuration));
        }
        else
        {
            displayed = Mathf.MoveTowards(displayed, target, Time.unscaledDeltaTime * 0.8f);
            ApplyUI();
        }
    }

    private IEnumerator TweenTo(float target, float duration)
    {
        float start = displayed;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / duration);
            displayed = Mathf.Lerp(start, target, u);
            ApplyUI();
            yield return null;
        }
        displayed = target;
        ApplyUI();
        tweenRoutine = null;
    }

    private void ApplyUI()
    {
        if (loadingSlider) loadingSlider.value = displayed;
        if (percentText) percentText.text = Mathf.RoundToInt(displayed * 100f) + "%";
    }
}
