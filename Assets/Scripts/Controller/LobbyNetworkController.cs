using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;
using TMPro;
public class LobbyNetworkController : NetworkBehaviour
{
    [SerializeField] Button StartHostButton;
    [SerializeField] Button StartClientButton;
    [SerializeField] Button GameStartButton;
    [SerializeField] GameObject PlayerClientContents;
    [SerializeField] GameObject PlayerClientsAll;
    [SerializeField] Button MapStartButton;
    private NetworkList<ulong> connectedMembers = new NetworkList<ulong>();
    [SerializeField] private TMP_InputField m_joinLobbyID_Field;    // LobbyID入力

    void Start()
    {
        Debug.Log("Start");
        StartHostButton.onClick.AddListener(() =>
        {
            this.StartHost();
        });

        StartClientButton.onClick.AddListener(() =>
        {
            this.StartClient();
        });

        GameStartButton.onClick.AddListener(() =>
        {
            // シーン遷移はサーバーがまとめて行う
            if (IsServer)
                NetworkManager.Singleton.SceneManager.LoadScene("Main", LoadSceneMode.Single);
        });


    }
    
    // ──────────────────────────────────────
    // ボタン：ロビー作成（ホスト）
    // ──────────────────────────────────────
    public void StartHost()
    {
        // byte[] handshake = { 0x01 };
        // NetworkManager.Singleton.NetworkConfig.ConnectionData = handshake;
        SteamLobby.Instance.CreateLobby();
        ulong hash = NetworkManager.Singleton.NetworkConfig.GetConfig(false);
        Debug.Log($"ConfigHash={hash:X8}");

        Debug.Log("GameNetworkController: StartHost() → CreateLobby() 呼んだ");
    }

    // ──────────────────────────────────────
    // ボタン：LobbyID を入力して参加（クライアント）
    // ──────────────────────────────────────
    public void StartClient()
    {
        string id = m_joinLobbyID_Field.text.Trim();
        if (!ulong.TryParse(id, out ulong lobbyId))
        {
            Debug.LogError($"GameNetworkController: JoinLobbyID パース失敗 → {id}");
            return;
        }
        // byte[] handshake = { 0x01 };
        // NetworkManager.Singleton.NetworkConfig.ConnectionData = handshake;
        ulong hash = NetworkManager.Singleton.NetworkConfig.GetConfig(false);
        Debug.Log($"ConfigHash={hash:X8}");

        SteamLobby.Instance.JoinLobby(lobbyId);
        Debug.Log($"GameNetworkController: StartClient() → JoinLobby({lobbyId}) 呼んだ");
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("OnNetworkSpawn");
        foreach (ulong clientid in connectedMembers)
        {
            Debug.Log("foreachないです");
            SpawnLocalUI(clientid);
        }
        this.m_joinLobbyID_Field.text = SteamLobby.Instance.LobbyID.ToString();
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            connectedMembers.OnListChanged += OnMemberListChanged;
        }
        // ホスト自身の UI もここで生成
        // SpawnLocalUI(NetworkManager.Singleton.LocalClientId);
    }

    private void HandleClientConnected(ulong clientId)
    {
        connectedMembers.Add(clientId);
    }
    private void OnMemberListChanged(NetworkListEvent<ulong> e){
        Debug.Log("OnMemberListChanged: " + e.Value);
        if (e.Type == NetworkListEvent<ulong>.EventType.Add)
            SpawnLocalUIClientRpc(e.Value);
        else if (e.Type == NetworkListEvent<ulong>.EventType.Remove)
            Debug.Log("");
            // RemoveUi(e.Value);
    }
    [ClientRpc]
    private void SpawnLocalUIClientRpc(ulong clientId)
    {
        Debug.Log("ここはClientRPCないです");
        SpawnLocalUI(clientId);
    }
    private void SpawnLocalUI(ulong clientId){
        Debug.Log("SpawnLocalUIが呼ばれています");
        var go = Instantiate(PlayerClientContents, PlayerClientsAll.transform);
        go.GetComponent<PlayerClientBox>()
           .PlayerOnClientConnectedSetText($"プレイヤーID: {clientId}");
    }
}
