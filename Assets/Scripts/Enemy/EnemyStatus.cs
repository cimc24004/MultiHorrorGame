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
    // Start is called before the first frame update
    void Start()
    {
        rigid = this.gameObject.GetComponent<Rigidbody>();
        follow_object_by_nav = this.gameObject.GetComponent<FollowObjectByNav>();
        discovery_player_controller = this.gameObject.GetComponent<DiscoveryPlayerController>();
        anim = this.gameObject.GetComponent<Animator>();
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
}
