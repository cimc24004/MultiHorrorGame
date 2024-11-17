using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFlinchController : MonoBehaviour
{
    private FollowObjectByNav follow_object_by_nav;
    private EnemyToPlayerKillController enemy_to_player_kill_controller;
    private EnemyStatus enemy_status;
    private CharacterController character_controller;
    [SerializeField] private bool FlinchNow;
    [SerializeField] private float FlinchTime;
    [SerializeField] private float FlinchTimeNow;
    private Vector3 move = new Vector3(0, 1, 0);
    private DiscoveryPlayerController discovery_player_controller;
    private Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        follow_object_by_nav = this.gameObject.GetComponent<FollowObjectByNav>();
        enemy_status = this.gameObject.GetComponent<EnemyStatus>();
        enemy_to_player_kill_controller = this.gameObject.GetComponent<EnemyToPlayerKillController>();
        character_controller = this.gameObject.GetComponent<CharacterController>();
        discovery_player_controller = this.gameObject.GetComponent<DiscoveryPlayerController>();
        anim = this.gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (FlinchNow)
        {
            FlinchTimeNow += Time.deltaTime;
            enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Stop;
            enemy_to_player_kill_controller.enabled = false;
            discovery_player_controller.DiscoveryPlayerNow = false;
            discovery_player_controller.SightningPlayer = false;
            discovery_player_controller.SightningPlayerWhenHidePlayer = false;
            discovery_player_controller.enabled = false;
            character_controller.Move(Vector3.zero);
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
        discovery_player_controller.enabled = true;
        enemy_to_player_kill_controller.enabled = true;
        follow_object_by_nav.MovePermit = true;
        enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Walk;
        FlinchNow = false;
    }
}
