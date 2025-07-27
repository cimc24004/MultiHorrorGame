using UnityEngine;
using Unity.Netcode;
public abstract class NetworkSingleton<T> : NetworkBehaviour where T : NetworkBehaviour
{
    public static T Instance { get; private set; }
    // private void Awake()
    // {
    //     if (Instance == null)
    //     {
    //         Instance = this as T;
    //     }
    // }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (Instance == null)
        {
            Instance = this as T;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
