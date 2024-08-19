using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionChaseToTarget : MonoBehaviour
{
    public Transform ChaseTarget;
    private void Update()
    {
        this.transform.position = ChaseTarget.position;
    }
}
