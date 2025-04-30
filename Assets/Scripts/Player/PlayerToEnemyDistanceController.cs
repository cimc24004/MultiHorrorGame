using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerToEnemyDistanceController : SerializedMonoBehaviour
{
    // 範囲内の敵を追跡するリスト
    [SerializeField]
    public Dictionary<GameObject, PlayerNearSensor> EnemiesWithPlayerNearSensors = new Dictionary<GameObject, PlayerNearSensor>(); 
    public bool NearEnemy;

    private HorrorDirector horror_director;
    private ClosestEnemyController closest_enemy_controller;
    private void Start()
    {
        horror_director = HorrorDirector.instance;
        closest_enemy_controller = ClosestEnemyController.instance;
    }

    private void Update()
    {
        if (NearEnemy)
        {
        }
    }
    // トリガーに敵が入った時
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerNearSensor")) // "Enemy"タグが付いたオブジェクトのみ反応
        {
            GameObject Enemy = other.gameObject.transform.parent.gameObject;
            PlayerNearSensor player_near_sensor = other.gameObject.GetComponent<PlayerNearSensor>();
            // リストに敵を追加
            if (!EnemiesWithPlayerNearSensors.ContainsKey(Enemy))
            {

                EnemiesWithPlayerNearSensors.Add(Enemy, player_near_sensor);

                if(player_near_sensor.IsNearEffect){
                    horror_director.AddNearEnemy();
                }
            }
            ClosestEnemyController.ClosestEnemyWithDistanceValue closestEnemyWithDistanceValue = new ClosestEnemyController.ClosestEnemyWithDistanceValue(){
                Distance = 0,
                player_near_sensor = player_near_sensor
            };
            if(!closest_enemy_controller.EnemiesInRangeWithToPlayerDistance.ContainsKey(Enemy)){
                closest_enemy_controller.EnemiesInRangeWithToPlayerDistance.Add(Enemy, closestEnemyWithDistanceValue);
            }
        }
    }
    private void OnTriggerStay(Collider other)
    {
        
        if(other.CompareTag("PlayerNearSensor")){
            GameObject Enemy = other.gameObject.transform.parent.gameObject;
            if(closest_enemy_controller.EnemiesInRangeWithToPlayerDistance.ContainsKey(Enemy)){
                closest_enemy_controller.EnemiesInRangeWithToPlayerDistance[Enemy].Distance = Vector3.Distance(this.transform.position, other.gameObject.transform.position);
            }
        }
    }
    // トリガーから敵が出た時
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerNearSensor")) // "Enemy"タグが付いたオブジェクトのみ反応
        {
            GameObject Enemy = other.gameObject.transform.parent.gameObject;
            // リストから敵を削除
            if (EnemiesWithPlayerNearSensors.ContainsKey(Enemy))
            {
                
                if(EnemiesWithPlayerNearSensors[Enemy].IsNearEffect){
                    horror_director.RemoveNearEnemy();
                }

                EnemiesWithPlayerNearSensors.Remove(Enemy);
            }
            if(closest_enemy_controller.EnemiesInRangeWithToPlayerDistance.ContainsKey(Enemy)){
                closest_enemy_controller.EnemiesInRangeWithToPlayerDistance.Remove(Enemy);
            }
        }
    }

}
