using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayLookDownAnimationStateEvent : StateMachineBehaviour
{
    PlayerStatus player_status;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player_status = PlayerStatus.LocalInstance;
        animator.gameObject.transform.forward = (player_status.gameObject.transform.position - animator.gameObject.transform.position).normalized;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //机に隠れてるの見つけたーアニメーションが終わったらプレイヤーを殺す。
        if (player_status.HidingNow)
        {
            player_status.player_hide_object_action.UnHide();
        }
        player_status.DeadEvent(animator.gameObject);
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
