using System;
using Netcode.Transports;
using Steamworks;
using Unity.Netcode;
using UnityEngine;

public class SteamLobby : MonoBehaviour
{
    private CallResult<LobbyCreated_t> m_crLobbyCreated;
    private Callback<LobbyEnter_t>     m_lobbyEnter;

    // ── 既存イベント（そのまま） ──
    public static event Action LobbyFull;
    public static event Action<string> LobbyConnectionError;
    public static event Action<string> ApprovalDenied;
    public static event Action<ConnectionFailReason,string> ConnectionFailed;
    public static event Action<ulong> PlayerLeft; 
    public static event Action<bool>  ServerStopped;
    private const string s_HostAddressKey = "HostAddress";
    public  ulong  LobbyID { get; private set; }

    // ★ 二重 StartClient 防止フラグ
    private bool m_clientStarted = false;

    // ──────────────────────────────────────
    // Start
    // ──────────────────────────────────────
    void Start()
    {
        Debug.Log($"[Host] SteamManager.Initialized = {SteamManager.Initialized}");
        if (!SteamManager.Initialized)
        {
            Debug.LogError("SteamLobby: SteamManager が初期化されていない！");
            LobbyConnectionError?.Invoke("SteamManager is not initialized");
            return;
        }

        /* Steam コールバック登録 */
        m_crLobbyCreated = CallResult<LobbyCreated_t>.Create(OnCreateLobby);
        m_lobbyEnter     = Callback<LobbyEnter_t>.Create(OnLobbyEntered);

        /* Netcode コールバック登録（ここだけ追加） */
        NetworkManager.Singleton.ConnectionApprovalCallback   = ApprovalCheck;
        NetworkManager.Singleton.OnClientDisconnectCallback  += OnClientDisconnect;
        NetworkManager.Singleton.OnTransportFailure          += OnTransportFailure; // ★追加
        NetworkManager.Singleton.OnServerStopped             += OnServerStopped;    // ★追加

        Debug.Log("SteamLobby: Start() OK");
        Debug.Log($"[Host] 自分の SteamID = {SteamUser.GetSteamID()}");
    }

    // ──────────────────────────────────────
    // ホストを立てる
    // ──────────────────────────────────────
    public void CreateLobby()
    {
        m_crLobbyCreated.Set(
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 4));
        Debug.Log("SteamLobby: CreateLobby リクエスト投げた");
    }

    private void OnCreateLobby(LobbyCreated_t cb, bool ioFailure)
    {
        if (ioFailure || cb.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogError($"SteamLobby: ロビー作成エラー {cb.m_eResult}");
            LobbyConnectionError?.Invoke("An error occurred while creating the lobby");
            return;
        }

        LobbyID = cb.m_ulSteamIDLobby;
        Debug.Log($"SteamLobby: ロビー作成成功 LobbyID={LobbyID}");

        // ホストの SteamID をロビーデータに保存
        SteamMatchmaking.SetLobbyData(
            new CSteamID(LobbyID),
            s_HostAddressKey,
            SteamUser.GetSteamID().ToString());

        // ホスト起動
        bool hostOK = NetworkManager.Singleton.StartHost();
        Debug.Log($"SteamLobby: StartHost 実行 → 成功? {hostOK}");
        if (!hostOK) ConnectionFailed?.Invoke(ConnectionFailReason.StartHostFailed,"StartHost() false");
    }
    public void LeaveCurrentLobby()
    {
        if (LobbyID == 0) return;

        SteamMatchmaking.LeaveLobby(new CSteamID(LobbyID));
        Debug.Log($"SteamLobby: LeaveLobby({LobbyID})");
        LobbyID = 0;
    }
    // ──────────────────────────────────────
    // クライアントとして参加
    // ──────────────────────────────────────
    public void JoinLobby(ulong targetLobbyId)
    {
        SteamMatchmaking.JoinLobby(new CSteamID(targetLobbyId));
        Debug.Log($"SteamLobby: JoinLobby リクエスト投げた -> {targetLobbyId}");
    }

    private void OnLobbyEntered(LobbyEnter_t cb)
    {
        if ((EChatRoomEnterResponse)cb.m_EChatRoomEnterResponse
            != EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
        {
            Debug.LogError("SteamLobby: ロビー入室失敗");
            LobbyConnectionError?.Invoke("Unable to join the lobby.\n Please verify the lobby ID.");
            return;
        }

        if (m_clientStarted) return;   // 二重防止
        m_clientStarted = true;

        LobbyID = cb.m_ulSteamIDLobby;
        Debug.Log($"SteamLobby: ロビー入室成功 LobbyID={LobbyID}");

        string hostAddress = SteamMatchmaking.GetLobbyData(
            new CSteamID(LobbyID), s_HostAddressKey);

        // 自分がホスト → 何もしない
        if (hostAddress == SteamUser.GetSteamID().ToString())
        {
            Debug.Log("SteamLobby: 自分がホストなので StartClient スキップ");
            return;
        }

        // Transport に接続先 SteamID をセット
        var stp = (SteamNetworkingSocketsTransport)
                  NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        stp.ConnectToSteamID = ulong.Parse(hostAddress);
        Debug.Log($"SteamLobby: ConnectToSteamID = {hostAddress}");

        // クライアント起動
        if (!NetworkManager.Singleton.StartClient())
        {
            ConnectionFailed?.Invoke(ConnectionFailReason.StartClientFailed,"StartClient() false");
            return;
        }

        Debug.Log("SteamLobby: StartClient 成功");
    }

    // ──────────────────────────────────────
    // Approval
    // ──────────────────────────────────────
    private void ApprovalCheck(
        NetworkManager.ConnectionApprovalRequest  req,
        NetworkManager.ConnectionApprovalResponse res)
    {
        Debug.Log("ApprovalCheck 呼ばれた");

        // 定員チェック
        if (NetworkManager.Singleton.ConnectedClients.Count >= 4)
        {
            LobbyFull?.Invoke();
            res.Approved = false;
            return;
        }

        /* 例：ハンドシェイクチェック (コメントアウト部分を書き換えて使う)
        var handshake = System.Text.Encoding.UTF8.GetString(req.Payload);
        if (handshake != "OK")
        {
            res.Approved = false;
            ApprovalDenied?.Invoke("Handshake mismatch");
            return;
        }
        */

        res.Approved           = true;
        res.CreatePlayerObject = false;   // プレイヤー生成は手動
    }

    // ──────────────────────────────────────
    // エラー／切断
    // ──────────────────────────────────────
    private void OnTransportFailure()
    {
        Debug.LogError("SteamLobby: TransportFailure 検知");
        ConnectionFailed?.Invoke(ConnectionFailReason.TransportError,"TransportFailure");
    }

    private void OnServerStopped(bool wasHost)
    {
        ServerStopped?.Invoke(wasHost);
        Debug.Log("SteamLobby: ホスト（自分）がサーバー停止");
        // ConnectionFailed?.Invoke(ConnectionFailReason.Unknown,"ServerStopped");
    }

    private void OnClientDisconnect(ulong clientId)
    {
         PlayerLeft?.Invoke(clientId);           // ←誰が抜けても通知
        Debug.Log($"SteamLobby: クライアント {clientId} が切断");

        // クライアント側で「自分が切られた＝ホスト消失」
        if (!NetworkManager.Singleton.IsServer &&
            clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.LogWarning("SteamLobby: ホスト切断を検知");
            PlayerLeft?.Invoke(clientId);
        }
    }

    // ──────────────────────────────────────
    // シングルトン
    // ──────────────────────────────────────
    private static SteamLobby instance;
    public static  SteamLobby Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<SteamLobby>();
            return instance;
        }
    }
}
