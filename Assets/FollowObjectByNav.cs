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

        if (nav_mesh_agent.path.corners.Length >= 1)
        {
            NavmeshNextPos = nav_mesh_agent.path.corners[0];
        }
        nav_mesh_agent.updatePosition = false;
        nav_mesh_agent.updateRotation = false;
    }

    // Update is called once per frame
    void Update()
    {
        ToTargetPosDistance = Vector3.Distance(ChaseTargetPosition, this.transform.position);
        Vector3 forward = (nav_mesh_agent.steeringTarget - this.transform.position).normalized;
        this.transform.forward = new Vector3(forward.x, 0, forward.z);
        if (ChaseToTargetMode)
        {
            nav_mesh_agent.SetDestination(ChaseTargetPosition);
            ToNextPointPosDistance = Vector3.Distance(NavmeshNextPos, this.transform.position);
            PosGetByFinder(ChaseTargetPosition);

            if (nav_mesh_agent.path.corners.Length == 2 && ToTargetPosDistance <= 2)
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
        }
    }
    private void FixedUpdate()
    {
        if(ArrivedTarget)
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
            Debug.Log("TargetPossssssssssssssssssssssssssssssssssssssssssssss: " + nav_mesh_agent.path.corners.Length);
            NavmeshNextPos = target_pos;
        }
        if (ToNextPointPosDistance <= 0f)
        {
            Debug.Log("NextPosに到着！");
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
