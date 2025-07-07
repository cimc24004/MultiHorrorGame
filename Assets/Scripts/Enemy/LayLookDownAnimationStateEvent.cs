using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LayLookDownAnimationStateEvent : StateMachineBehaviour
{
    private PlayerStatus player_status;
    private DiscoveryPlayerController discovery_player_controller;
    private GameObject FirstKeyOfDiscoveriedPlayers;
    private FollowObjectByNav follow_object_by_nav;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        discovery_player_controller = animator.gameObject.GetComponent<DiscoveryPlayerController>();
        follow_object_by_nav = animator.gameObject.GetComponent<FollowObjectByNav>();
        FirstKeyOfDiscoveriedPlayers = discovery_player_controller.FirstKeyOfDiscoveriedPlayers;
        if (FirstKeyOfDiscoveriedPlayers == null)
        {
            animator.Play("Idle", layerIndex, 0f);      // または CrossFadeInFixedTime("Idle", 0f)
            return;
        }
        player_status = FirstKeyOfDiscoveriedPlayers.GetComponent<PlayerStatus>();
        follow_object_by_nav.MovePermit = false;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 水平成分だけ取り出す
        if (!NetworkManager.Singleton.IsServer) return;
        Vector3 dir = player_status.transform.position - animator.transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return;    // ほぼ同じ位置なら回さない

        Quaternion targetRot = Quaternion.LookRotation(dir);
        animator.transform.rotation =
            Quaternion.Slerp(animator.transform.rotation, targetRot, Time.deltaTime * 10);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //机に隠れてるの見つけたーアニメーションが終わったらプレイヤーを殺す。
        if (!NetworkManager.Singleton.IsServer) return;
        if (player_status.HidingNow.Value)
        {
            player_status.player_hide_object_action.LayLookDownAnimationEndDeathOnServer();
        }
        player_status.player_hide_object_action.layLookDownTriggered = false;
        follow_object_by_nav.MovePermit = true;
        discovery_player_controller.RemovePlayerOfDiscoveryPlayerLists(player_status.gameObject);
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
