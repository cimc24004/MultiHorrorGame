using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OshiireMouzyaController : MonoBehaviour
{
    [SerializeField]
    private Collidered OshiireAttackRange;
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
    private PlayerStatus player_status;
    private EnemyFlinchController enemy_flinch_controller;
    private EnemyDeathController enemy_death_controller;
    private EnemyToPlayerKillController enemy_to_player_kill_controller;
    [SerializeField] private GameObject AttackedDoorEffectPrefab;

    private FootStepSoundController player_foot_step_sound_controller;
    private EnemyActionForDoorController enemy_action_for_door_controller;
    // Start is called before the first frame update
    void Start()
    {
        discovery_player_controller = this.gameObject.GetComponent<DiscoveryPlayerController>();
        m_Animator = this.gameObject.GetComponent<Animator>();
        enemy_death_controller = this.gameObject.GetComponent<EnemyDeathController>();
        enemy_action_for_door_controller = this.gameObject.GetComponent<EnemyActionForDoorController>();
        enemy_status = this.gameObject.GetComponent<EnemyStatus>();
        enemy_flinch_controller = this.gameObject.GetComponent<EnemyFlinchController>();
        follow_object_by_nav = this.gameObject.GetComponent<FollowObjectByNav>();
        enemy_to_player_kill_controller = this.gameObject.GetComponent<EnemyToPlayerKillController>();
        player_status = PlayerStatus.LocalInstance;
        player_foot_step_sound_controller = player_status.foot_step_sound_controller;
        player_near_sensor.IsNearToKillEffect = false;

        foreach (Transform door in door_attacked_controller.gameObject.transform)
        {
            Debug.Log("Door:r:r::" + door.gameObject.name);
            if(door.tag == "HideActionObject")
            {
                door.tag = "Untagged";
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (OshiireAttackRange.Collider && !BrokenOshiire)
        {
            if(!(player_foot_step_sound_controller.FootStepSoundLevel == 0))
            {
                discovery_player_controller.DiscoveryPlayerNow = true;
                OshiireAttackMode = true;
                if (!BrokenOshiire)
                {
                    follow_object_by_nav.MovePermit = false;
                    enemy_action_for_door_controller.door_attacked_controller = this.door_attacked_controller;
                    enemy_to_player_kill_controller.enabled = false;
                }
            }

        }
        else
        {
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
                discovery_player_controller.DiscoveryPlayerNow = false;
            }
        }
    }


}
