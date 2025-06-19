using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using System;
using DG.Tweening;
using UnityEngine.UIElements;
using Unity.Netcode;
public class FollowObjectByNav : NetworkBehaviour
{
    //ただ追いかける為だけのクラス ここに直接イベントは書かないこと。
    private NavMeshAgent nav_mesh_agent;
    public bool ChaseToTargetMode;
    public float speed;
    //ターゲットの位置と自分との距離がStoppingDistance以内であれば停止する。
    public float StoppingDistance = 0.5f;
    //追いかけるtargetに到着したかどうか
    public bool ArrivedTarget;
    //private Rigidbody rigid;
    //[HideInInspector]
    public double ToTargetPosDistance;
    //[HideInInspector]
    public Vector3 ChaseTargetPosition;

    [SerializeField]public bool MovePermit = true;
    [HideInInspector]
    public float DefaultStoppingDistance;
    private Vector3 NavmeshNextPos;
    private TimeManager time_manager;
    private CharacterController character_controller;
    private Vector3 Vector3Move;
    private NavMeshHit nav_mesh_hit;
    private EnemyStatus enemy_status;
    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        nav_mesh_agent = this.gameObject.GetComponent<NavMeshAgent>();
        //rigid = this.gameObject.GetComponent<Rigidbody>();
        character_controller = this.gameObject.GetComponent<CharacterController>();
        time_manager = TimeManager.instance;
        enemy_status = this.gameObject.GetComponent<EnemyStatus>();
        this.DefaultStoppingDistance = StoppingDistance;
        //デフォルトでは探索結果の0番目を指定する
        if (nav_mesh_agent.path.corners.Length >= 1)
        {
            NavmeshNextPos = nav_mesh_agent.path.corners[0];
        }
        //navmeshagentによる動きを停止。 動きはCharacterControllerで行いnavmeshagentは探索結果のみを用いる。
        nav_mesh_agent.updatePosition = true;
        nav_mesh_agent.updateRotation = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;
        if (!MovePermit)
        {
            return;
        }
        //追いかけるターゲットと自分との距離
        ToTargetPosDistance = Math.Round(Vector3.Distance(ChaseTargetPosition, this.transform.position), 1, MidpointRounding.ToEven);
        PosGetByFinder(ChaseTargetPosition);
        nav_mesh_agent?.SetDestination(ChaseTargetPosition);
        if (ChaseToTargetMode)
        {

            //Debug.Log(Math.Round(ToTargetPosDistance, 1));
            if (!(nav_mesh_agent.path.corners.Length > 2) && ToTargetPosDistance <= StoppingDistance)
            {
                //もしターゲットまでの最終ルートでかつ、到着したらArrivedTarget=Trueにする。
                Debug.Log("!nav_mesh_agent.Length > 2 && ToTargetPosDIstance <= StoppingDIsntance");
                ArrivedTarget = true;
            }
            else
            {
                ArrivedTarget = false;

            }

            if (ArrivedTarget)
            {
                //ターゲットの位置に到着したらChaseToTargetModeはFalseにしてリセット
                Vector3Move = Vector3.down * character_controller.velocity.y;
                ChaseToTargetMode = false;
                enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Stop;
            }
            else
            {
                //ターゲットの位置に到着していない場合は
                Vector3 target = (NavmeshNextPos - this.transform.position).normalized;
                Vector3Move = target * speed;
            }


        }
        nav_mesh_agent.velocity = character_controller.velocity;
        character_controller.Move(new Vector3(Vector3Move.x * Time.deltaTime, 0, Vector3Move.z * Time.deltaTime));
    }
    private void FixedUpdate()
    {
        if (!IsServer) return;
        if (!MovePermit) return;
        if (ChaseToTargetMode)
        {

            //追いかけるモードがTrueならば中継地点との差を求め、PosGetByFinder関数を実行。
            Vector3 direction = (nav_mesh_agent.steeringTarget - this.transform.position).normalized;
            direction = new Vector3(direction.x, 0, direction.z);
            //Debug.Log("direction: " + direction);

            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 0.1f);
            }
        }

    }
    public void ChaseToTargetSet(bool _mode, Vector3 _position, float _speed)
    {
        if (!IsServer)
        {
            Debug.LogError("この関数はサーバーからでしか実行できませんよ！");
        }
        //この関数は外部から使用する。 targetの位置をセットする為の関数 
        //_SetDestinationOnceとは SetDestinationを何回もやってると重くなるのでプレイヤーなど動くもの以外は
        //一回だけ探索ゴールを設定するようにしておく。 軽量化のため。
        this.ArrivedTarget = false;
        this.ChaseToTargetMode = _mode;
        this.speed = _speed;
        this.ChaseTargetPosition = _position;
        this.ToTargetPosDistance = Math.Round(Vector3.Distance(ChaseTargetPosition, this.transform.position), 1, MidpointRounding.ToEven);
    }
    public void PosGetByFinder(Vector3 target_pos)
    {
        //探索結果を取得するコード
        if (nav_mesh_agent.path.corners.Length > 2)
        {
            NavmeshNextPos = nav_mesh_agent.path.corners[1];
        }
        else
        {
            NavmeshNextPos = target_pos;
        }


        nav_mesh_agent.nextPosition = this.transform.position;

    }

    public bool CanMoveCheck(Vector3 targetPos)
    {
        if (!IsServer)
        {
            Debug.LogError("この関数はサーバーからでしか実行できませんよ！");
        }
        NavMeshPath checkPath = new NavMeshPath();
        nav_mesh_agent.CalculatePath(targetPos, checkPath);

        // パスが完全でないなら「青いエリア外 or パスを引けない」
        if (checkPath.status != NavMeshPathStatus.PathComplete)
        {
            return false;
        }
        return true;
    }
    public Vector3 BlueRangeNearPositionGet(Vector3 targetPos, float maxDistance)
    {
        if (!IsServer)
        {
            Debug.LogError("この関数はサーバーからでしか実行できませんよ！");
        }
        //青の範囲内の中で近いものをゲット
        NavMeshPath path = new NavMeshPath();
        bool found = NavMesh.SamplePosition(targetPos, out nav_mesh_hit, maxDistance, NavMesh.AllAreas);
        return nav_mesh_hit.position;
    }
    private void OnDrawGizmos()
    {
        if (!ChaseToTargetMode) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(nav_mesh_hit.position, 1);


        Vector3 prepos = ChaseTargetPosition;
        Gizmos.color = Color.green;
        foreach (Vector3 pos in nav_mesh_agent.path.corners)
        {
            Gizmos.DrawCube(pos, new Vector3(1, 1, 0));
            prepos = pos;
        }
        Gizmos.color = Color.red;
        Gizmos.DrawCube(nav_mesh_agent.nextPosition, new Vector3(1, 1, 1));
    }


}
