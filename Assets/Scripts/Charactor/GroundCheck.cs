using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public float Distance = 0.1f;
    private RaycastHit raycast_hit;
    public CustomGravity custom_gravity;
    public bool OnGround;
    public SlopeAdjust slope_adjust;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //if(Physics.Raycast(this.transform.position, Vector3.down,out raycast_hit, Distance))
        //{
        //    custom_gravity.enabled = false;
        //}
        //else
        //{
        //    custom_gravity.enabled = true;
        //}

        if (!slope_adjust.OnSlope())
        {
            custom_gravity.enabled = !OnGround;
        }
        else
        {
            custom_gravity.enabled = false;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Object"))
        {
            OnGround = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Object"))
        {
            OnGround = false;
        }
    }
}
