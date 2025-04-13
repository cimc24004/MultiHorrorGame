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
    private AudioSource OshiireAttackSource;
    [SerializeField]
    private AudioClip OshiireAttackAudioClip;
    [SerializeField]
    private AudioClip OshiireBrokeAudioClip;

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
    // Start is called before the first frame update
    void Start()
    {
        discovery_player_controller = this.gameObject.GetComponent<DiscoveryPlayerController>();
        m_Animator = this.gameObject.GetComponent<Animator>();
        enemy_death_controller = this.gameObject.GetComponent<EnemyDeathController>();
        enemy_status = this.gameObject.GetComponent<EnemyStatus>();
        enemy_flinch_controller = this.gameObject.GetComponent<EnemyFlinchController>();
        follow_object_by_nav = this.gameObject.GetComponent<FollowObjectByNav>();
        enemy_to_player_kill_controller = this.gameObject.GetComponent<EnemyToPlayerKillController>();
        player_status = PlayerStatus.instance;
        player_foot_step_sound_controller = player_status.foot_step_sound_controller;


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
        if (OshiireAttackRange.Collider)
        {
            if(!(player_foot_step_sound_controller.FootStepSoundLevel == 0))
            {
                discovery_player_controller.DiscoveryPlayerNow = true;
                OshiireAttackMode = true;
                if (!BrokenOshiire)
                {
                    follow_object_by_nav.MovePermit = false;
                    enemy_to_player_kill_controller.enabled = false;
                }
            }

        }
        else
        {
        }

        if (OshiireAttackMode)
        {
            // m_Animator.SetBool("DoorAttack", true);
            if (!BrokenOshiire)
            {
                enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Stop;
                OshiireAttackWaitTimeNow += Time.deltaTime;
                if(OshiireAttackWaitTimeNow > OshiireAttackWaitTime)
                {
                    AttackToOshiire(1);
                    OshiireAttackWaitTimeNow = 0;
                }
            }
        }
        if (BrokenOshiire)
        {
            ToDeathWaitTimeNowFromBrokeOshiire += Time.deltaTime;
            if(ToDeathWaitTimeNowFromBrokeOshiire > ToDeathWaitTimeFromBrokeOshiire)
            {
                enemy_to_player_kill_controller.enabled = true;
                follow_object_by_nav.MovePermit = false;
                enemy_death_controller.DeadStart();
                discovery_player_controller.DiscoveryPlayerNow = false;
            }
        }
    }

    public void AttackToOshiire(int AttackDoorPlusCount)
    {
        if (BrokenOshiire) return;
        bool broken = door_attacked_controller.DoorAttacked(AttackDoorPlusCount);
        if (broken)
        {
            this.gameObject.transform.SetParent(null);
            follow_object_by_nav.MovePermit = true;
            BrokenOshiire = true;
            OshiireAttackMode = false;
            OshiireAttackSource.PlayOneShot(OshiireBrokeAudioClip);
            OshiireAttackSource.PlayOneShot(OshiireAttackAudioClip);
        }
        else
        {
            GameObject effect = Instantiate(AttackedDoorEffectPrefab, door_attacked_controller.Doors[0].gameObject.transform.position + (Vector3.up * 0.2f), Quaternion.identity);
            effect.transform.forward = this.transform.forward;
            BrokenOshiire = false;
            OshiireAttackSource.PlayOneShot(OshiireAttackAudioClip);
        }
    }
}
