using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
using Steamworks;
using Unity.Collections;

/* 名前エントリ（NetworkListで同期） */
public struct ClientNameEntry : INetworkSerializable, System.IEquatable<ClientNameEntry>
{
    public ulong ClientId;
    public FixedString64Bytes Name;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref Name);
    }
    public bool Equals(ClientNameEntry other) => ClientId == other.ClientId && Name.Equals(other.Name);
}

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
    [SerializeField] private Button errorOKButtonUI;

    // クライアントID→UI行の対応表（重複生成防止＆Remove用）
    private readonly Dictionary<ulong, GameObject> _rows = new();

    private SessionController sessionController; // SessionController 参照

    // ClientId→名前（サーバー権威で同期）
    public NetworkList<ClientNameEntry> ClientNames = new NetworkList<ClientNameEntry>();

    void Start()
    {
        if (StartHostButton)   StartHostButton.onClick.AddListener(StartHost);
        if (StartClientButton) StartClientButton.onClick.AddListener(StartClient);

        if (GameStartButton) GameStartButton.onClick.AddListener(() =>
        {
            GameStartButton.gameObject.SetActive(false);
            if (IsServer)
            {
                if (NetworkLoadingScreen.Instance != null)
                    NetworkLoadingScreen.Instance.Show();
                NetworkManager.Singleton.SceneManager.LoadScene("Main", UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
        });

        if (errorOKButtonUI) errorOKButtonUI.onClick.AddListener(() =>
        {
            if (LobbyNetworkErrorMessageCanvasObject) LobbyNetworkErrorMessageCanvasObject.SetActive(false);
        });

        SteamLobby.LobbyConnectionError += OnLobbyConnectionError;
    }

    public override void OnNetworkSpawn()
    {
        ClientNames.OnListChanged += OnClientNamesChanged;

        // 自分の名前をサーバーに提出（ホストもIsClientなので実行される）
        if (IsClient)
        {
            var my = new FixedString64Bytes(SteamFriends.GetPersonaName());
            SubmitMyNameServerRpc(my);
        }

        StartCoroutine(InitAfterSessionController());
    }

    private IEnumerator InitAfterSessionController()
    {
        while (sessionController == null)
        {
            var go = GameObject.FindWithTag("SessionController");
            if (go != null) sessionController = go.GetComponent<SessionController>();
            yield return null;
        }

        sessionController.connectedMembers.OnListChanged += OnMemberListChangedClient;

        // 既存メンバーをUIへ
        foreach (var id in sessionController.connectedMembers)
            SpawnOrUpdateRow(id);

        if (IsServer)
        {
            foreach (var id in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if (!ContainsId(sessionController.connectedMembers, id))
                    sessionController.connectedMembers.Add(id);
            }

            NetworkManager.Singleton.OnClientConnectedCallback  += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
        }

        if (m_joinLobbyID_Field && SteamLobby.Instance != null)
            m_joinLobbyID_Field.text = SteamLobby.Instance.LobbyID.ToString();
    }

    private void OnLobbyConnectionError(string _Message) => ShowLobbyConnectErrorUI(_Message);

    private void ShowLobbyConnectErrorUI(string _message)
    {
        if (errorTextMessageUI) errorTextMessageUI.SetText(_message);
        if (LobbyNetworkErrorMessageCanvasObject) LobbyNetworkErrorMessageCanvasObject.gameObject.SetActive(true);
    }

    private bool ContainsId(NetworkList<ulong> list, ulong id)
    {
        for (int i = 0; i < list.Count; i++) if (list[i] == id) return true;
        return false;
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (sessionController != null && sessionController.connectedMembers != null && !ContainsId(sessionController.connectedMembers, clientId))
            sessionController.connectedMembers.Add(clientId);
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        if (sessionController != null && ContainsId(sessionController.connectedMembers, clientId))
            sessionController.connectedMembers.Remove(clientId);

        if (IsServer)
        {
            int idx = FindNameIndex(clientId);
            if (idx >= 0) ClientNames.RemoveAt(idx);
        }
    }

    private void OnMemberListChangedClient(NetworkListEvent<ulong> e)
    {
        if (LobbyNetworkErrorMessageCanvasObject) LobbyNetworkErrorMessageCanvasObject.gameObject.SetActive(false);
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
                break;
        }
    }

    private void OnClientNamesChanged(NetworkListEvent<ClientNameEntry> e)
    {
        // 追加/更新/挿入のときUI更新
        if (e.Type == NetworkListEvent<ClientNameEntry>.EventType.Add ||
            e.Type == NetworkListEvent<ClientNameEntry>.EventType.Value ||
            e.Type == NetworkListEvent<ClientNameEntry>.EventType.Insert)
        {
            SpawnOrUpdateRow(e.Value.ClientId);
        }
        else if (e.Type == NetworkListEvent<ClientNameEntry>.EventType.Remove ||
                 e.Type == NetworkListEvent<ClientNameEntry>.EventType.RemoveAt)
        {
            RemoveRowSafe(e.Value.ClientId);
        }
    }

    private string GetRoleLabel(ulong clientId)
        => clientId == NetworkManager.ServerClientId ? "ホスト" : "クライアント";

    private int FindNameIndex(ulong clientId)
    {
        for (int i = 0; i < ClientNames.Count; i++)
            if (ClientNames[i].ClientId == clientId) return i;
        return -1;
    }

    private string GetDisplayName(ulong clientId)
    {
        int idx = FindNameIndex(clientId);
        if (idx >= 0)
        {
            var s = ClientNames[idx].Name.ToString();
            if (!string.IsNullOrEmpty(s)) return s;
        }

        // フォールバック（同期前など）
        var alt = SteamLobby.GetPersonaNameByClientId(clientId).ToString();
        return string.IsNullOrEmpty(alt) ? $"Client {clientId}" : alt;
    }

    private void SpawnOrUpdateRow(ulong clientId)
    {
        string personaName = GetDisplayName(clientId);
        string role        = GetRoleLabel(clientId);

        if (_rows.TryGetValue(clientId, out var row) && row != null)
        {
            var box = row.GetComponent<PlayerClientBox>();
            if (box) box.PlayerOnClientConnectedSetText(personaName, role);
            return;
        }

        if (StartHostButton)  StartHostButton.gameObject.SetActive(false);
        if (StartClientButton) StartClientButton.gameObject.SetActive(false);

        if (PlayerClientContents == null || PlayerClientsAll == null)
        {
            Debug.LogError("[Lobby] PlayerClientContents / PlayerClientsAll が未設定");
            return;
        }

        var go  = Instantiate(PlayerClientContents, PlayerClientsAll);
        var pcb = go.GetComponent<PlayerClientBox>();
        if (pcb) pcb.PlayerOnClientConnectedSetText(personaName, role);
        _rows[clientId] = go;
    }

    private void RemoveRow(ulong clientId)
    {
        if (_rows.TryGetValue(clientId, out var row) && row != null)
            Destroy(row);
        _rows.Remove(clientId);
    }
    private void RemoveRowSafe(ulong clientId)
    {
        if (_rows.TryGetValue(clientId, out var row) && row != null)
            Destroy(row);
        _rows.Remove(clientId);
    }

    private void ClearAllRows()
    {
        foreach (var kv in _rows)
            if (kv.Value) Destroy(kv.Value);
        _rows.Clear();
    }

    public override void OnNetworkDespawn()
    {
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
            sessionController.connectedMembers.OnListChanged -= OnMemberListChangedClient;

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback  -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }

        ClientNames.OnListChanged -= OnClientNamesChanged;
        SteamLobby.LobbyConnectionError -= OnLobbyConnectionError;
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

    /* ───────── 名前提出（サーバーでUpsert） ───────── */
    [ServerRpc(RequireOwnership = false)]
    private void SubmitMyNameServerRpc(FixedString64Bytes name, ServerRpcParams rpcParams = default)
    {
        var senderId = rpcParams.Receive.SenderClientId;

        int idx = FindNameIndex(senderId);
        var entry = new ClientNameEntry { ClientId = senderId, Name = name };

        if (idx >= 0)
            ClientNames[idx] = entry;     // 更新
        else
            ClientNames.Add(entry);       // 追加
    }
}
