using UnityEngine;
using Unity.Netcode;

// これを NetworkManager と同じGameObjectにアタッチ
[DefaultExecutionOrder(-10000)] // できる限り早く動かす
public class NetworkManagerGate : MonoBehaviour
{
    void Awake()
    {
        Debug.Log("NetworkManagerGate Awake");
        var mine = GetComponent<NetworkManager>();
        var exist = NetworkManager.Singleton;

        if (exist != null && exist != mine)
        {
            // 既に生き残ってるNMがいる → このシーンのNMは破棄
            Destroy(gameObject);
            return;
        }

        // まだ無いなら、こいつを永続化
        DontDestroyOnLoad(gameObject);
    }
}
