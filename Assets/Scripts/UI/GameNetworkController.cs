using UnityEngine;
using Unity.Netcode;
using Steamworks;
using TMPro;

public class GameNetworkController : Singleton<GameNetworkController>
{
    [SerializeField] private Transform PlayerCreatePos;     // プレイヤースポーン位置
    [SerializeField] private NetworkObject m_playerPrefab; // プレイヤープレハブ

    [SerializeField] private TMP_InputField m_joinLobbyID; // LobbyID 入力用
    private string SteamLobbyId;

    void Start()
    {
        Debug.Log(NetworkManager.Singleton.NetworkConfig.NetworkTransport != null 
            ? "Transport OK" 
            : "Transport NULL!!");
    }

    // ボタンから呼ぶ：ロビー作成（ホスト立て）
    public void StartHost()
    {
        SteamLobby.Instance.CreateLobby();
        Debug.Log("GameNetworkController: StartHost() → CreateLobby() 呼んだ");
    }

    // ボタンから呼ぶ：LobbyID 入力して参加
    public void StartClient()
    {
        SteamLobbyId = m_joinLobbyID.text.Trim();
        if (string.IsNullOrEmpty(SteamLobbyId))
        {
            Debug.LogError("GameNetworkController: JoinLobbyID が空");
            return;
        }

        if (!ulong.TryParse(SteamLobbyId, out ulong targetLobbyId))
        {
            Debug.LogError($"GameNetworkController: JoinLobbyID のパース失敗 → {SteamLobbyId}");
            return;
        }

        SteamLobby.Instance.JoinLobby(targetLobbyId);
        Debug.Log($"GameNetworkController: StartClient() → JoinLobby({targetLobbyId}) 呼んだ");
    }

    // ホスト側でクライアントが接続されたら呼ばれる
    public void OnClientConnected(ulong clientId)
    {
        Vector3 spawnPos = PlayerCreatePos != null ? PlayerCreatePos.position : Vector3.zero;
        NetworkObject playerObj = Instantiate(m_playerPrefab, spawnPos, Quaternion.identity);
        playerObj.SpawnAsPlayerObject(clientId);
        Debug.Log($"GameNetworkController: クライアント {clientId} のプレイヤーを生成した");

        // 切断時のログだけ拾いたいならこのままでもOK
        NetworkManager.Singleton.OnClientDisconnectCallback += (uid) =>
        {
            Debug.Log($"GameNetworkController: クライアント {uid} が切断");
        };
    }

    // シングルトン実装
    private static GameNetworkController instance;
    public static GameNetworkController Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<GameNetworkController>();
            return instance;
        }
    }
}
