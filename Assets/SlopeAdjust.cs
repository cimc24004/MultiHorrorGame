using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlopeAdjust : MonoBehaviour
{
    //ƒXƒ[ƒv
    public float slopeForceRayLength;
    public float Angle;
    private RaycastHit hit;

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1f))
        {
            Angle = Vector3.Angle(hit.normal, Vector3.up);
            return Angle > 5;
        }
        return false;
    }
    public Vector3 GetSlopeMoveDirection(Vector3 MoveDirection)
    {
        return Vector3.ProjectOnPlane(MoveDirection, hit.normal).normalized;
    }
}
