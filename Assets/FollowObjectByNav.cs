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
    private Rigidbody rigid;
    private float ToTargetPosDistance;
    // Start is called before the first frame update
    void Start()
    {
        nav_mesh_agent = this.gameObject.GetComponent<NavMeshAgent>();
        rigid = this.gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        ToTargetPosDistance = Vector3.Distance(ChaseTargetPosition, this.transform.position);
        if (ChaseToTargetMode)
        {
            nav_mesh_agent.isStopped = false;
            nav_mesh_agent.SetDestination(ChaseTargetPosition);
            if(ToTargetPosDistance <= 0.5f)
            {
                //ターゲットの位置に到着したらストップする
                ChaseToTargetMode = false;
            }
        }
        else
        {
            rigid.velocity = Vector3.zero;
            nav_mesh_agent.isStopped = true;
        }
    }
    public void ChaseToTargetSet(bool _mode, Vector3 _position, float _speed)
    {
        ChaseToTargetMode = _mode;
        nav_mesh_agent.speed = _speed;
        ChaseTargetPosition = _position;
    }
}
