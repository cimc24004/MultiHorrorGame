using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using Sirenix.OdinInspector;

public class GameProgressController : NetworkBehaviour
{
    // ====== 手順の定義 ======
    public enum StepType { PrayCount, GetItem, DeliverToSakura, Escape }

    [Serializable]
    public class Step
    {
        [HorizontalGroup("h", 30), LabelText("有効")]
        public bool enabled = true;

        [HorizontalGroup("h"), HideLabel]
        public StepType type = StepType.PrayCount;

        [ShowIf("@type == StepType.PrayCount")]
        [LabelText("必要祈り数"), MinValue(1)]
        public int prayTarget = 5;

        [ShowIf("@type == StepType.PrayCount")]
        [LabelText("入ったら祈り数リセット")]
        public bool resetPrayCountOnEnter = true;

        [ShowIf("@type == StepType.GetItem")]
        [LabelText("手順開始時にアイテム出現")]
        public bool spawnItemOnEnter = false;

        [LabelText("TODOキー（UI辞書キー）")]
        [ValueDropdown(nameof(TodoKeysDropdown))]
        public string todoKey = "OnPrayedButsudan";

        private static IEnumerable<string> TodoKeysDropdown => new[]
        {
            "OnPrayedButsudan", "OnItemGeted", "OnDeliverSakura", "Escape"
        };
    }

    [Header("フロー（並び替え自由／無効化でスキップ）")]
    [ListDrawerSettings(DraggableItems = true, Expanded = true, ShowIndexLabels = true)]
    public List<Step> Flow = new();

    // ====== 従来のメンバ ======
    [Header("現在のお祈り数")]
    public NetworkVariable<int> PrayCountNow = new NetworkVariable<int>(0);

    [Header("（後方互換用）デフォルト必要祈り数")]
    public int NeedPrayCount = 5;

    public bool DeliverItemGeted;
    public bool DeliveredToSakura;

    public NetworkVariable<bool> CanEscape = new NetworkVariable<bool>(false);

    [SerializeField] private List<ulong> ArrivedExitPlayers = new List<ulong>();

    private GameProgressUIData game_progress_ui_data;
    private ScoreTimeManager scoreTimeManager;
    private GameStateController gameStateController;

    [Header("アイテム出現に使う（既存コンポーネントを参照）")]
    [SerializeField] private PrayToDeliverItemAppearPlace deliverItemSpawner;

    // いまの手順インデックス（Flow内のindex、無効は飛ばす）
    private NetworkVariable<int> currentStepIndex = new NetworkVariable<int>(-1);

    // ==== TodoUIの見た目制御 ====
    private enum TodoVisualState { Hidden, Dormant, Active, Completed }
    private readonly Dictionary<string, string> _todoOriginalText = new();

    // ====== ライフサイクル ======
    public override void OnNetworkSpawn()
    {
        gameStateController = GameStateController.instance;
        scoreTimeManager = ScoreTimeManager.Instance;
        game_progress_ui_data = GetComponent<GameProgressUIData>();

        // 初期UI
        game_progress_ui_data.ButsudanCountNowTextUI.text = $"{PrayCountNow.Value}/{NeedPrayCount}";
        game_progress_ui_data.ProgressButtonAfterGameClear.onClick.AddListener(ProgressAfterGameClear);

        // まず全部 Dormant/Hidden に初期化（Flow未開始）
        RefreshTodoUIFlowClient(currentStepIndex.Value);

        // 値変更コールバック
        PrayCountNow.OnValueChanged += PrayCountNowOnValueChanged;
        currentStepIndex.OnValueChanged += OnStepIndexChangedClient;

        // フロー開始（サーバーのみで決定）
        if (IsServer)
        {
            StartFlowServer();
        }

        GameStart();
    }

    private void OnDestroy()
    {
        PrayCountNow.OnValueChanged -= PrayCountNowOnValueChanged;
        currentStepIndex.OnValueChanged -= OnStepIndexChangedClient;
    }

    public void GameStart()
    {
        scoreTimeManager.MeasureTimeStart();
    }

    // ====== フロー管理 ======
    private void StartFlowServer()
    {
        var first = NextEnabledIndex(-1);
        currentStepIndex.Value = first;
        EnterStepServer(first, previous: -1);
    }

    private int NextEnabledIndex(int from)
    {
        for (int i = from + 1; i < Flow.Count; i++)
            if (Flow[i].enabled) return i;
        return -1; // もうない
    }

    private Step CurrentStep => (currentStepIndex.Value >= 0 && currentStepIndex.Value < Flow.Count)
        ? Flow[currentStepIndex.Value]
        : null;

    private Step PeekNextStep()
    {
        int n = NextEnabledIndex(currentStepIndex.Value);
        return (n >= 0) ? Flow[n] : null;
    }

    // 手順開始（サーバー）
    private void EnterStepServer(int index, int previous)
    {
        if (index < 0 || index >= Flow.Count)
        {
            GameClear();
            return;
        }

        var step = Flow[index];

        if (step.type == StepType.PrayCount)
        {
            NeedPrayCount = Mathf.Max(1, step.prayTarget);
            if (step.resetPrayCountOnEnter) PrayCountNow.Value = 0;
        }

        if (step.type == StepType.GetItem && step.spawnItemOnEnter && deliverItemSpawner)
        {
            // サーバーのみ呼ぶ
            deliverItemSpawner.DeliverItemAppear();
        }

        if (step.type == StepType.Escape)
        {
            // Escapeに入った瞬間に脱出解放（Deliverをスキップする構成でもOK）
            CanEscape.Value = true;
        }

        bool isPray = (step.type == StepType.PrayCount);
        int target = isPray ? NeedPrayCount : 0;

        // RPC（Nullable使わない）
        StepChangedClientRpc(
            previous >= 0 && previous < Flow.Count ? Flow[previous].todoKey : string.Empty,
            step.todoKey,
            isPray,
            target,
            PrayCountNow.Value,
            index // ← currentIndex を送る（ネットワーク変数の同期順に依存しない）
        );
    }

    // Flow切替時のUI更新（全端末）
    [ClientRpc]
    private void StepChangedClientRpc(
        string prevTodoKey,
        string newTodoKey,
        bool isPrayStep,
        int newPrayTarget,
        int prayNow,
        int currentIndex
    )
    {
        // 祈り数UI
        if (isPrayStep)
        {
            game_progress_ui_data.ButsudanCountNowTextUI.gameObject.SetActive(true);
            game_progress_ui_data.ButsudanCountNowTextUI.text = $"{prayNow}/{newPrayTarget}";
        }
        else
        {
            game_progress_ui_data.ButsudanCountNowTextUI.gameObject.SetActive(false);
        }

        // Flowに基づき全体を整える
        RefreshTodoUIFlowClient(currentIndex);
    }

    private void OnStepIndexChangedClient(int oldIdx, int newIdx)
    {
        // NetVarの反映タイミングで再描画（保険）
        RefreshTodoUIFlowClient(newIdx);
    }

    private void AdvanceIfMatches(StepType cause)
    {
        if (!IsServer) return;
        var step = CurrentStep;
        if (step == null || step.type != cause) return;

        int prev = currentStepIndex.Value;
        int next = NextEnabledIndex(prev);
        currentStepIndex.Value = next;
        EnterStepServer(next, previous: prev);
    }

    // ====== 既存イベントとの接続 ======
    public void OnPrayedButsudan()
    {
        if (IsServer) PrayCountNow.Value += 1;
    }

    private void PrayCountNowOnValueChanged(int oldValue, int newValue)
    {
        // 全クライアントUI更新（Pray手順中のみ可視）
        game_progress_ui_data.ButsudanCountNowTextUI.text = $"{newValue}/{NeedPrayCount}";

        // 手順達成判定（サーバーのみ）
        if (IsServer && CurrentStep != null && CurrentStep.type == StepType.PrayCount)
        {
            if (newValue >= NeedPrayCount)
            {
                var next = PeekNextStep();
                // 次がGetItem かつ「祈り完了で出現」設定ならここで出す
                if (next != null && next.type == StepType.GetItem && !next.spawnItemOnEnter && deliverItemSpawner)
                {
                    deliverItemSpawner.DeliverItemAppear();
                }

                AdvanceIfMatches(StepType.PrayCount);
            }
        }
    }

    public void OnItemGeted() // ServerRpc 経由でサーバーから呼ばれる想定
    {
        DeliverItemGeted = true;
        // 旧UI個別制御は撤廃、Flow再描画に寄せる（互換で呼んでおく）
        OnItemGetedUIChangeAllClientRpc(currentStepIndex.Value);
        AdvanceIfMatches(StepType.GetItem);
    }

    [ClientRpc]
    private void OnItemGetedUIChangeAllClientRpc(int currentIndex)
    {
        RefreshTodoUIFlowClient(currentIndex);
    }

    public void OnDeliverSakura() // DeliverMemoryPlace から呼ばれる
    {
        DeliveredToSakura = true;
        OnDeliverSakuraUIChangeAllClientRpc(currentStepIndex.Value);
        AdvanceIfMatches(StepType.DeliverToSakura);
    }

    [ClientRpc]
    private void OnDeliverSakuraUIChangeAllClientRpc(int currentIndex)
    {
        RefreshTodoUIFlowClient(currentIndex);
    }

    public void ExitGateArrived(ulong playerClientId)
    {
        if (!IsServer) return;

        if (ArrivedExitPlayers.Contains(playerClientId))
            return;

        ArrivedExitPlayers.Add(playerClientId);

        // 到着プレイヤーだけ非表示＆専用UI
        var sendParams = new ClientRpcSendParams { TargetClientIds = new ulong[] { playerClientId } };
        var playerNO = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(playerClientId);
        if (playerNO) playerNO.gameObject.SetActive(false);
        ExitGateArrivedRequestForArrivedPlayerClientRpc(new ClientRpcParams { Send = sendParams });

        // 全員到着でクリア
        if (ArrivedExitPlayers.Count >= NetworkManager.Singleton.ConnectedClients.Count)
        {
            GameClear();
        }
    }

    [ClientRpc]
    private void ExitGateArrivedRequestForArrivedPlayerClientRpc(ClientRpcParams clientRpcParams)
    {
        var localNO = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(NetworkManager.Singleton.LocalClientId);
        if (localNO) localNO.gameObject.SetActive(false);
        game_progress_ui_data.PlayerArrivedExitPanel.gameObject.SetActive(true);
    }

    public void GameClear()
    {
        Time.timeScale = 0;
        GameClearUIClientRpc();
    }

    public void ProgressAfterGameClear()
    {
        if (IsServer)
        {
            Time.timeScale = 1;
            NetworkManager.Singleton.SceneManager.LoadScene("Lobby", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    [ClientRpc]
    private void GameClearUIClientRpc()
    {
        var time = TimeSpan.FromSeconds(scoreTimeManager.ScoreTimeNow.Value);
        gameStateController.MenuOnGamePlayMode(true);
        string timeText = time.ToString(@"mm\:ss\:ff");
        game_progress_ui_data.PlayerArrivedExitPanel.gameObject.SetActive(false);
        game_progress_ui_data.scoreTimeTextUI.SetText($"{timeText}");
        game_progress_ui_data.GameClearPanel.SetActive(true);
    }

    // ====== UIヘルパ ======
    private void RefreshTodoUIFlowClient(int currentIdx)
    {
        // Flowの“有効な手順”だけ抽出
        var enabledFlow = Flow.Where(s => s.enabled).ToList();
        var orderByKey = new Dictionary<string, int>();
        for (int i = 0; i < enabledFlow.Count; i++)
            orderByKey[enabledFlow[i].todoKey] = i;

        foreach (var kv in game_progress_ui_data.TodoName_with_game_todo_list_ui_datas)
        {
            string key = kv.Key;
            var ui = kv.Value;

            if (!orderByKey.ContainsKey(key))
            {
                // Flow外/無効 → 非表示
                ui.TodoBox.gameObject.SetActive(false);
                continue;
            }

            int orderIndex = orderByKey[key];
            TodoVisualState state;

            if (currentIdx < 0)
                state = TodoVisualState.Dormant;             // Flow開始前
            else if (orderIndex < currentIdx)
                state = TodoVisualState.Completed;
            else if (orderIndex == currentIdx)
                state = TodoVisualState.Active;
            else
                state = TodoVisualState.Dormant;

            ApplyTodoVisual(key, state, orderIndex);
        }

        // UIの並び順もFlow順に並べ替え（親に VerticalLayoutGroup を推奨）
        ReorderTodoBoxesByFlow(enabledFlow);
    }

    private void ReorderTodoBoxesByFlow(List<Step> enabledFlow)
    {
        for (int i = 0; i < enabledFlow.Count; i++)
        {
            var key = enabledFlow[i].todoKey;
            if (game_progress_ui_data.TodoName_with_game_todo_list_ui_datas.TryGetValue(key, out var ui))
            {
                ui.TodoBox.transform.SetSiblingIndex(i);
            }
        }
    }

    private void ApplyTodoVisual(string key, TodoVisualState state, int orderIndex)
    {
        if (!game_progress_ui_data.TodoName_with_game_todo_list_ui_datas.TryGetValue(key, out var ui))
            return;

        // 元テキストのキャッシュ
        if (!_todoOriginalText.TryGetValue(key, out var original) || string.IsNullOrEmpty(original))
        {
            original = ui.TodoTextUI.text;
            _todoOriginalText[key] = original;
        }

        if (state == TodoVisualState.Hidden)
        {
            ui.TodoBox.gameObject.SetActive(false);
            return;
        }
        ui.TodoBox.gameObject.SetActive(true);

        // 表示テキスト
        string prefix = state == TodoVisualState.Completed ? "✔ " : $"{orderIndex + 1}. ";
        ui.TodoTextUI.SetText($"{prefix}{original}");

        // 透明度
        float targetAlpha =
            state == TodoVisualState.Active ? 1f :
            state == TodoVisualState.Dormant ? 0.2f :
            0.45f; // Completed

        ui.TodoTextUI.DOKill();
        ui.TodoTextUI.DOFade(targetAlpha, 0.25f).SetLink(gameObject);

        if (state == TodoVisualState.Active)
        {
            ui.TodoBox.transform.DOKill();
            ui.TodoBox.transform
                .DOPunchScale(Vector3.one * 0.03f, 0.25f, 8, 0.5f)
                .SetLink(gameObject);
        }
    }
}
