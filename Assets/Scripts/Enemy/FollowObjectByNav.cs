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
    public Vector3 ChaseTargetPosition;
    public float speed;
    //追いかけるtargetに到着したかどうか
    public bool ArrivedTarget;
    private Rigidbody rigid;
    [HideInInspector]
    public float ToTargetPosDistance;
    private float ToNextPointPosDistance;
    
    private Vector3 NavmeshNextPos;
    
    // Start is called before the first frame update
    void Start()
    {
        nav_mesh_agent = this.gameObject.GetComponent<NavMeshAgent>();
        rigid = this.gameObject.GetComponent<Rigidbody>();

        //デフォルトでは探索結果の0番目を指定する
        if (nav_mesh_agent.path.corners.Length >= 1)
        {
            NavmeshNextPos = nav_mesh_agent.path.corners[0];
        }
        //navmeshagentによる動きを停止。 動きはrigidbodyで行いnavmeshagentは探索結果のみを用いる。
        nav_mesh_agent.updatePosition = false;
    }

    // Update is called once per frame
    void Update()
    {
        //追いかけるターゲットと自分との距離
        ToTargetPosDistance = Vector3.Distance(ChaseTargetPosition, this.transform.position);
        //steeringTargetは今向かっている中継地点のことでその中継地点と自分自身との差ベクトルを単位ベクトル化して向かっている場所へ向きを向ける。



        if (ChaseToTargetMode)
        {
            //追いかけるモードがTrueならば中継地点との差を求め、PosGetByFinder関数を実行。
            ToNextPointPosDistance = Vector3.Distance(NavmeshNextPos, this.transform.position);
            PosGetByFinder(ChaseTargetPosition);
            //nav_mesh_agent.updateRotation = true;

            if (nav_mesh_agent.path.corners.Length == 2 && ToTargetPosDistance <= 2)
                //もしターゲットまでの最終ルートでかつ、到着したらArrivedTarget=Trueにする。
                ArrivedTarget = true;
            else
                ArrivedTarget = false;

            if (ArrivedTarget)
            {
                //ターゲットの位置に到着したらChaseToTargetModeはFalseにしてリセット
                ChaseToTargetMode = false;
            }
        }
        else
        {
            //ArrivedTarget = false;
            //nav_mesh_agent.updateRotation = false;
        }
    }
    private void FixedUpdate()
    {
        Vector3 forward = (NavmeshNextPos - this.transform.position).normalized;
        this.transform.forward = new Vector3(forward.x, 0, forward.z);
        if (ChaseToTargetMode)
        {
            nav_mesh_agent.SetDestination(ChaseTargetPosition);
        }
        if (ArrivedTarget)
        {
            //ターゲットの位置に到着した。場合はストップする。 
            rigid.velocity = Vector3.zero;
        }
        else
        {
            //ターゲットの位置に到着していない場合は
            if (ChaseToTargetMode && ToNextPointPosDistance > 0f)
            {

                rigid.velocity = (NavmeshNextPos - this.transform.position).normalized * speed;
            }
            else
            {
                rigid.velocity = Vector3.zero;
            }
        }

    }
    public void ChaseToTargetSet(bool _mode, Vector3 _position, float _speed)
    {
        //この関数は外部から使用する。 targetの位置をセットする為の関数
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
