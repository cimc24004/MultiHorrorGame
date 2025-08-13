using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class LobbyNetworkController : NetworkBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private Button StartHostButton;
    [SerializeField] private Button StartClientButton;
    [SerializeField] private Button GameStartButton;
    [SerializeField] private GameObject PlayerClientContents; // 各プレイヤー行のプレハブ
    [SerializeField] private Transform PlayerClientsAll;      // 行を並べる親
    [SerializeField] private TMP_InputField m_joinLobbyID_Field; // LobbyID表示/入力
    [SerializeField] private GameObject LobbyNetworkErrorMessageCanvasObject;
    [SerializeField] private TextMeshProUGUI errorTextMessageUI; //エラー表示用TextUI
    // クライアントID→UI行の対応表（重複生成防止＆Remove用）
    private readonly Dictionary<ulong, GameObject> _rows = new();

    private SessionController sessionController; // SessionController 参照

    void Start()
    {
        // ボタン系（ローカルUI）
        if (StartHostButton) StartHostButton.onClick.AddListener(StartHost);
        if (StartClientButton) StartClientButton.onClick.AddListener(StartClient);
        if (GameStartButton) GameStartButton.onClick.AddListener(() =>
        {
            GameStartButton.gameObject.SetActive(false);
            if (IsServer)
            {
                NetworkManager.Singleton.SceneManager.LoadScene("Main", UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
        });
        SteamLobby.LobbyConnectionError += OnLobbyConnectionError;
    }

    public override void OnNetworkSpawn()
    {
        StartCoroutine(InitAfterSessionController());
    }

    private IEnumerator InitAfterSessionController()
    {
        // SessionController 検索＆待機
        while (sessionController == null)
        {
            var go = GameObject.FindWithTag("SessionController");
            if (go != null) sessionController = go.GetComponent<SessionController>();
            yield return null;
        }

        // ★ ここでNetworkList購読（クライアント/サーバー両方）
        sessionController.connectedMembers.OnListChanged += OnMemberListChangedClient;

        // 既存メンバー分をまずUIへ反映
        foreach (var id in sessionController.connectedMembers)
        {
            SpawnOrUpdateRow(id);
        }

        if (IsServer)
        {
            // 既に繋がっているクライアントIDをNetworkListへ反映（重複防止チェック）
            foreach (var id in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if (!ContainsId(sessionController.connectedMembers, id))
                {
                    sessionController.connectedMembers.Add(id);
                }
            }

            // 接続イベント（サーバー側のみ）
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        }

        // 表示用：ロビーID
        if (m_joinLobbyID_Field && SteamLobby.Instance != null)
        {
            m_joinLobbyID_Field.text = SteamLobby.Instance.LobbyID.ToString();
        }
    }
    private void OnLobbyConnectionError(string _Message)
    {
        ShowLobbyConnectErrorUI(_Message);
    }
    private void ShowLobbyConnectErrorUI(string _message)
    {
        errorTextMessageUI.SetText(_message);
        LobbyNetworkErrorMessageCanvasObject.gameObject.SetActive(true);
    }
    private bool ContainsId(NetworkList<ulong> list, ulong id)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == id) return true;
        }
        return false;
    }

    private void HandleClientConnected(ulong clientId)
    {
        // サーバーがNetworkListを唯一のソースとしてAdd
        if (sessionController != null && sessionController.connectedMembers != null && !ContainsId(sessionController.connectedMembers, clientId))
        {

            sessionController.connectedMembers.Add(clientId);
        }
    }

    private void OnMemberListChangedClient(NetworkListEvent<ulong> e)
    {
        this.LobbyNetworkErrorMessageCanvasObject.gameObject.SetActive(false);
        Debug.Log($"[Lobby] OnListChanged: {e.Type} / {e.Value}");

        switch (e.Type)
        {
            case NetworkListEvent<ulong>.EventType.Add:
                SpawnOrUpdateRow(e.Value);
                break;
            case NetworkListEvent<ulong>.EventType.Remove:
                RemoveRow(e.Value);
                break;
            case NetworkListEvent<ulong>.EventType.Insert:
            case NetworkListEvent<ulong>.EventType.RemoveAt:
            case NetworkListEvent<ulong>.EventType.Value:
            case NetworkListEvent<ulong>.EventType.Clear:
                // 必要に応じてハンドリング
                break;
        }
    }

    private void SpawnOrUpdateRow(ulong clientId)
    {
        if (_rows.TryGetValue(clientId, out var row) && row != null)
        {
            // 既存更新
            var box = row.GetComponent<PlayerClientBox>();
            if (box) box.PlayerOnClientConnectedSetText($"プレイヤーID: {clientId}");
            return;
        }
        StartHostButton.gameObject.SetActive(false);
        StartClientButton.gameObject.SetActive(false);
        if (PlayerClientContents == null || PlayerClientsAll == null)
        {
            Debug.LogError("[Lobby] PlayerClientContents / PlayerClientsAll が未設定");
            return;
        }

        // 新規生成
        var go = GameObject.Instantiate(PlayerClientContents, PlayerClientsAll);
        var pcb = go.GetComponent<PlayerClientBox>();
        if (pcb) pcb.PlayerOnClientConnectedSetText($"プレイヤーID: {clientId}");
        _rows[clientId] = go;
    }

    private void RemoveRow(ulong clientId)
    {
        if (_rows.TryGetValue(clientId, out var row) && row != null)
        {
            GameObject.Destroy(row);
        }
        _rows.Remove(clientId);
    }

    private void ClearAllRows()
    {
        foreach (var kv in _rows)
        {
            if (kv.Value) GameObject.Destroy(kv.Value);
        }
        _rows.Clear();
    }

    public override void OnNetworkDespawn()
    {
        // クリーンアップ（OnDestroyより先に呼ばれるケースもある）
        CleanupSubscriptions();
    }

    private void OnDestroy()
    {
        CleanupSubscriptions();
        ClearAllRows();
    }

    private void CleanupSubscriptions()
    {
        if (sessionController != null && sessionController.connectedMembers != null)
        {
            sessionController.connectedMembers.OnListChanged -= OnMemberListChangedClient;
        }
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        }
    }

    // ──────────────────────────────────────
    // ボタン：ロビー作成（ホスト）
    // ──────────────────────────────────────
    public void StartHost()
    {

        SteamLobby.Instance.CreateLobby();
        ulong hash = NetworkManager.Singleton.NetworkConfig.GetConfig(false);
        Debug.Log($"ConfigHash={hash:X8}");
    }

    // ──────────────────────────────────────
    // ボタン：LobbyID を入力して参加（クライアント）
    // ──────────────────────────────────────
    public void StartClient()
    {

        if (m_joinLobbyID_Field == null)
        {
            Debug.LogError("JoinLobbyID Field 未設定");
            return;
        }

        string idText = m_joinLobbyID_Field.text.Trim();
        if (!ulong.TryParse(idText, out ulong lobbyId))
        {
            Debug.LogError($"JoinLobbyID パース失敗 → {idText}");
            return;
        }

        ulong hash = NetworkManager.Singleton.NetworkConfig.GetConfig(false);
        Debug.Log($"ConfigHash={hash:X8}");

        SteamLobby.Instance.JoinLobby(lobbyId);
        Debug.Log($"StartClient() → JoinLobby({lobbyId})");
    }
}