using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // ←必須
using TMPro;

public class NetworkLoadingScreen : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject loadingRoot; // 非アクティブ開始
    [SerializeField] private Slider loadingSlider;        // Image(Type=Filled)
    [SerializeField] private TMP_Text percentText;   // 任意

    [SerializeField] private float fadeOutDelay = 0.15f;

    private Coroutine trackRoutine;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (loadingRoot) loadingRoot.SetActive(false);
    }

    void Start()
    {
        var nm = NetworkManager.Singleton;
        if (nm == null || nm.SceneManager == null) return;

        nm.SceneManager.OnSceneEvent += OnSceneEvent;                             // SceneEvent全般
        nm.SceneManager.OnLoadComplete += OnLoadComplete;                         // 各クライアントの完了
        nm.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;             // 全員完了
    }

    void OnDisable()
    {
        var nm = NetworkManager.Singleton;
        if (nm == null || nm.SceneManager == null) return;

        nm.SceneManager.OnSceneEvent -= OnSceneEvent;
        nm.SceneManager.OnLoadComplete -= OnLoadComplete;
        nm.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
    }

    // 進捗バーの元になる AsyncOperation は SceneEvent から取る
    private void OnSceneEvent(SceneEvent e)
    {
        if (e.SceneEventType == SceneEventType.Load)
        {
            Debug.Log("OnSceneEvent --ShowLoaddesu");
            Show();
            if (trackRoutine != null) StopCoroutine(trackRoutine);
            trackRoutine = StartCoroutine(TrackProgress(e.AsyncOperation)); // progressをポーリング
        }
    }

    // ローカル（自分 or 各クライアント）のロード完了通知
    private void OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode mode)
    {
        // 自分のロード完了タイミングで100%にスナップ
        SetProgressUI(1f);
    }

    // すべてのクライアントのロードが完了
    private void OnLoadEventCompleted(string sceneName, LoadSceneMode mode,
                                      List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        StartCoroutine(CloseAfterDelay());
    }

    private IEnumerator TrackProgress(AsyncOperation op)
    {
        if (op == null) yield break;

        // Unityの仕様：.progress は 0.0〜0.9 が主で、最後に isDone とともに 1.0（有効化）になる
        // なので 0.9 で正規化して滑らかに見せる
        while (!op.isDone)
        {
            float t = Mathf.Clamp01(op.progress / 0.9f);
            SetProgressUI(t);
            yield return null;
        }
        SetProgressUI(1f);
    }

    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(fadeOutDelay);
        Hide();
    }

    private void Show()
    {
        if (loadingRoot && !loadingRoot.activeSelf) loadingRoot.SetActive(true);
        SetProgressUI(0f);
    }

    private void Hide()
    {
        if (trackRoutine != null) { StopCoroutine(trackRoutine); trackRoutine = null; }
        if (loadingRoot && loadingRoot.activeSelf) loadingRoot.SetActive(false);
    }

    private void SetProgressUI(float t)
    {
        if (loadingSlider) loadingSlider.value = t;
        if (percentText) percentText.text = Mathf.RoundToInt(t * 100f) + "%";
    }
}
