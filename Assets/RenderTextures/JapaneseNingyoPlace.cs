using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public class JapaneseNingyoPlace : SerializedMonoBehaviour
{
    [SerializeField] private GameObject EnemyNingyoPrefab;
    private DiscoveryPlayerController discovery_player_controller;
    private FollowObjectByNav follow_object_by_nav;
    [SerializeField] private Transform EnemyNingyoApperancePlaceTransform;
    private GameObject EnemyNingyo;

    // Start is called before the first frame update
    void Start()
    {
        
    }
    [Button]
    public void EnemyNingyoApperance(){
        EnemyNingyo = Instantiate(EnemyNingyoPrefab, EnemyNingyoApperancePlaceTransform.transform.position, Quaternion.identity);
        discovery_player_controller = EnemyNingyo.GetComponent<DiscoveryPlayerController>();
        discovery_player_controller.DiscoveryPlayerBySightning = false;

        Destroy(EnemyNingyoApperancePlaceTransform.gameObject, 0.1f);
        EnemyNingyoApperancePlaceTransform = null;
    }
    [Button]
    public void EnemyNingyoDiscoveryPlayer(){
        discovery_player_controller.DiscoveryPlayerNow = true;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
