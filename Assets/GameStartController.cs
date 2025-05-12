using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;

public class GameStartController : NetworkBehaviour
{
    [SerializeField] Button StartHostButton;
    [SerializeField] Button StartClientButton;
    [SerializeField] Button GameStartButton;
    [SerializeField] GameObject PlayerClientContents;
    [SerializeField] GameObject PlayerClientsAll;
    private NetworkList<ulong> connectedMembers = new NetworkList<ulong>();
    void Start()
    {
        Debug.Log("Start");
        StartHostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();   // ホスト起動
        });

        StartClientButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient(); // 純クライアント起動
        });

        GameStartButton.onClick.AddListener(() =>
        {
            // シーン遷移はサーバーがまとめて行う
            if (IsServer)
                NetworkManager.Singleton.SceneManager.LoadScene("Main", LoadSceneMode.Single);
        });


    }
    public override void OnNetworkSpawn()
    {
            Debug.Log("OnNetworkSpawn");
        foreach(ulong clientid in connectedMembers){
            Debug.Log("foreachないです");
            SpawnLocalUI(clientid);
        }

        if (IsServer){
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
