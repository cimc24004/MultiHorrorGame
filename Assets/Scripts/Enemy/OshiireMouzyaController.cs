using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OshiireMouzyaController : NetworkBehaviour
{
    [SerializeField]
    private CollideredForPlayers OshiireAttackRangeWithPlayers;
    [SerializeField]
    private DoorAttackedController door_attacked_controller;
    [SerializeField]
    private PlayerNearSensor player_near_sensor;
    [SerializeField]
    private float ToDeathWaitTimeFromBrokeOshiire = 1;
    private float ToDeathWaitTimeNowFromBrokeOshiire = 0;
    [SerializeField]
    private float OshiireAttackWaitTime = 1;
    [SerializeField]
    private float OshiireAttackWaitTimeNow = 0;

    private FollowObjectByNav follow_object_by_nav;

    private DiscoveryPlayerController discovery_player_controller;
    [SerializeField]
    private bool OshiireAttackMode;
    private Animator m_Animator;
    private bool BrokenOshiire;
    private EnemyStatus enemy_status;
    private EnemyDeathController enemy_death_controller;
    private EnemyToPlayerKillController enemy_to_player_kill_controller;
    [SerializeField] private GameObject AttackedDoorEffectPrefab;

    private FootStepSoundController player_foot_step_sound_controller;
    private EnemyActionForDoorController enemy_action_for_door_controller;
    private PlayerAllController player_all_controller;
    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        discovery_player_controller = this.gameObject.GetComponent<DiscoveryPlayerController>();
        m_Animator = this.gameObject.GetComponent<Animator>();
        player_all_controller = PlayerAllController.instance;
        enemy_death_controller = this.gameObject.GetComponent<EnemyDeathController>();
        enemy_action_for_door_controller = this.gameObject.GetComponent<EnemyActionForDoorController>();
        enemy_status = this.gameObject.GetComponent<EnemyStatus>();
        follow_object_by_nav = this.gameObject.GetComponent<FollowObjectByNav>();
        enemy_to_player_kill_controller = this.gameObject.GetComponent<EnemyToPlayerKillController>();
        player_near_sensor.IsNearToKillEffect = false;

        foreach (Transform door in door_attacked_controller.gameObject.transform)
        {
            Debug.Log("Door:r:r::" + door.gameObject.name);
            if (door.tag == "HideActionObject")
            {
                door.tag = "Untagged";
            }
        }
        follow_object_by_nav.MovePermit = false;

    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;
        if (OshiireAttackRangeWithPlayers.CollideredObjes.Count != 0 && !BrokenOshiire)
        {
            for (int i = 0; i < OshiireAttackRangeWithPlayers.CollideredObjes.Count; i++)
            {
                GameObject Player = OshiireAttackRangeWithPlayers.CollideredObjes[i];
                Debug.Log("OshiireAttackRange-???Player: " + Player);
                Debug.Log("player_all_controller: " + player_all_controller);
                FootStepSoundController player_foot_step_sound_controller = player_all_controller.SpawnedPlayersWithPlayerStatuses[Player].foot_step_sound_controller;
                if (!(player_foot_step_sound_controller.FootStepSoundLevel.Value == 0))
                {
                    //足音聞こえていたら
                    // discovery_player_controller.PlayersVisibilityMapStatuesWithKey[discovery_player_controller.FirstKeyOfDiscoveriedPlayers].DiscoveryPlayerNow = true;
                    discovery_player_controller.AddPlayerToDiscoveryPlayerLists(Player);
                    OshiireAttackMode = true;
                    if (!BrokenOshiire)
                    {
                        // follow_object_by_nav.MovePermit = false;
                        enemy_action_for_door_controller.door_attacked_controller = this.door_attacked_controller;
                        enemy_to_player_kill_controller.enabled = false;
                    }
                }
                else
                {
                }
            }


        }

        if (OshiireAttackMode)
        {
            enemy_action_for_door_controller.enabled = true;
            enemy_action_for_door_controller.AutoDoorAttack = false;
            m_Animator.SetBool("DoorAttack", true);
            if (!BrokenOshiire)
            {
                follow_object_by_nav.enabled = false;
                enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Stop;

                if(enemy_action_for_door_controller.DoorBroken){
                    follow_object_by_nav.enabled = true;
                    m_Animator.SetBool("DoorAttack", false);
                    enemy_action_for_door_controller.door_attacked_controller = null;
                    enemy_action_for_door_controller.the_door_controller = null;
                    enemy_action_for_door_controller.DoorAttackNow = false;
                    enemy_action_for_door_controller.AutoDoorAttack = true;
                    enemy_action_for_door_controller.DoorBroken = false;
                    follow_object_by_nav.MovePermit = true;
                    enemy_to_player_kill_controller.enabled = true;
                    player_near_sensor.IsNearToKillEffect = true;
                    BrokenOshiire = true;
                    OshiireAttackMode = false;
                }

            }
        }
        if (BrokenOshiire)
        {
            ToDeathWaitTimeNowFromBrokeOshiire += Time.deltaTime;
            if(ToDeathWaitTimeNowFromBrokeOshiire > ToDeathWaitTimeFromBrokeOshiire)
            {
                enemy_death_controller.DeadStart();
                discovery_player_controller.PlayersVisibilityMapStatuesWithKey[discovery_player_controller.FirstKeyOfDiscoveriedPlayers].DiscoveryPlayerNow = false;
            }
        }
    }


}
