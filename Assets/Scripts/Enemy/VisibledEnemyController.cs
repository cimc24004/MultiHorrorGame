using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class VisibledEnemyController : MonoBehaviour
{
    private FollowObjectByNav follow_object_by_nav;
    private DiscoveryPlayerController discovery_player_controller;
    private EnemyToPlayerKillController enemy_to_player_kill_controller;
    private CharacterController character_controller;
    private PlayerStatus player_status;
    private Camera PlayerCamera;
    private Vector3 PlayerHeadPos;
    private EnemyStatus enemy_status;
    private Transform EyeTransform;
    private Animator m_Animator;
    private RaycastHit hit;
    private EnemyFlinchController enemy_flinch_controller;
    private NavMeshAgent nav_mesh_agent;
    private float NavMeshSpeed;
    private bool isVisible;

    private Vector3 Move = new Vector3(0, 1, 0);
    // Start is called before the first frame update
    void Start()
    {
        player_status = PlayerStatus.instance;
        enemy_status = this.gameObject.GetComponent<EnemyStatus>();
        m_Animator = this.gameObject.GetComponent<Animator>();
        PlayerHeadPos = player_status.HeadPos.position;
        EyeTransform = enemy_status.EyePosition;
        PlayerCamera = Camera.main;
        follow_object_by_nav = this.gameObject.GetComponent<FollowObjectByNav>();
        character_controller = this.gameObject.GetComponent<CharacterController>();
        enemy_to_player_kill_controller = this.gameObject.GetComponent<EnemyToPlayerKillController>();
        character_controller = this.gameObject.GetComponent<CharacterController>();
        discovery_player_controller = this.gameObject.GetComponent<DiscoveryPlayerController>();
        enemy_flinch_controller = this.gameObject.GetComponent<EnemyFlinchController>();
        nav_mesh_agent = this.gameObject.GetComponent<NavMeshAgent>();
        NavMeshSpeed = this.nav_mesh_agent.speed;
    }

    [Header("遮蔽物として判定するレイヤー")]
    [SerializeField] private LayerMask obstacleMask;

    void Update()
    {
        if (IsVisibleFromPlayerCamera())
        {
            // カメラから見えているので移動を停止
            isVisible = true;

        }
        else
        {
            // 見えていないので移動を再開
            isVisible = false;

        }
        if(enemy_flinch_controller.FlinchNow){
            m_Animator.speed = 1;
            return;
        }
        if (isVisible)
        {
            
            follow_object_by_nav.MovePermit = false;
            character_controller.Move(Move * character_controller.velocity.y);
            enemy_to_player_kill_controller.enabled = false;
            m_Animator.speed = 0;
            nav_mesh_agent.speed = 0;
        }
        else
        {
            enemy_to_player_kill_controller.enabled = true;
            m_Animator.speed = 1;
            nav_mesh_agent.speed = NavMeshSpeed;
            follow_object_by_nav.MovePermit = true;
        }
    }

    /// <summary>
    /// プレイヤーのカメラから自分（敵）が視認できるかどうかを判定
    /// </summary>
    /// <returns>見えている場合true、見えていない場合false</returns>
    private bool IsVisibleFromPlayerCamera()
    {
        // カメラが設定されていなければ判定不能
        if (PlayerCamera == null) return false;

        // 1. カメラの視野フラスタム内にいるかどうか
        Vector3 viewPos = PlayerCamera.WorldToViewportPoint(transform.position);
        // カメラの後方(z<0)にいる or 画面外(x,yが[0,1]範囲外)なら不可視と判断
        if (viewPos.z < 0f) return false;
        if (viewPos.x < 0f || viewPos.x > 1f || viewPos.y < -0.8f || viewPos.y > 0.95f)
        {
            return false;
        }

        // 2. レイキャストで遮蔽物の有無をチェック
        Vector3 direction = PlayerCamera.transform.position - EyeTransform.position;
        Debug.DrawRay(EyeTransform.position, direction * 10000, Color.red);
        // カメラ位置から敵(自分)に向けてRayを飛ばし、ヒット情報を取得
        if (Physics.Raycast(EyeTransform.position, direction, out RaycastHit hit, Mathf.Infinity, obstacleMask))
        {
            // Raycast飛ばしてplayerであればtrueを返す それ以外ならfalseを変えす
            Debug.Log("hited----!?!?: " + hit.collider.gameObject);
            if (hit.collider.gameObject.tag == "Player")
            {
                return true;
            }
        }

        // フラスタム内かつ遮蔽物なし → 見えていると判断
        return false;
    }

}
