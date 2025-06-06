using Netcode.Transports;
using Steamworks;
using Unity.Netcode;
using UnityEngine;

public class SteamLobby : MonoBehaviour
{
    private CallResult<LobbyCreated_t> m_crLobbyCreated;
    private Callback<LobbyEnter_t>     m_lobbyEnter;

    private const string s_HostAddressKey = "HostAddress";
    public  ulong  LobbyID { get; private set; }

    // ★ 二重 StartClient 防止フラグ
    private bool m_clientStarted = false;

    void Start()
    {
        Debug.Log($"[Host] SteamManager.Initialized = {SteamManager.Initialized}");

        if (!SteamManager.Initialized)
        {
            Debug.LogError("SteamLobby: SteamManager が初期化されていない！");
            return;
        }

        // Steam コールバックはここで一度だけ登録
        m_crLobbyCreated = CallResult<LobbyCreated_t>.Create(OnCreateLobby);
        m_lobbyEnter     = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        Debug.Log("SteamLobby: Start() OK");
        Debug.Log($"[Host] 自分の SteamID = {SteamUser.GetSteamID()}");

        // ★ Netcode コールバックもここで一度だけ登録
        NetworkManager.Singleton.OnClientConnectedCallback  += GameNetworkController.instance.OnClientConnected;
        NetworkManager.Singleton.ConnectionApprovalCallback   = ApprovalCheck;
        NetworkManager.Singleton.OnClientDisconnectCallback  += OnClientDisconnect;
    }

    // ──────────────────────────────────────
    // ホストを立てたい時に呼ぶ（UIボタンなどから）
    // ──────────────────────────────────────
    public void CreateLobby()
    {
        var call = SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 4);
        m_crLobbyCreated.Set(call);
        Debug.Log("SteamLobby: CreateLobby リクエスト投げた");
    }

    private void OnCreateLobby(LobbyCreated_t cb, bool ioFailure)
    {
        if (ioFailure || cb.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogError($"SteamLobby: ロビー作成エラー {cb.m_eResult}");
            return;
        }

        LobbyID = cb.m_ulSteamIDLobby;
        Debug.Log($"SteamLobby: ロビー作成成功 LobbyID={LobbyID}");

        // ロビーデータにホストの SteamID を載せる
        SteamMatchmaking.SetLobbyData(
            new CSteamID(LobbyID),
            s_HostAddressKey,
            SteamUser.GetSteamID().ToString());

        // ホスト起動
        bool hostOK = NetworkManager.Singleton.StartHost();
        Debug.Log($"SteamLobby: StartHost 実行 → 成功? {hostOK}");
    }

    // ──────────────────────────────────────
    // クライアントとしてロビーに入る時に呼ぶ（UIボタンなど）
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
            return;
        }

        // ★ StartClient 二重実行ガード
        if (m_clientStarted) return;
        m_clientStarted = true;

        LobbyID = cb.m_ulSteamIDLobby;
        Debug.Log($"SteamLobby: ロビー入室成功 LobbyID={LobbyID}");

        string hostAddress = SteamMatchmaking.GetLobbyData(
            new CSteamID(LobbyID), s_HostAddressKey);

        // 自分がホストなら何もしない
        if (hostAddress == SteamUser.GetSteamID().ToString())
        {
            Debug.Log("SteamLobby: 自分がホストなので StartClient をスキップ");
            return;
        }

        Debug.Log($"SteamLobby: ホストの SteamID = {hostAddress}");

        // Transport に接続先 SteamID をセット
        var stp = (SteamNetworkingSocketsTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        stp.ConnectToSteamID = ulong.Parse(hostAddress);
        Debug.Log($"SteamLobby: ConnectToSteamID に {hostAddress} セット");

        // クライアント起動
        bool clientOK = NetworkManager.Singleton.StartClient();
        Debug.Log($"SteamLobby: StartClient 実行 → 成功? {clientOK}");
    }

    // ──────────────────────────────────────
    // Connection Approval
    // ──────────────────────────────────────
    private void ApprovalCheck(
        NetworkManager.ConnectionApprovalRequest  req,
        NetworkManager.ConnectionApprovalResponse res)
    {
        Debug.Log("ApprovalCheck 呼ばれた");

        // 定員チェック
        if (NetworkManager.Singleton.ConnectedClients.Count >= 4)
        {
            res.Approved = false;
            Debug.Log("SteamLobby: 承認拒否 (クライアント数上限)");
            return;
        }

        res.Approved          = true;
        res.CreatePlayerObject = false;   // プレイヤー生成はホスト側で手動
        // Pending を触らない（同期承認）
    }

    // ──────────────────────────────────────
    // 切断通知（ここではログだけ）
    // ──────────────────────────────────────
    private void OnClientDisconnect(ulong clientId)
    {
        Debug.Log($"SteamLobby: クライアント {clientId} が切断");
        // 必要ならここでロビーからキックしたり部屋を閉じたりしてもOK
    }

    // シングルトン
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
