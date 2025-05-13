using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
public class ClosestEnemyController : Singleton<ClosestEnemyController>
{
    public class ClosestEnemyWithDistanceValue{
        public float Distance;
        public PlayerNearSensor player_near_sensor;
    }
    //EnemyNearSensorの範囲内にいる敵たち
    private PlayerToEnemyDistanceController player_to_enemy_distance_controller;
    private PlayerStatus player_status;
    public Dictionary<GameObject, ClosestEnemyWithDistanceValue> EnemiesInRangeWithToPlayerDistance = new Dictionary<GameObject, ClosestEnemyWithDistanceValue>();
    
    public KeyValuePair<GameObject,ClosestEnemyWithDistanceValue> ClosestEnemyWithDistance = new KeyValuePair<GameObject, ClosestEnemyWithDistanceValue>();
    // Start is called before the first frame update
    void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InitWhenPlayerSpawned;
    }
    void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitWhenPlayerSpawned;
    }
    private void InitWhenPlayerSpawned(PlayerStatus _player_status){
        player_status = _player_status;
        player_to_enemy_distance_controller = player_status.gameObject.GetComponent<PlayerToEnemyDistanceController>();
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (EnemiesInRangeWithToPlayerDistance.Count == 0)
        {
            ClosestEnemyWithDistance = new KeyValuePair<GameObject, ClosestEnemyWithDistanceValue>();
            return;
        }

        // Value（距離）が最小のペアを取得して、その Key（GameObject）を代入
        var closestPair = EnemiesInRangeWithToPlayerDistance
            .OrderBy(pair => pair.Value.Distance)
            .First();

        ClosestEnemyWithDistance = closestPair;
    }
}
