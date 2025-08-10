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
    private NetworkObjectReference targetRef;
    private ulong targetClientId;
    // 切断コールバック登録フラグ
    private bool subscribedDisconnect;

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
        // 最初の解決を試みる
        player_status = TryResolvePlayerStatus(out var ok) ? player_status : null;
        if (!ok)
        {
            Abort(animator, layerIndex);
            return;
        }

        follow_object_by_nav.MovePermit = false;

        // 対象クライアントの切断を検知してアボート
        subscribedDisconnect = true;

    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 水平成分だけ取り出す
        if (!NetworkManager.Singleton.IsServer) return;
        // 毎フレーム解決できるかチェック（切断・Despawn・非Active対策）
        if (!TryResolvePlayerStatus(out var ok) || player_status == null)
        {
            Abort(animator, layerIndex);
            return;
        }
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
        // 切断イベント解除
        if (subscribedDisconnect && NetworkManager.Singleton != null)
        {
            subscribedDisconnect = false;
        }

        // 以降は「解決できたら実行」方針（nullガード徹底）
        TryResolvePlayerStatus(out var ok);

        if (player_status.HidingNow.Value)
        {
            player_status.player_hide_object_action.LayLookDownAnimationEndDeathOnServer();
        }
        player_status.player_hide_object_action.layLookDownTriggered = false;
        follow_object_by_nav.MovePermit = true;
        discovery_player_controller.RemovePlayerOfDiscoveryPlayerLists(player_status.gameObject);
        player_status.DeadEvent(animator.gameObject);
    }
    private bool TryResolvePlayerStatus(out bool ok)
    {
        ok = false;

        // NetworkObjectReference → NetworkObject へ解決
        if (!targetRef.TryGet(out var targetNo)) return false;
        if (targetNo == null || !targetNo.IsSpawned) return false;

        // クライアントがまだ接続中か
        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(targetClientId)) return false;

        // 実体取得
        var go = targetNo.gameObject;
        if (go == null || !go.activeInHierarchy) return false;

        player_status ??= go.GetComponent<PlayerStatus>();
        ok = (player_status != null);
        return ok;
    }

    private void Abort(Animator animator, int layerIndex)
    {
        // 途中でターゲット喪失・切断など → 安全に抜ける
        if (follow_object_by_nav != null) follow_object_by_nav.MovePermit = true;

        // 進行中だった各種フラグ/状態を最低限戻す
        // （発見状態の整理は Discovery 側の監視で行うのが理想。ここでは「触らない」方が副作用少ない）

        animator.Play("Idle", layerIndex, 0f);

        // 以降の Update/Exit で余計な処理しないように
        player_status = null;

        // 切断イベント解除
        if (subscribedDisconnect && NetworkManager.Singleton != null)
        {
            subscribedDisconnect = false;
        }
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
