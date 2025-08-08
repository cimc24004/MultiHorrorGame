using Unity.Netcode;
using UnityEngine;
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
    /*━━━━━━━━ ライフサイクル ━━━━━━━━*/
    protected void Start()
    {
        DontDestroyOnLoad(gameObject);

        /*--- SteamLobby のイベント購読 ---*/
        SteamLobby.ConnectionFailed += OnConnectionFailed;
        SteamLobby.LobbyFull        += OnLobbyFull;
        SteamLobby.PlayerLeft       += OnPlayerLeft;
        SteamLobby.ServerStopped    += OnServerStopped;

        /*--- クライアント側：ホスト落ちを検知 ---*/
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleSelfDisconnect;
    }

    private void OnDestroy()
    {
        SteamLobby.ConnectionFailed -= OnConnectionFailed;
        SteamLobby.LobbyFull        -= OnLobbyFull;
        SteamLobby.PlayerLeft       -= OnPlayerLeft;
        SteamLobby.ServerStopped    -= OnServerStopped;

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

    private void OnServerStopped(bool wasHost)
    {
        Debug.Log("[Session] Host stopped the server");
        if (IsServer) ReturnToTitle();    // ←wasHost は使わなくてもOK
    }

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
        // ReturnToTitle();
    }

    private void ReturnToTitle()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
    }
}
