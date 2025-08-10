using UnityEngine;
using System.Reflection;
using Unity.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
public class PlayerAllController : Singleton<PlayerAllController>
{
    [SerializeField] public List<GameObject> SpawnedPlayers = new List<GameObject>();
    [SerializeField] public Dictionary<GameObject, PlayerStatus> SpawnedPlayersWithPlayerStatuses = new Dictionary<GameObject, PlayerStatus>();
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(SpawnedPlayerSet());
        PlayerStatus.OnDestroyPlayer += OnDestroyPlayer;

    }
    void OnDestroy()
    {
        PlayerStatus.OnDestroyPlayer -= OnDestroyPlayer;
    }
    private void OnDestroyPlayer(PlayerStatus p) {
        SpawnedPlayers.Remove(p.gameObject);
        SpawnedPlayersWithPlayerStatuses.Remove(p.gameObject);
    }
    IEnumerator SpawnedPlayerSet()
    {
        yield return null;
        foreach (NetworkObject pObj in NetworkManager.Singleton.SpawnManager.PlayerObjects)
        {
            SpawnedPlayersWithPlayerStatuses.Add(pObj.gameObject,pObj.GetComponent<PlayerStatus>());
            SpawnedPlayers.Add(pObj.gameObject);
        }

    }

}
