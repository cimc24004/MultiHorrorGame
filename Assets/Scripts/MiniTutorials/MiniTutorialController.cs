using TMPro;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MiniTutorialController : Singleton<MiniTutorialController>
{
    [Header("UI Refs")]
    [Required, SerializeField] private TextMeshProUGUI TitleTextUI;
    [Required, SerializeField] private TextMeshProUGUI DescriptionTextUI;
    [Required, SerializeField] private GameObject CanvasObj;
    [Required, SerializeField] private GameObject Panel;

    [Header("Auto Close (秒)")]
    [SerializeField] private float ToCloseMinute = 10f; // 実質「秒」
    [ShowInInspector, ReadOnly] private float ToCloseMinuteNow = 0f;

    [Header("Slide X 設定 (RectTransform.anchoredPosition.x)")]
    [SerializeField] private float OpenX = 0f;
    [SerializeField] private float ClosedX = -1600f;

    [Header("Tween 設定")]
    [SerializeField] private float OpenDuration = 0.4f;
    [SerializeField] private float CloseDuration = 0.3f;
    [SerializeField] private Ease OpenEase = Ease.OutCubic;
    [SerializeField] private Ease CloseEase = Ease.InCubic;

    [ShowInInspector, ReadOnly] private bool OpenNow = false;

    private RectTransform _panelRect;
    private Tween _tween;
    private Action _afterClose;
    private float _lockedY;
    private MiniTutorialDatabases miniTutorialDatabases;

    // 既読管理
    private readonly HashSet<int> _seenThisScene = new HashSet<int>();
    private const string SeenKeyPrefix = "MiniTutSeen_"; // PlayerPrefs key

    private void Start()
    {
        // 必須参照の存在を確認（ダメなら即例外）
        if (Panel == null) throw new NullReferenceException("[MiniTutorialController] Panel is null.");
        if (CanvasObj == null) throw new NullReferenceException("[MiniTutorialController] CanvasObj is null.");
        if (TitleTextUI == null) throw new NullReferenceException("[MiniTutorialController] TitleTextUI is null.");
        if (DescriptionTextUI == null) throw new NullReferenceException("[MiniTutorialController] DescriptionTextUI is null.");

        _panelRect = Panel.GetComponent<RectTransform>();
        if (_panelRect == null) throw new MissingComponentException("[MiniTutorialController] Panel has no RectTransform.");

        _lockedY = _panelRect.anchoredPosition.y;
        _panelRect.anchoredPosition = new Vector2(ClosedX, _lockedY);

        miniTutorialDatabases = MiniTutorialDatabases.instance;
        if (miniTutorialDatabases == null) throw new NullReferenceException("[MiniTutorialController] MiniTutorialDatabases.instance is null.");

        CanvasObj.SetActive(false);
    }

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void OnDestroy()
    {
        // ここは実用上nullになり得るのでだけ安全に
        _tween?.Kill();
    }

    private void Update()
    {
        if (OpenNow)
        {
            ToCloseMinuteNow += Time.deltaTime;
            if (ToCloseMinuteNow > ToCloseMinute)
            {
                Close();
            }
        }
        else
        {
            ToCloseMinuteNow = 0f;
        }
    }

    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        _seenThisScene.Clear(); // シーン内一度きりをリセット
    }

    /// <summary>ID指定で表示（onlyOnce判定込み）</summary>
    public void ShowById(int id)
    {
        var e = miniTutorialDatabases.Get(id);
        if (e == null) throw new KeyNotFoundException($"[MiniTutorialController] DBにID {id} が無い。");

        // 既読チェック（黙ってreturnしない。条件満たさなければ何も起こらないのが仕様）
        if (e.onlyOnce == MiniTutorialEntryData.OnlyOnce.ForEver && PlayerPrefs.GetInt(SeenKeyPrefix + id, 0) == 1)
            return;
        if (e.onlyOnce == MiniTutorialEntryData.OnlyOnce.Scene && _seenThisScene.Contains(id))
            return;

        Action openAct = () => OpenAndMark(id, e.title, e.description, e.onlyOnce);

        if (OpenNow || (_tween != null && _tween.IsActive() && _tween.IsPlaying()))
        {
            _afterClose = openAct; // 閉じた後に最新を開く
            Close();
            return;
        }

        openAct.Invoke();
    }

    [Button]
    public void Show(string title, string description)
    {
        if (OpenNow || (_tween != null && _tween.IsActive() && _tween.IsPlaying()))
        {
            _afterClose = () => Open(title, description);
            Close();
            return;
        }
        Open(title, description);
    }

    private void OpenAndMark(int id, string title, string description, MiniTutorialEntryData.OnlyOnce mode)
    {
        MarkSeen(id, mode);
        Open(title, description);
    }

    private void MarkSeen(int id, MiniTutorialEntryData.OnlyOnce mode)
    {
        switch (mode)
        {
            case MiniTutorialEntryData.OnlyOnce.ForEver:
                PlayerPrefs.SetInt(SeenKeyPrefix + id, 1);
                PlayerPrefs.Save();
                break;
            case MiniTutorialEntryData.OnlyOnce.Scene:
                _seenThisScene.Add(id);
                break;
        }
    }

    private void Open(string _title, string _description)
    {
        // ここから先は必須参照がnullなら即死する仕様（fail-fast）
        _tween?.Kill();

        TitleTextUI.SetText(_title);
        DescriptionTextUI.SetText(_description);

        CanvasObj.SetActive(true);

        _panelRect.anchoredPosition = new Vector2(ClosedX, _lockedY);

        OpenNow = true;
        ToCloseMinuteNow = 0f;

        _tween = _panelRect
            .DOAnchorPosX(OpenX, OpenDuration)
            .SetEase(OpenEase);
    }

    private void Close()
    {
        _tween?.Kill();

        OpenNow = false;
        ToCloseMinuteNow = 0f;

        _tween = _panelRect
            .DOAnchorPosX(ClosedX, CloseDuration)
            .SetEase(CloseEase)
            .OnComplete(() =>
            {
                CanvasObj.SetActive(false);

                var cb = _afterClose;
                _afterClose = null;
                cb?.Invoke();
            });
    }

    [Button("CloseImmediate")]
    public void CloseImmediate()
    {
        _tween?.Kill();
        OpenNow = false;
        ToCloseMinuteNow = 0f;
        _panelRect.anchoredPosition = new Vector2(ClosedX, _lockedY);
        CanvasObj.SetActive(false);
        _afterClose = null;
    }
}
