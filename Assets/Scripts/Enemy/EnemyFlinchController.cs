using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class EnemyFlinchController : NetworkBehaviour
{
    private FollowObjectByNav follow_object_by_nav;
    private EnemyToPlayerKillController enemy_to_player_kill_controller;
    private EnemyStatus enemy_status;
    private CharacterController character_controller;
    [SerializeField] public bool FlinchNow;
    [SerializeField] private float FlinchTime;
    [SerializeField] private float FlinchTimeNow;
    private DiscoveryPlayerController discovery_player_controller;
    private Animator anim;
    private Vector3 Move = new Vector3(0, -1, 0);
    private NavMeshAgent nav_mesh_agent;
    private float DefaultNavMeshSpeed;
    private Animator m_Animator;
    [Header("FlinchされたときにApplyMotionをチェック外すかどうか")]
    [SerializeField]
    private bool ChangeApplyMotionWhenFlinch = false;
    private bool DefaultApplyMotion;
    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        follow_object_by_nav = this.gameObject.GetComponent<FollowObjectByNav>();
        enemy_status = this.gameObject.GetComponent<EnemyStatus>();
        m_Animator = this.gameObject.GetComponent<Animator>();
        enemy_to_player_kill_controller = this.gameObject.GetComponent<EnemyToPlayerKillController>();
        character_controller = this.gameObject.GetComponent<CharacterController>();
        discovery_player_controller = this.gameObject.GetComponent<DiscoveryPlayerController>();
        anim = this.gameObject.GetComponent<Animator>();
        nav_mesh_agent = this.gameObject.GetComponent<NavMeshAgent>();
        DefaultNavMeshSpeed = nav_mesh_agent.speed;
        DefaultApplyMotion = m_Animator.applyRootMotion;
    }

    // Update is called once per frame
    void Update()
    {
        if (FlinchNow)
        {
            FlinchTimeNow += Time.deltaTime;
            enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Stop;
            enemy_to_player_kill_controller.enabled = false;
            discovery_player_controller.PlayersVisibilityMapStatuesWithKey[discovery_player_controller.FirstKeyOfDiscoveriedPlayers].DiscoveryPlayerNow = false;
            discovery_player_controller.PlayersVisibilityMapStatuesWithKey[discovery_player_controller.FirstKeyOfDiscoveriedPlayers].SightningPlayer = false;
            discovery_player_controller.SightningPlayerWhenHidePlayer = false;
            discovery_player_controller.enabled = false;
            nav_mesh_agent.speed = 0;
            if(ChangeApplyMotionWhenFlinch){
                m_Animator.applyRootMotion = false;
            }
            character_controller.Move(Move);
            if (FlinchTime < FlinchTimeNow)
            {
                FlinchEnd();
            }
        }
    }
    
    public void FlinchStart(float _time)
    {
        FlinchTime = _time;
        follow_object_by_nav.MovePermit = false;
        FlinchNow = true;
        anim.SetBool("Flinch", true);
    }
    public void FlinchEnd()
    {
        anim.SetBool("Flinch", false);
        FlinchTimeNow = 0;
        if(ChangeApplyMotionWhenFlinch){
            m_Animator.applyRootMotion = DefaultApplyMotion;
        }
        discovery_player_controller.enabled = true;
        nav_mesh_agent.speed = DefaultNavMeshSpeed;
        enemy_to_player_kill_controller.enabled = true;
        follow_object_by_nav.MovePermit = true;
        enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Walk;
        FlinchNow = false;
    }
}
