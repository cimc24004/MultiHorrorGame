using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;

public enum ConnectionFailReason
{
    LobbyCreateFailed,
    LobbyEnterFailed,
    StartHostFailed,
    StartClientFailed,
    LobbyFull,
    ApprovalDenied,
    TransportError,
    Timeout,
    Unknown
}
public class SessionController : NetworkSingleton<SessionController>
{
    private ErrorMessageController errorMessageController;
    [ShowInInspector, SerializeField]public NetworkList<ulong> connectedMembers = new NetworkList<ulong>();
    
    private GameStateController gameStateController;
    /*━━━━━━━━ ライフサイクル ━━━━━━━━*/
    protected void Start()
    {
        DontDestroyOnLoad(gameObject);
        errorMessageController = this.gameObject.GetComponent<ErrorMessageController>();
        /*--- SteamLobby のイベント購読 ---*/
        SteamLobby.ConnectionFailed += OnConnectionFailed;
        SteamLobby.LobbyFull += OnLobbyFull;
        SteamLobby.PlayerLeft += OnPlayerLeft;
        /*--- クライアント側：ホスト落ちを検知 ---*/
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleSelfDisconnect;
    }

    private void OnDestroy()
    {
        SteamLobby.ConnectionFailed -= OnConnectionFailed;
        SteamLobby.LobbyFull        -= OnLobbyFull;
        SteamLobby.PlayerLeft       -= OnPlayerLeft;
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleSelfDisconnect;
    }

    /*━━━━━━━━ SteamLobby から来る ━━━━━━━━*/
    private void OnConnectionFailed(ConnectionFailReason reason, string detail)
    {
        Debug.LogError($"[Session] ConnectionFailed: {reason} / {detail}");
        ShowErrorUI(reason.ToString(), reason);
    }

    private void OnLobbyFull()
    {
        Debug.LogWarning("[Session] LobbyFull");
        ShowErrorUI(ConnectionFailReason.LobbyFull.ToString(), ConnectionFailReason.LobbyFull);
    }

    private void OnPlayerLeft(ulong id)
    {
        Debug.Log($"[Session] PlayerLeft: {id}");
        // LobbyNetworkController で UI を消すとき用にイベント飛ばすとか
    }

    // private void OnServerStopped(bool wasHost)
    // {
    //     Debug.Log("[Session] Host stopped the server");
    //     if (IsServer) ReturnToTitle();    // ←wasHost は使わなくてもOK
    // }

    /*━━━━━━━━ クライアント専用 ━━━━━━━━*/
    private void HandleSelfDisconnect(ulong clientId)
    {
        if (!IsServer && clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.LogWarning("[Session] Lost connection to host");
            ShowErrorUI(ConnectionFailReason.Timeout.ToString(), ConnectionFailReason.Timeout);
        }
    }

    /*━━━━━━━━ UI / シーン遷移 ━━━━━━━━*/
    private void ShowErrorUI(string _errorMessage, ConnectionFailReason _connectionFailReason)
    {
        // TODO: 各理由ごとにメッセージ切り替え
        // ここでポップアップやロビー画面へ遷移
        gameStateController = GameStateController.instance;
        string errorMessage = _errorMessage;
        gameStateController?.ActionByModeSelect(GameStateController.GameState.MenuOnGamePlay, true);
        errorMessageController.ShowErrorUI(errorMessage);
        // ReturnToTitle();
    }
    public void ReturnToLobby()
    {
        
    }
    public void ReturnToTitle()
    {
        // --- ネットワーク終了 ---
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();   // これ一本でホスト／クライアント両対応
        }
        // 2) Steam ロビーから抜ける
        if (SteamLobby.Instance != null && SteamLobby.Instance.LobbyID != 0)
        {
            SteamLobby.Instance.LeaveCurrentLobby();   // ←後述するメソッド
        }
        SceneManager.LoadScene("Lobby");
    }
}
