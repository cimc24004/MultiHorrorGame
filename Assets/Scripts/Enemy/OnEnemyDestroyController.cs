using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class EnemyOnDestroyController : MonoBehaviour
{
    [SerializeField] private UnityEvent EnemyOnDestroyEvent;
    private ClosestEnemyController closest_enemy_controller;
    void Start()
    {
        closest_enemy_controller = ClosestEnemyController.instance;
    }
    void OnDestroy()
    {
        if(closest_enemy_controller.EnemiesInRangeWithToPlayerDistance.ContainsKey(this.gameObject)){
            closest_enemy_controller.EnemiesInRangeWithToPlayerDistance.Remove(this.gameObject);
        }
    }
}
