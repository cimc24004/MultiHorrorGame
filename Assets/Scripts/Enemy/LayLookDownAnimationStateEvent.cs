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

    private NetworkObjectReference targetRef;   // 追跡対象のNetworkObject参照
    private ulong targetClientId;               // 追跡対象のクライアントID

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsServer) return;

        discovery_player_controller = animator.gameObject.GetComponent<DiscoveryPlayerController>();
        follow_object_by_nav        = animator.gameObject.GetComponent<FollowObjectByNav>();

        // 追跡ターゲット取得
        FirstKeyOfDiscoveriedPlayers = discovery_player_controller != null
            ? discovery_player_controller.FirstKeyOfDiscoveriedPlayers
            : null;

        if (FirstKeyOfDiscoveriedPlayers == null)
        {
            Abort(animator, layerIndex);
            return;
        }

        // 直参照を保持
        player_status = FirstKeyOfDiscoveriedPlayers.GetComponent<PlayerStatus>();

        // NetworkObject をキャッシュ（TryResolveで使う）
        var no = FirstKeyOfDiscoveriedPlayers.GetComponent<NetworkObject>();
        if (no == null || !no.IsSpawned)
        {
            Abort(animator, layerIndex);
            return;
        }
        targetRef     = new NetworkObjectReference(no);
        targetClientId = no.OwnerClientId;

        // 追跡中は移動を止める
        if (follow_object_by_nav != null) follow_object_by_nav.MovePermit = false;

        // ここでは「解決できなきゃ即アボート」じゃなく、Updateで都度監視する
        // （ターゲットが瞬間的にnullでも復帰できるように）
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsServer) return;

        if (!TryResolvePlayerStatus(out var ok) || !ok || player_status == null)
        {
            Abort(animator, layerIndex);
            return;
        }

        // 水平成分だけ向きを合わせる
        Vector3 dir = player_status.transform.position - animator.transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        animator.transform.rotation = Quaternion.Slerp(
            animator.transform.rotation, targetRot, Time.deltaTime * 10f
        );
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsServer) return;

        // 念のため再解決（切断/Despawn直後対策）
        TryResolvePlayerStatus(out var ok);

        // MovePermitはなるべく戻す（nullガード）
        if (follow_object_by_nav != null) follow_object_by_nav.MovePermit = true;

        // 対象が有効なときだけキル処理/後始末を実行
        if (ok && player_status != null)
        {
            // 隠れ中なら専用キル
            if (player_status.HidingNow.Value)
            {
                var act = player_status.player_hide_object_action;
                if (act != null)
                {
                    act.LayLookDownAnimationEndDeathOnServer();
                    act.layLookDownTriggered = false;
                }
            }
            else
            {
                // 隠れていない場合でもトリガ解除だけは試みる
                var act = player_status.player_hide_object_action;
                if (act != null) act.layLookDownTriggered = false;
            }

            // 発見リストから外す（片方でもnullなら何もしない）
            if (discovery_player_controller != null)
            {
                discovery_player_controller.RemovePlayerOfDiscoveryPlayerLists(player_status.gameObject);
            }

            // 最後にデスイベント
            player_status.DeadEvent(animator.gameObject);
        }
        else
        {
            // 何もできない場合でもアニメ状態はIdleへ
            animator.Play("Idle", layerIndex, 0f);
        }
    }

    /// <summary>
    /// 追跡対象のPlayerStatusを安全に再解決。成功時 ok=true。
    /// 既存のplayer_statusが生きていればそれを優先。
    /// </summary>
    private bool TryResolvePlayerStatus(out bool ok)
    {
        ok = false;
        // 既に保持している参照が有効ならそれを使う
        if (player_status != null)
        {
            var go = player_status.gameObject;
            var no = go != null ? go.GetComponent<NetworkObject>() : null;

            if (go != null && go.activeInHierarchy && no != null && no.IsSpawned &&
                NetworkManager.Singleton.ConnectedClients.ContainsKey(no.OwnerClientId))
            {
                targetClientId = no.OwnerClientId;
                targetRef = new NetworkObjectReference(no);
                ok = true;
                return true;
            }
        }

        // NetworkObjectReference から解決
        if (targetRef.TryGet(out var targetNo) && targetNo != null && targetNo.IsSpawned)
        {
            var go = targetNo.gameObject;
            if (go != null && go.activeInHierarchy &&
                NetworkManager.Singleton.ConnectedClients.ContainsKey(targetNo.OwnerClientId))
            {
                player_status  = go.GetComponent<PlayerStatus>();
                targetClientId = targetNo.OwnerClientId;
                ok = (player_status != null);
                return ok;
            }
        }

        // 解決不可
        player_status = null;
        return false;
    }

    private void Abort(Animator animator, int layerIndex)
    {
        // 安全復帰
        if (follow_object_by_nav != null) follow_object_by_nav.MovePermit = true;

        // 触らない方が安全なものは触らない（発見解除はDiscovery側の監視に任せる）
        animator.Play("Idle", layerIndex, 0f);

        // 以降の処理を抑止
        player_status = null;
    }
}
