using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToPlayerNearSensorForEnemy : MonoBehaviour
{
    //敵が近くにいるかどうかのセンサー

    //NearAreaに触れればplayerToEnemyDistanceControllerのnearをtrueにする

    private GameObject Player;
    private PlayerToEnemyDistanceController player_to_enemy_distance_controller;
    [SerializeField]private bool Trigger;
    // Start is called before the first frame update
    void Start()
    {
        Player = PlayerStatus.instance.gameObject;
        player_to_enemy_distance_controller = Player.gameObject.GetComponent<PlayerToEnemyDistanceController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Trigger)
        {
            player_to_enemy_distance_controller.AwayEnemy = false;
        }
        else
        {

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            player_to_enemy_distance_controller.NearEnemyStart = true;
            Trigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            Debug.Log("敵がプレイヤーと離れましたのでAwayEnemyをTrueにします From" + this.gameObject.transform.parent);
            player_to_enemy_distance_controller.AwayEnemy = true;
            Trigger = false;
        }
    }
}
