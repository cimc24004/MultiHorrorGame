using UnityEngine;
using Unity.Netcode;
using Steamworks;
using TMPro;

public class GameNetworkController : Singleton<GameNetworkController>
{
    [SerializeField] private Transform      PlayerCreatePos;  // プレイヤー湧き位置
    [SerializeField] private NetworkObject  m_playerPrefab;   // プレハブ
    [SerializeField] private TMP_InputField m_joinLobbyID;    // LobbyID入力

    void Start()
    {
        Debug.Log(NetworkManager.Singleton.NetworkConfig.NetworkTransport != null
            ? "Transport OK"
            : "Transport NULL!!");
    }

    // ──────────────────────────────────────
    // ボタン：ロビー作成（ホスト）
    // ──────────────────────────────────────
    public void StartHost()
    {
        SteamLobby.Instance.CreateLobby();
        Debug.Log("GameNetworkController: StartHost() → CreateLobby() 呼んだ");
    }

    // ──────────────────────────────────────
    // ボタン：LobbyID を入力して参加（クライアント）
    // ──────────────────────────────────────
    public void StartClient()
    {
        string id = m_joinLobbyID.text.Trim();
        if (!ulong.TryParse(id, out ulong lobbyId))
        {
            Debug.LogError($"GameNetworkController: JoinLobbyID パース失敗 → {id}");
            return;
        }

        SteamLobby.Instance.JoinLobby(lobbyId);
        Debug.Log($"GameNetworkController: StartClient() → JoinLobby({lobbyId}) 呼んだ");
    }

    // ──────────────────────────────────────
    // ホスト側：クライアント承認後に呼ばれる
    // ──────────────────────────────────────
    public void OnClientConnected(ulong clientId)
    {
        Debug.Log($"OnClientConnected: {clientId}");

        Vector3 pos = PlayerCreatePos ? PlayerCreatePos.position : Vector3.zero;
        NetworkObject pObj = Instantiate(m_playerPrefab, pos, Quaternion.identity);
        pObj.SpawnAsPlayerObject(clientId);

        Debug.Log($"GameNetworkController: クライアント {clientId} のプレイヤー生成 ObjectID={pObj.NetworkObjectId}");
    }
}
