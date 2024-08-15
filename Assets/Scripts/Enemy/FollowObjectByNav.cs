using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using System;
public class FollowObjectByNav : MonoBehaviour
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
    private Vector3 Move;
    [HideInInspector]
    public float DefaultStoppingDistance;
    private Vector3 NavmeshNextPos;
    private TimeManager time_manager;
    private CharacterController character_controller;
    private Vector3 Vector3Move;
    private CustomGravity custom_gravity;
    // Start is called before the first frame update
    void Start()
    {
        nav_mesh_agent = this.gameObject.GetComponent<NavMeshAgent>();
        //rigid = this.gameObject.GetComponent<Rigidbody>();
        character_controller = this.gameObject.GetComponent<CharacterController>();
        time_manager = TimeManager.instance;
        custom_gravity = this.gameObject.GetComponent<CustomGravity>();
        this.DefaultStoppingDistance = StoppingDistance;
        //デフォルトでは探索結果の0番目を指定する
        if (nav_mesh_agent.path.corners.Length >= 1)
        {
            NavmeshNextPos = nav_mesh_agent.path.corners[0];
        }
        //navmeshagentによる動きを停止。 動きはrigidbodyで行いnavmeshagentは探索結果のみを用いる。
        nav_mesh_agent.updatePosition = false;
        nav_mesh_agent.updateRotation = false;
    }

    // Update is called once per frame
    void Update()
    {

        //追いかけるターゲットと自分との距離
        ToTargetPosDistance = Math.Round(Vector3.Distance(ChaseTargetPosition, this.transform.position), 1, MidpointRounding.ToEven);
        PosGetByFinder(ChaseTargetPosition);
        nav_mesh_agent.SetDestination(ChaseTargetPosition);

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
            }
            else
            {
                //ターゲットの位置に到着していない場合は
                Vector3 target = (NavmeshNextPos - this.transform.position).normalized;
                Vector3Move = target * speed;
            }



        }


    }
    private void FixedUpdate()
    {
        if (ChaseToTargetMode)
        {

            //追いかけるモードがTrueならば中継地点との差を求め、PosGetByFinder関数を実行。
            //nav_mesh_agent.velocity = character_controller.velocity;
            Vector3 direction = (nav_mesh_agent.steeringTarget - this.transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 0.1f);
            }
        }
        character_controller.Move(Vector3Move * Time.fixedDeltaTime);


    }
    public void ChaseToTargetSet(bool _mode, Vector3 _position, float _speed)
    {
        //この関数は外部から使用する。 targetの位置をセットする為の関数 
        //_SetDestinationOnceとは SetDestinationを何回もやってると重くなるのでプレイヤーなど動くもの以外は
        //一回だけ探索ゴールを設定するようにしておく。 軽量化のため。
        this.ChaseToTargetMode = _mode;
        this.speed = _speed;
        this.ChaseTargetPosition = _position;
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

    private void OnDrawGizmos()
    {
        if (!ChaseToTargetMode) return;
        Vector3 prepos = ChaseTargetPosition;
        Gizmos.color = Color.green;
        foreach (Vector3 pos in nav_mesh_agent.path.corners)
        {
            Gizmos.DrawCube(pos, new Vector3(1,1,0));
            prepos = pos;
        }
        Gizmos.color = Color.red;
        Gizmos.DrawCube(nav_mesh_agent.nextPosition, new Vector3(1,1,1));
    }
}
