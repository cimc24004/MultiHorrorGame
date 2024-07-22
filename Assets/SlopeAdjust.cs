using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlopeAdjust : MonoBehaviour
{
    public float slopeForceRayLength;
    private Rigidbody rb;
    public float Angle;
    private RaycastHit hit;

    private void Start()
    {
        //rb = this.gameObject.GetComponent<Rigidbody>();
        //player_status = this.gameObject.GetComponent<PlayerStatus>();
        //custom_gravity = this.gameObject.GetComponent<CustomGravity>();
    }
    void FixedUpdate()
    {

    }

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
