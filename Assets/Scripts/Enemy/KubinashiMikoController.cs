using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using GSpawn_Pro;
using UnityEngine;
using UnityEngine.AI;

public class KubinashiMikoController : MonoBehaviour
{
    private FollowObjectByNav follow_object_by_nav;
    private PlayerStatus player_status;
    private EnemyStatus enemy_status;
    private DiscoveryPlayerController discovery_player_controller;
    private CharacterController character_controller;
    private NavMeshAgent nav_mesh_agent;
    private HorrorDirector horror_director;
    private EnemyActionForDoorController enemy_action_for_door_controller;
    private bool no_discovery_player_once = false;
    [SerializeField] private float DiscoveryPlayerTimeNow;
    // Start is called before the first frame update
    void Start()
    {
        player_status = PlayerStatus.LocalInstance;
        enemy_status = this.gameObject.GetComponent<EnemyStatus>();
        discovery_player_controller = this.gameObject.GetComponent<DiscoveryPlayerController>();
        follow_object_by_nav = this.gameObject.GetComponent<FollowObjectByNav>();
        character_controller = this.gameObject.GetComponent<CharacterController>();
        nav_mesh_agent = this.gameObject.GetComponent<NavMeshAgent>();
        horror_director = HorrorDirector.instance;
        enemy_action_for_door_controller = this.gameObject.GetComponent<EnemyActionForDoorController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!discovery_player_controller.PlayersVisibilityMapStatuesWithKey[discovery_player_controller.FirstKeyOfDiscoveriedPlayers].DiscoveryPlayerNow)
        {
            // 
            discovery_player_controller.PlayersVisibilityMapStatuesWithKey[discovery_player_controller.FirstKeyOfDiscoveriedPlayers].DiscoveryPlayerNow = true;
            no_discovery_player_once = false;

        }else{
            if(!no_discovery_player_once){
                DOVirtual.DelayedCall(3, () => {
                    horror_director.EnemyLostPlayer();
                });
                no_discovery_player_once = true;
            }

        }


        // if(!enemy_action_for_door_controller.DoorAttackNow){
        //     enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Run;
        //     follow_object_by_nav.ChaseToTargetSet(true, player_status.gameObject.transform.position, 2);
        // }
    }
}
