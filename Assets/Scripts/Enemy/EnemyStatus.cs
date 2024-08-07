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
    public EnemyMoveState enemy_move_state;
    public GameObject FaceExpandingCamera;
    private FollowObjectByNav follow_object_by_nav;
    private DiscoveryPlayerController discovery_player_controller;
    private Rigidbody rigid;
    private Animator anim;
    private CharacterMovePermit character_move_permit;
    private EnemyRoutingController enemy_routing_controller;
    public bool StopEnemyByPlayerNow;
    private float StopEnemyByPlayerTimeNow;
    // Start is called before the first frame update
    void Start()
    {
        rigid = this.gameObject.GetComponent<Rigidbody>();
        character_move_permit = this.gameObject.GetComponent<CharacterMovePermit>();
        follow_object_by_nav = this.gameObject.GetComponent<FollowObjectByNav>();
        discovery_player_controller = this.gameObject.GetComponent<DiscoveryPlayerController>();
        enemy_routing_controller = this.gameObject.GetComponent<EnemyRoutingController>();
        anim = this.gameObject.GetComponent<Animator>();
    }
    private void Update()
    {
        if (StopEnemyByPlayerNow)
        {
            StopEnemyByPlayerTimeNow -= TimeManager.instance.deltaTime;
            Debug.Log("StopEnemyByPlayerTimeNow: " + StopEnemyByPlayerTimeNow);
            if(StopEnemyByPlayerTimeNow <= 0)
            {
                StopEnemyByPlayerToMove();
            }
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {

        if (enemy_move_state == EnemyMoveState.Run)
        {
            anim.SetFloat("MoveY", 1);
        }else if(enemy_move_state == EnemyMoveState.Walk)
        {
            anim.SetFloat("MoveY", 0.5f);
        }
        else if(enemy_move_state == EnemyMoveState.Stop)
        {
            anim.SetFloat("MoveY", 0);
        }
        if (follow_object_by_nav.ArrivedTarget)
        {
            enemy_move_state = EnemyMoveState.Stop;
        }

    }
    public void StopEnemyByPlayer(float stop_time)
    {
        StopEnemyByPlayerTimeNow = stop_time;
        StopEnemyByPlayerNow = true;
        discovery_player_controller.DiscoveryPlayerNow = false;
        enemy_routing_controller.NearRouteSet();
        this.enemy_move_state = EnemyMoveState.Stop;
        character_move_permit.Stop();
    }
    public void StopEnemyByPlayerToMove()
    {
        character_move_permit.Move();

        StopEnemyByPlayerNow = false;
    }

    
}
