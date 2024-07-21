using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlopeAdjust : MonoBehaviour
{
    public float slopeForceRayLength;
    private Rigidbody rb;
    public float slopeForce;
    public float Angle;
    RaycastHit hit;
    private PlayerStatus player_status;
    private PlayerMoveScript player_move_script;
    private CustomGravity custom_gravity;
    private void Start()
    {
        rb = this.gameObject.GetComponent<Rigidbody>();
        player_status = this.gameObject.GetComponent<PlayerStatus>();
        custom_gravity = this.gameObject.GetComponent<CustomGravity>();
        player_move_script = this.gameObject.GetComponent<PlayerMoveScript>();
    }
    void FixedUpdate()
    {

        // â“¹‚Å‚Ì’Ç‰Á—Í
        if (OnSlope())
        {
            rb.AddForce(GetSlopeMoveDirection() * player_status.MoveSpeed * 20, ForceMode.Acceleration);
            if(rb.velocity.y > 0)
            {
                Debug.Log("rb.velocity.y > 0");
                rb.AddForce(Vector3.down * 80f, ForceMode.Acceleration);
            }
            rb.AddForce(-hit.normal * 100f, ForceMode.Acceleration);
        }
        Debug.DrawRay(transform.position, Vector3.down * 0.2f, Color.yellow);
        custom_gravity.enabled = false;
    }

    bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 0.2f))
        {
            Angle = Vector3.Angle(hit.normal, Vector3.up);
            return Angle > 5;
        }
        return false;
    }
    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(player_move_script.MoveDirection, hit.normal).normalized;
    }
}
