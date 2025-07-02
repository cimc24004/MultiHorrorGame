// VisibledEnemyController.cs
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
public class VisibledEnemyController : NetworkBehaviour
{
    /* ========= 参照 ========= */
    private FollowObjectByNav follow_object_by_nav;
    private DiscoveryPlayerController discovery_player_controller;
    private EnemyToPlayerKillController enemy_to_player_kill_controller;
    private EnemyFlinchController enemy_flinch_controller;
    private EnemyStatus enemy_status;
    private CharacterController character_controller;
    private NavMeshAgent nav_mesh_agent;
    private Animator animator;

    /* ========= 可視判定用 ========= */
    [Header("遮蔽物として判定するレイヤー")] 
    [SerializeField] private LayerMask obstacleMask;

    private Camera cam;               // クライアント側でのみ使う
    [SerializeField, ShowInInspector]private bool  lastVisible;        // クライアント側の前フレ状態

    /* ========= サーバー側管理 ========= */
    [SerializeField, ShowInInspector]private readonly HashSet<ulong> viewers = new();  // 今見ているクライアントID

    
    [SerializeField, ShowInInspector]private bool isVisible;                           // 1人でも見ていれば true
    private float defaultNavSpeed;
    private PlayerStatus playerStatus;
    /* ========= Unity LifeCycle ========= */
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // 共通初期化
            enemy_status = GetComponent<EnemyStatus>();
            follow_object_by_nav = GetComponent<FollowObjectByNav>();
            discovery_player_controller = GetComponent<DiscoveryPlayerController>();
            enemy_to_player_kill_controller = GetComponent<EnemyToPlayerKillController>();
            enemy_flinch_controller = GetComponent<EnemyFlinchController>();
            character_controller = GetComponent<CharacterController>();
            nav_mesh_agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            defaultNavSpeed = nav_mesh_agent.speed;
        }
        playerStatus = PlayerStatus.LocalInstance;

        // クライアント (ホスト含む) だけカメラ取得
        cam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        Debug.Log("MainCamera: " + GameObject.FindWithTag("MainCamera"));
    }

    void LateUpdate()   // カメラが動いた後に判定
    {
        /* ---------- クライアント側: 可視判定してサーバーへ ---------- */
        Debug.Log("IsClient: " + IsClient);
        if (cam == null) { Debug.Log("Camera == null"); }          // 途中でカメラが切り替わる対策
        if (cam != null)
        {
            Debug.Log("IsClient And Cam != NULL");
            bool nowVisible = IsVisibleFromCamera();
            if (nowVisible != lastVisible)
            {
                SetVisibleStateServerRpc(nowVisible);
            }
            lastVisible = nowVisible;
        }

        /* ---------- サーバー側: 可視状態に応じて挙動更新 ---------- */
        if (IsServer && enemy_flinch_controller != null && !enemy_flinch_controller.FlinchNow)
        {
            // 必要なら毎フレ Apply しても軽いのでここで
            ApplyVisibleState();
        }
    }

    /* =========================================================================
       判定処理  (クライアントのみ呼ばれる)
       =========================================================================*/
    private bool IsVisibleFromCamera()
    {
        Vector3 viewPos = cam.WorldToViewportPoint(transform.position);

        // 画面外 or 背面
        if (viewPos.z < 0f ||
            viewPos.x < 0f || viewPos.x > 1f ||
            viewPos.y < -0.8f || viewPos.y > 0.95f)
            return false;

        // 遮蔽物チェック
        Vector3 dir = transform.position - cam.transform.position;
        float   dist = dir.magnitude;

        if (Physics.Raycast(cam.transform.position, dir.normalized, out RaycastHit hit, dist, obstacleMask))
        {
            // 自分以外に当たったら見えてない
            if (hit.collider.gameObject != gameObject) return false;
        }
        return true;
    }

    /* =========================================================================
       サーバー RPC  (クライアント→サーバー, 所有権不要)
       =========================================================================*/
    [ServerRpc(RequireOwnership = false)]
    private void SetVisibleStateServerRpc(bool visible, ServerRpcParams rpc = default)
    {
        ulong sender = rpc.Receive.SenderClientId;

        if (visible) viewers.Add(sender);
        else         viewers.Remove(sender);

        bool newVisible = viewers.Count > 0;
        if (newVisible != isVisible)
        {
            isVisible = newVisible;
            ApplyVisibleState();      // 状態変化時は即反映
        }
    }

    /* =========================================================================
       サーバー側: 敵の挙動を切り替え
       =========================================================================*/
    private void ApplyVisibleState()
    {
        if (enemy_flinch_controller != null && enemy_flinch_controller.FlinchNow) return;

        if (isVisible)
        {
            nav_mesh_agent.isStopped = true;
            nav_mesh_agent.speed     = 0;
            animator.speed           = 0;
            enemy_to_player_kill_controller.enabled = false;
            follow_object_by_nav.MovePermit         = false;
        }
        else
        {
            nav_mesh_agent.isStopped = false;
            nav_mesh_agent.speed     = defaultNavSpeed;
            animator.speed           = 1;
            enemy_to_player_kill_controller.enabled = true;
            follow_object_by_nav.MovePermit         = true;
        }
    }
}
