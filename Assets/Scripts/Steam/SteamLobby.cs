using Netcode.Transports;
using Steamworks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SteamLobby : MonoBehaviour
{
    // ロビー作成＆入室のコールバック保持用
    private CallResult<LobbyCreated_t> m_crLobbyCreated;
    private Callback<LobbyEnter_t> m_lobbyEnter;

    // ロビーデータに載せるキー
    private const string s_HostAddressKey = "HostAddress";

    public ulong LobbyID { get; private set; }

    private void Start()
    {
        // SteamAPI.RunCallbacks() は SteamManager で回っている想定
        if (SteamManager.Initialized)
        {
            m_crLobbyCreated = CallResult<LobbyCreated_t>.Create(OnCreateLobby);
            m_lobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        }
    }

    /// <summary>
    /// ロビー作成（ホストを立てる）
    /// </summary>
    public void CreateLobby()
    {
        // ロビー作成リクエストを投げる
        SteamAPICall_t hCreateLobby = SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 4);
        m_crLobbyCreated.Set(hCreateLobby);
    }

    // ロビー作成完了コールバック
    private void OnCreateLobby(LobbyCreated_t pCallback, bool bIOFailure)
    {
        if (bIOFailure || pCallback.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogError($"ロビー作成失敗: {pCallback.m_eResult}");
            return;
        }

        // ロビーIDを保存
        LobbyID = pCallback.m_ulSteamIDLobby;

        // ホストの SteamID (文字列) をロビーデータに載せる
        SteamMatchmaking.SetLobbyData(
            new CSteamID(LobbyID),
            s_HostAddressKey,
            SteamUser.GetSteamID().ToString());

        Debug.Log($"ロビー作成成功: {LobbyID}"); // ログ確認用

        // このタイミングで NetworkManager のホストを立てる
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        bool hostResult = NetworkManager.Singleton.StartHost();
        Debug.Log($"[Netcode] StartHost → 成功: {hostResult}");

        // （必要ならゲームシーンに遷移する処理を書く。コメントアウトしてある）
        // NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    /// <summary>
    /// 指定したロビーIDに入室する（クライアント）
    /// </summary>
    public void JoinLobby(ulong targetLobbyId)
    {
        SteamMatchmaking.JoinLobby(new CSteamID(targetLobbyId));
    }

    // ロビー入室完了コールバック
    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        // 入室失敗チェック
        if ((EChatRoomEnterResponse)callback.m_EChatRoomEnterResponse != EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
        {
            Debug.LogError("ロビー入室失敗");
            return;
        }

        // 入室したロビーの LobbyID を保存
        LobbyID = callback.m_ulSteamIDLobby;
        Debug.Log($"ロビー入室成功: {LobbyID}");

        // ロビー内に登録されたホストの SteamID（文字列）を取得
        string hostAddress = SteamMatchmaking.GetLobbyData(
            new CSteamID(LobbyID),
            s_HostAddressKey);

        // 自分自身がホストのときは何もしない
        if (hostAddress == SteamUser.GetSteamID().ToString())
        {
            Debug.Log("自分自身がホストなのでクライアント接続はスキップ");
            return;
        }

        // 取得したホストの SteamID (ulong) を Transport にセット
        var stp = (SteamNetworkingSocketsTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        stp.ConnectToSteamID = ulong.Parse(hostAddress);
        Debug.Log($"ConnectToSteamID に {hostAddress} をセット");

        // このタイミングでクライアントをスタート
        bool clientResult = NetworkManager.Singleton.StartClient();
        Debug.Log($"[Netcode] StartClient → 成功: {clientResult}");

        // 切断時のコールバック登録
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }

    // 接続承認時の処理（ホスト側で呼ばれる）
    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        response.Pending = true;

        // 最大人数チェック（ホスト自身も ConnectedClients にカウントされる）
        if (NetworkManager.Singleton.ConnectedClients.Count >= 2)
        {
            response.Approved = false;
            response.Pending = false;
            return;
        }

        response.Approved = true;
        response.CreatePlayerObject = false;
        response.PlayerPrefabHash = null;
        response.Position = Vector3.zero;
        response.Rotation = Quaternion.identity;
        response.Pending = false;
    }

    // クライアントが切断したときの処理（ホスト側で呼ばれる）
    private void OnClientDisconnect(ulong clientId)
    {
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        // 切断されたらネットワーク落としてメインシーンに戻す
        NetworkManager.Singleton.Shutdown();
        // SceneManager.LoadScene("Title"); // 必要なら有効化
        Debug.Log($"クライアント {clientId} が切断 → NetworkManager.Shutdown()");
    }

    // シングルトン実装
    private static SteamLobby instance;
    public static SteamLobby Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SteamLobby>();
            }
            return instance;
        }
    }
}
