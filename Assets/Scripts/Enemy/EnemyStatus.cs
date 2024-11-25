using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatus : MonoBehaviour
{
    public enum EnemyMoveState
    {
        Stop,
        Walk,
        Run
    }
    public FootStepSoundController foot_step_sound_controller;
    public EnemyMoveState enemy_move_state;
    public GameObject FaceExpandingCamera;

    public Transform EyePosition;
    private FollowObjectByNav follow_object_by_nav;
    private DiscoveryPlayerController discovery_player_controller;
    private Animator anim;
    private CharacterMovePermit character_move_permit;
    private TimeManager time_manager;
    private EnemyRoutingController enemy_routing_controller;
    [HideInInspector]public EnemyFlinchController enemy_flinch_controller;
    public bool StopEnemyByPlayerNow;
    private float StopEnemyByPlayerTimeNow;

    // Start is called before the first frame update
    void Start()
    {
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
        switch (enemy_move_state)
        {
            case EnemyMoveState.Walk:
                foot_step_sound_controller.move_status = FootStepSoundController.MoveStatus.Walk;
                break;
            case EnemyMoveState.Run:
                foot_step_sound_controller.move_status = FootStepSoundController.MoveStatus.Run;
                break;
            default:
                foot_step_sound_controller.move_status = FootStepSoundController.MoveStatus.Stop;
                break;
        }

        if (enemy_move_state == EnemyMoveState.Run)
        {
            if (this.gameObject.name == "‹ƒ‚«—ì")

                Debug.Log("EnemyStatus.EnemyMoveState.Run");
            anim.SetFloat("MoveY", 1);
        }
        else if (enemy_move_state == EnemyMoveState.Walk)
        {
            if (this.gameObject.name == "‹ƒ‚«—ì")

                Debug.Log("EnemyStatus.EnemyMoveState.Walk");
            anim.SetFloat("MoveY", 0.5f);
        }
        else if (enemy_move_state == EnemyMoveState.Stop)
        {
            if (this.gameObject.name == "‹ƒ‚«—ì")

                Debug.Log("EnemyStatus.EnemyMoveState.Stop");
            anim.SetFloat("MoveY", 0);
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {

        if (follow_object_by_nav.ArrivedTarget)
        {
        }

    }
    public void StopEnemyByPlayer(float stop_time)
    {
        enemy_flinch_controller.FlinchStart(stop_time);
        enemy_routing_controller.NearRouteSet();

    }
    
}
