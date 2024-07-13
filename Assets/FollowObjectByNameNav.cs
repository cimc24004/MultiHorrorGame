using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FollowObjectByNameNav : MonoBehaviour
{
    //ただ追いかける為だけのクラス ここに直接イベントは書かないこと。
    private NavMeshAgent nav_mesh_agent;
    public bool ChaseToTargetMode;
    public Vector3 ChaseTargetPosition;
    private Rigidbody rigid;
    private Vector3 DefaultPos;
    // Start is called before the first frame update
    void Start()
    {
        nav_mesh_agent = this.gameObject.GetComponent<NavMeshAgent>();
        rigid = this.gameObject.GetComponent<Rigidbody>();
        DefaultPos = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (ChaseToTargetMode)
        {
            nav_mesh_agent.isStopped = false;
            nav_mesh_agent.SetDestination(ChaseTargetPosition);
        }
        else
        {
            rigid.velocity = Vector3.zero;
            nav_mesh_agent.isStopped = true;
        }
    }
    public void ChaseToTargetSet(bool _mode, Vector3 _position)
    {
        ChaseToTargetMode = _mode;
        ChaseTargetPosition = _position;
    }
}
