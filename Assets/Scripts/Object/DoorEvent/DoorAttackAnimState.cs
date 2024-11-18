using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorAttackAnimState : StateMachineBehaviour
{
    private float previousNormalizedTime;
    private EnemyActionForDoorController enemy_action_for_door_controller;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float currentNormalizedTime = stateInfo.normalizedTime % 1.0f;
        if (currentNormalizedTime < previousNormalizedTime)
        {
            // ループアニメーションの1サイクル終了時の処理
            enemy_action_for_door_controller = animator.gameObject.GetComponent<EnemyActionForDoorController>();
            if (enemy_action_for_door_controller.the_door_controller != null)
            {
                enemy_action_for_door_controller.AttackToDoor(enemy_action_for_door_controller.the_door_controller);

            }
        }

        // previousNormalizedTimeを更新
        previousNormalizedTime = currentNormalizedTime;
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    Debug.Log("State   Exit--------------");
    //}

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
