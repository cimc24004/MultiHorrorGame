using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
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
    public float ToTargetPosDistance;
    [HideInInspector]
    public Vector3 ChaseTargetPosition;
    private float ToNextPointPosDistance;
    [HideInInspector]
    public float DefaultStoppingDistance;
    private Vector3 NavmeshNextPos;
    private CharacterController character_controller;
    // Start is called before the first frame update
    void Start()
    {
        nav_mesh_agent = this.gameObject.GetComponent<NavMeshAgent>();
        //rigid = this.gameObject.GetComponent<Rigidbody>();
        character_controller = this.gameObject.GetComponent<CharacterController>();
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
        ToTargetPosDistance = Vector3.Distance(ChaseTargetPosition, this.transform.position);


        if (ChaseToTargetMode)
        {
            //追いかけるモードがTrueならば中継地点との差を求め、PosGetByFinder関数を実行。
            ToNextPointPosDistance = Vector3.Distance(NavmeshNextPos, this.transform.position);
            PosGetByFinder(ChaseTargetPosition);
            if (!(nav_mesh_agent.path.corners.Length > 2) && ToTargetPosDistance <= StoppingDistance)
            {
                //もしターゲットまでの最終ルートでかつ、到着したらArrivedTarget=Trueにする。
                ArrivedTarget = true;

            }
            else
            {
                ArrivedTarget = false;

            }

            if (ArrivedTarget)
            {
                //ターゲットの位置に到着したらChaseToTargetModeはFalseにしてリセット
                ChaseToTargetMode = false;
            }
        }
        else
        {
            //ArrivedTarget = false;

        }
    }
    private void FixedUpdate()
    {

        if (ChaseToTargetMode)
        {

            // 回転する方向を計算
            Vector3 direction = (nav_mesh_agent.steeringTarget - this.transform.position).normalized;
            Debug.Log(nav_mesh_agent.steeringTarget);
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 0.1f);
            }

            nav_mesh_agent.SetDestination(ChaseTargetPosition);
            if (ArrivedTarget)
            {
                character_controller.Move(new Vector3(0, character_controller.velocity.y * Time.deltaTime, 0));
            }

        }
        else
        {
        }

        if(!ArrivedTarget)
        {
            //ターゲットの位置に到着していない場合は
            if (ChaseToTargetMode && ToNextPointPosDistance > 0f)
            {
                Vector3 target = (NavmeshNextPos - this.transform.position).normalized;
                //rigid.velocity = target * speed;
                character_controller.Move(target * speed * Time.deltaTime);
            }
            else
            {
                //rigid.velocity = Vector3.zero;
                //character_controller.Move(new Vector3(0, character_controller.velocity.y * Time.deltaTime, 0));
            }
        }

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
        if (ToNextPointPosDistance <= 0f)
        {
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
