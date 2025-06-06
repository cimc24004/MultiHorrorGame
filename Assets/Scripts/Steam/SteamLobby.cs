using Netcode.Transports;
using Steamworks;
using Unity.Netcode;
using UnityEngine;

public class SteamLobby : MonoBehaviour
{
    private CallResult<LobbyCreated_t> m_crLobbyCreated;
    private Callback<LobbyEnter_t> m_lobbyEnter;
    private const string s_HostAddressKey = "HostAddress";
    public ulong LobbyID { get; private set; }

    void Start()
    {
        Debug.Log($"[Host] SteamManager.Initialized = {SteamManager.Initialized}");

        if (SteamManager.Initialized)
        {
            m_crLobbyCreated = CallResult<LobbyCreated_t>.Create(OnCreateLobby);
            m_lobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
            Debug.Log("SteamLobby: Start() OK");
            Debug.Log($"[Host] 自分の SteamID = {SteamUser.GetSteamID()}");
        }
        else
        {
            Debug.LogError("SteamLobby: SteamManager が初期化されていない！");
        }
    }

    // ホストを立てたい時に呼ぶ（UIボタンなどから）
    public void CreateLobby()
    {
        SteamAPICall_t call = SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 4);
        m_crLobbyCreated.Set(call);
        Debug.Log("SteamLobby: CreateLobby リクエスト投げた");
    }

    private void OnCreateLobby(LobbyCreated_t callback, bool ioFailure)
    {
        if (ioFailure || callback.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogError($"SteamLobby: ロビー作成エラー {callback.m_eResult}");
            return;
        }

        LobbyID = callback.m_ulSteamIDLobby;
        Debug.Log($"SteamLobby: ロビー作成成功 LobbyID={LobbyID}");

        // ロビーデータにホストの SteamID を載せる
        SteamMatchmaking.SetLobbyData(
            new CSteamID(LobbyID),
            s_HostAddressKey,
            SteamUser.GetSteamID().ToString());

        // **ここでホスト側の接続待ち callback を登録する！**
        NetworkManager.Singleton.OnClientConnectedCallback += GameNetworkController.Instance.OnClientConnected;
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;

        // ホスト起動
        bool hostOK = NetworkManager.Singleton.StartHost();
        Debug.Log($"SteamLobby: StartHost 実行 → 成功? {hostOK}");
    }

    // クライアントとしてロビーに入る時に呼ぶ（UIボタンなど）
    public void JoinLobby(ulong targetLobbyId)
    {
        SteamMatchmaking.JoinLobby(new CSteamID(targetLobbyId));
        Debug.Log($"SteamLobby: JoinLobby リクエスト投げた -> {targetLobbyId}");
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if ((EChatRoomEnterResponse)callback.m_EChatRoomEnterResponse 
            != EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
        {
            Debug.LogError("SteamLobby: ロビー入室失敗");
            return;
        }

        LobbyID = callback.m_ulSteamIDLobby;
        Debug.Log("SteamLobby: ロビー入室成功 LobbyID=");
        Debug.Log($"{LobbyID}");
        string hostAddress = SteamMatchmaking.GetLobbyData(
            new CSteamID(LobbyID),
            s_HostAddressKey);

        // 自分がホストの場合は何もしない
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

        // 切断コールバック（オプション）
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest req, NetworkManager.ConnectionApprovalResponse res)
    {
        Debug.Log("ApprovalCheck が呼ばれた"); 
        res.Pending = true;
        if (NetworkManager.Singleton.ConnectedClients.Count >= 4)
        {
            res.Approved = false;
            res.Pending = false;
            Debug.Log("SteamLobby: 承認拒否 (クライアント数上限)");
            return;
        }

        res.Approved = true;
        res.CreatePlayerObject = false;
        res.PlayerPrefabHash = null;
        res.Position = Vector3.zero;
        res.Rotation = Quaternion.identity;
        res.Pending = false;
        Debug.Log("SteamLobby: 承認OK");
    }

    private void OnClientDisconnect(ulong clientId)
    {
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        Debug.Log($"SteamLobby: クライアント {clientId} 切断 → Shutdown");
        NetworkManager.Singleton.Shutdown();
    }

    // シングルトン
    private static SteamLobby instance;
    public static SteamLobby Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<SteamLobby>();
            return instance;
        }
    }
}
