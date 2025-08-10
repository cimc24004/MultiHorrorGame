using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using GSpawn_Pro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class KubinashiMikoController : NetworkBehaviour
{
    private FollowObjectByNav follow_object_by_nav;
    private PlayerStatus player_status;
    private EnemyStatus enemy_status;
    private DiscoveryPlayerController discovery_player_controller;
    private PlayerAllController player_all_controller;
    private EnemyToPlayerKillController enemyToPlayerKillController;
    [SerializeField] private float DiscoveryPlayerTimeNow;

    [SerializeField] private List<GameObject> TargetPlayers;
    [SerializeField] private GameObject ChaseTargetPlayer;
    private MoveSpeedController moveSpeedController;
    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        player_status = PlayerStatus.LocalInstance;
        enemy_status = this.gameObject.GetComponent<EnemyStatus>();
        moveSpeedController = this.gameObject.GetComponent<MoveSpeedController>();
        discovery_player_controller = this.gameObject.GetComponent<DiscoveryPlayerController>();
        follow_object_by_nav = this.gameObject.GetComponent<FollowObjectByNav>();
        player_all_controller = PlayerAllController.instance;
        TargetPlayers = player_all_controller.SpawnedPlayers;
        ChaseTargetPlayer = TargetPlayers[Random.Range(0, TargetPlayers.Count)];
        enemyToPlayerKillController = this.gameObject.GetComponent<EnemyToPlayerKillController>();
        enemyToPlayerKillController.KillPlayerEvent.AddListener((_player) =>
        {
            TargetPlayers.Remove(_player);
            ChaseTargetPlayer = TargetPlayers[Random.Range(0, TargetPlayers.Count)];
        });
        foreach (GameObject targetPlayer in TargetPlayers)
        {
            PlayerStatus killedPlayerStatus = player_all_controller.SpawnedPlayersWithPlayerStatuses[targetPlayer];
            //RevivalPlayerEventにAddListenerする。つまり 復活した場合はその復活したやつもターゲットに
            killedPlayerStatus.RevivalPlayerEvent.AddListener(() =>
            {
                //プレイヤーが
                TargetPlayers.Add(targetPlayer);
            });
        }
        PlayerStatus.OnDestroyPlayer += OnDestroyPlayer;
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        PlayerStatus.OnDestroyPlayer -= OnDestroyPlayer;
    }
    private void OnDestroyPlayer(PlayerStatus p)
    {
        TargetPlayers.Remove(p.gameObject);
        ChaseTargetPlayer = TargetPlayers[Random.Range(0, TargetPlayers.Count)];

    }
    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;
        if (!discovery_player_controller.DiscoveryPlayerNowCheck())
        {
            if (ChaseTargetPlayer != null)
            {
                follow_object_by_nav.ChaseToTargetSet(true,ChaseTargetPlayer.transform.position,moveSpeedController.WalkSpeed, FollowObjectByNav.MoveStyle.Run);
            }
            // no_discovery_player_once = false;

        }
        else
        {
            // if (!no_discovery_player_once)
            // {
            //     DOVirtual.DelayedCall(3, () =>
            //     {
            //         horror_director.EnemyLostPlayer();
            //     });
            //     no_discovery_player_once = true;
            // }

        }


        // if(!enemy_action_for_door_controller.DoorAttackNow){
        //     enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Run;
        //     follow_object_by_nav.ChaseToTargetSet(true, player_status.gameObject.transform.position, 2);
        // }
    }
}
