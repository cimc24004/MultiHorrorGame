using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KubinashiMikoController : MonoBehaviour
{
    private FollowObjectByNav follow_object_by_nav;
    private PlayerStatus player_status;
    private EnemyStatus enemy_status;
    private DiscoveryPlayerController discovery_player_controller;
    private HorrorDirector horror_director;

    private bool no_discovery_player_once = false;
    // Start is called before the first frame update
    void Start()
    {
        player_status = PlayerStatus.instance;
        enemy_status = this.gameObject.GetComponent<EnemyStatus>();
        discovery_player_controller = this.gameObject.GetComponent<DiscoveryPlayerController>();
        follow_object_by_nav = this.gameObject.GetComponent<FollowObjectByNav>();
        horror_director = HorrorDirector.instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (!discovery_player_controller.DiscoveryPlayerNow)
        {
            enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Run;
            discovery_player_controller.DiscoveryPlayerNow = true;
            if(!no_discovery_player_once){
                horror_director.ResetSetting();
                no_discovery_player_once = true;
            }
            follow_object_by_nav.ChaseToTargetSet(true, player_status.gameObject.transform.position, 2);
        }else{
            no_discovery_player_once = false;
        }
       
    }
}
