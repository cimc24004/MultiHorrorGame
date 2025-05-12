using UnityEngine;
using Unity.Netcode;
public class GameNetworkController : NetworkSingleton<GameNetworkController>
{
    //とりあえずテストようのStartHost StartClient
    public void StartHost(){
        NetworkManager.Singleton.StartHost();   // ホスト起動
    }
    public void StartClient(){
        NetworkManager.Singleton.StartClient();   // ホスト起動
    }
}
