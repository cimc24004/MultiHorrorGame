using UnityEngine;
using Unity.Netcode;
public class GameNetworkController : Singleton<GameNetworkController>
{
    [SerializeField] private Transform PlayerCreatePos;
    [SerializeField] private NetworkObject m_playerPrefab;
    [SerializeField] private NetworkObject CamerasPrefab;
    [SerializeField] private NetworkObject CMFpsControllerPrefab;
    //とりあえずテストようのStartHost StartClient
    void Start()
    {
        Debug.Log(NetworkManager.Singleton.NetworkConfig.NetworkTransport != null 
    ? "Transport OK" 
    : "Transport NULL!!");
    }
    public void StartHost(){
        // NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;

        NetworkManager.Singleton.StartHost();   // ホスト起動

        //クライアント接続時
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        //すでに存在するクライアント用に関数呼び出す
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            OnClientConnected(client.ClientId);
        }
    }
    public void StartClient(){
        NetworkManager.Singleton.StartClient();   // ホスト起動
    }
    // public override void OnNetworkSpawn()
    // {
    //             //ホスト以外の場合
    //     if (IsServer == false){ return; }


    // }
    public void OnClientConnected(ulong clientId){
        var generatePos = PlayerCreatePos.position;
        // generatePos.x = -5 + 5 * (NetworkManager.Singleton.ConnectedClients.Count % 3);
        NetworkObject playerObject = Instantiate(m_playerPrefab, generatePos, Quaternion.identity);
        //接続クライアントをOwnerにしてPlayerObjectとしてスポーン
        playerObject.SpawnAsPlayerObject(clientId);
        NetworkManager.Singleton.OnClientDisconnectCallback += (clientId) => {
            Debug.Log($"Client {clientId} disconnected. IsHost? {NetworkManager.Singleton.IsHost}");
        };
        // playerObject.transform.position = generatePos;
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        response.Pending = true;
        if (NetworkManager.Singleton.ConnectedClients.Count >= 4)
        {
            response.Approved = false;//接続を許可しない
            response.Pending = false;
            return;
        }
        //ここからは接続成功クライアントに向けた処理
        response.Approved = true;//接続を許可

        //PlayerObjectを生成するかどうか
        response.CreatePlayerObject = false;

        //生成するPrefabハッシュ値。nullの場合NetworkManagerに登録したプレハブが使用される
        response.PlayerPrefabHash = null;

        //PlayerObjectをスポーンする位置(nullの場合Vector3.zero)
        // var position = PlayerCreatePos.position;
        // position.x = -0.1f + 0.1f * (NetworkManager.Singleton.ConnectedClients.Count % 3);
        // response.Position = PlayerCreatePos.position;

        // //PlayerObjectをスポーン時の回転 (nullの場合Quaternion.identity)
        response.Rotation = Quaternion.identity;

        response.Pending = false;

    }
}
