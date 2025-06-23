using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
public class EnemyStatus : NetworkBehaviour
{
    public enum EnemyMoveState
    {
        Stop,
        Walk,
        Run
    }
    public FootStepSoundController foot_step_sound_controller;
    private EnemyMoveState m_enemy_move_state;
    public EnemyMoveState enemy_move_state
    {
        get { return m_enemy_move_state; }
        set {
            if (m_enemy_move_state != value)
            {
                m_enemy_move_state = value;
                OnChangeEnemyMoveEvent.Invoke();
            }

        }
    }
    public GameObject FaceExpandingCamera;

    public Transform EyePosition;
    [SerializeField] public CharacterAudioController character_audio_controller;
    private FollowObjectByNav follow_object_by_nav;
    private DiscoveryPlayerController discovery_player_controller;
    private Animator anim;
    private CharacterMovePermit character_move_permit;
    private TimeManager time_manager;
    private EnemyRoutingController enemy_routing_controller;
    [HideInInspector]public EnemyFlinchController enemy_flinch_controller;
    public bool StopEnemyByPlayerNow;
    private float StopEnemyByPlayerTimeNow;
    public GroundCheck ground_check;

    [SerializeField]
    public UnityEvent OnChangeEnemyMoveEvent;
    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        character_move_permit = this.gameObject.GetComponent<CharacterMovePermit>();
        follow_object_by_nav = this.gameObject.GetComponent<FollowObjectByNav>();
        discovery_player_controller = this.gameObject.GetComponent<DiscoveryPlayerController>();
        enemy_routing_controller = this.gameObject.GetComponent<EnemyRoutingController>();
        enemy_flinch_controller = this.gameObject.GetComponent<EnemyFlinchController>();
        anim = this.gameObject.GetComponent<Animator>();
        time_manager = TimeManager.instance;
    }
    

    private void Update()
    {
        if (!IsServer) return;
        if (foot_step_sound_controller != null)
        {
            switch (enemy_move_state)
            {
                case EnemyMoveState.Walk:
                    foot_step_sound_controller.move_status.Value = FootStepSoundController.MoveStatus.Walk;
                    break;
                case EnemyMoveState.Run:
                    foot_step_sound_controller.move_status.Value = FootStepSoundController.MoveStatus.Run;
                    break;
                default:
                    foot_step_sound_controller.move_status.Value = FootStepSoundController.MoveStatus.Stop;
                    break;
            }
        }


        if (enemy_move_state == EnemyMoveState.Run)
        {

            anim.SetFloat("MoveY", 1);
        }
        else if (enemy_move_state == EnemyMoveState.Walk)
        {

            anim.SetFloat("MoveY", 0.5f);
        }
        else if (enemy_move_state == EnemyMoveState.Stop)
        {

            anim.SetFloat("MoveY", 0);
        }
    }
    // Update is called once per frame

    public void StopEnemyByPlayer(float stop_time)
    {
        //サーバー側で実行されます。
        enemy_flinch_controller.FlinchStart(stop_time);
        enemy_routing_controller?.NearRouteSet();

    }
    
}
