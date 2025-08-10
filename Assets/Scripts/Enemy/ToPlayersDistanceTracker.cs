using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class ToPlayersDistanceTracker : NetworkBehaviour
{
    [SerializeField, ShowInInspector] public Dictionary<GameObject, float> ToPlayerDistanceLists = new Dictionary<GameObject, float>();

    private List<GameObject> Players = new List<GameObject>();
    private PlayerAllController playerAllController;
    private int i = 0;
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        playerAllController = PlayerAllController.instance;
        StartCoroutine(DictionarySet());
        PlayerStatus.OnDestroyPlayer += OnDestroyPlayer;
    }
    public override void OnNetworkDespawn()
    {
        PlayerStatus.OnDestroyPlayer -= OnDestroyPlayer;
    }
    private void OnDestroyPlayer(PlayerStatus p)
    {
        Players.Remove(p.gameObject);
        ToPlayerDistanceLists.Remove(p.gameObject);

    }
    IEnumerator DictionarySet()
    {
        yield return null;
        foreach (NetworkObject PlayerNetworkObject in NetworkManager.SpawnManager.PlayerObjects)
        {
            ToPlayerDistanceLists.Add(PlayerNetworkObject.gameObject, 0f);
            Players.Add(PlayerNetworkObject.gameObject);
        }

    }

    void Update()
    {
        if (!IsServer) return;
        for(i = 0; i < Players.Count; i++)
        {
            ToPlayerDistanceLists[Players[i]] = Vector3.Distance(this.gameObject.transform.position, Players[i].transform.position);
        }
    }
}
