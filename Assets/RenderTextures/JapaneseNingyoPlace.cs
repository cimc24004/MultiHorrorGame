using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Netcode;
using Unity.Netcode;
public class JapaneseNingyoPlace : NetworkBehaviour
{
    [SerializeField] private GameObject EnemyNingyoPrefab;
    private DiscoveryPlayerController discovery_player_controller;
    [SerializeField] private Transform EnemyNingyoApperancePlaceTransform;
    private GameObject EnemyNingyo;

    [Button]
    public void EnemyNingyoApperance()
    {
        //Server Only
        EnemyNingyo = Instantiate(EnemyNingyoPrefab, EnemyNingyoApperancePlaceTransform.transform.position, Quaternion.identity);
        discovery_player_controller = EnemyNingyo.GetComponent<DiscoveryPlayerController>();
        discovery_player_controller.NetworkObject.Spawn();
        discovery_player_controller.DiscoveryPlayerBySightning = true;
        EnemyNingyoApperanceAllClientRpc();
        EnemyNingyoApperancePlaceTransform = null;
    }
    [ClientRpc]
    private void EnemyNingyoApperanceAllClientRpc()
    {
        Destroy(EnemyNingyoApperancePlaceTransform.gameObject, 0.1f);
    }
}
