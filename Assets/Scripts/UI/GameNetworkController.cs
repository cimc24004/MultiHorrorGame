using UnityEngine;
using Unity.Netcode;
using Steamworks;
using TMPro;
using DG.Tweening;

public class GameNetworkController : Singleton<GameNetworkController>
{
    [SerializeField] private Transform PlayerCreatePos;
    [SerializeField] private NetworkObject m_playerPrefab;
    // もし追加でカメラプレハブや CMFpsController が必要ならコメント解除して使って
    // [SerializeField] private NetworkObject CamerasPrefab;
    // [SerializeField] private NetworkObject CMFpsControllerPrefab;

    [SerializeField] private TMP_InputField m_joinLobbyID;
    private string SteamLobbyId; // UI 入力用文字列格納

    private void Start()
    {
        Debug.Log(NetworkManager.Singleton.NetworkConfig.NetworkTransport != null 
            ? "Transport OK" 
            : "Transport NULL!!");
    }

    /// <summary>
    /// ボタンなどから呼び出されてホストを立てる
    /// </summary>
    public void StartHost()
    {
        // まずはロビー作成を呼ぶだけ。実際のNetworkManager.StartHostは OnCreateLobby 内で行われる
        SteamLobby.Instance.CreateLobby();

        // ロビーが作成されてOnCreateLobbyが呼ばれたあとでホストが立つので、
        // ここでは特に NetworkManager.StartHost() は呼ばない。

        // ロビーID が返ってくるまで少し時間がかかるから、UIに「コピー用の文字列」を
        // ここではまだコピーしない。OnCreateLobby 内でコピーするようにする。
    }

    /// <summary>
    /// ボタンなどから呼び出されてクライアントとして参加する
    /// </summary>
    public void StartClient()
    {
        // UI から入力された文字列を読む
        SteamLobbyId = m_joinLobbyID.text.Trim();
        if (string.IsNullOrEmpty(SteamLobbyId))
        {
            Debug.LogError("JoinLobbyID が空文字 or null だよ");
            return;
        }

        // 数字にパースできるかチェック
        if (!ulong.TryParse(SteamLobbyId, out ulong targetLobbyId))
        {
            Debug.LogError($"JoinLobbyID を ulong にパースできなかった: {SteamLobbyId}");
            return;
        }

        // ここで JoinLobby を呼ぶだけ。OnLobbyEntered が呼ばれたタイミングで StartClient が走る
        SteamLobby.Instance.JoinLobby(targetLobbyId);
        Debug.Log($"JoinLobby 呼び出し: {targetLobbyId}");
    }

    /// <summary>
    /// クライアントが接続されたときに呼ばれるコールバック（ホスト側で呼ばれる）
    /// </summary>
    /// <param name="clientId"></param>
    public void OnClientConnected(ulong clientId)
    {
        // PlayerCreatePos が設定されていないとエラーになるから要注意
        Vector3 spawnPos = PlayerCreatePos != null ? PlayerCreatePos.position : Vector3.zero;
        NetworkObject playerObj = Instantiate(m_playerPrefab, spawnPos, Quaternion.identity);
        playerObj.SpawnAsPlayerObject(clientId);

        Debug.Log($"Client {clientId} のプレイヤー生成完了");

        // 切断時のログだけ拾いたいならこれでもOK
        NetworkManager.Singleton.OnClientDisconnectCallback += (uid) =>
        {
            Debug.Log($"Client {uid} が切断されたよ");
        };
    }

    // シングルトン実装
    private static GameNetworkController instance;
    public static GameNetworkController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameNetworkController>();
            }
            return instance;
        }
    }
}
