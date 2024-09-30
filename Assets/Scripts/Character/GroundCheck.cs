using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    private RaycastHit raycast_hit;
    public float Distance;
    public bool OnGround;

    public string GroundedTagName;
    public LayerMask GroundLayerMask;

    public GameObject HittedObj;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Physics.Raycast(this.transform.position, Vector3.down, out raycast_hit, Distance, GroundLayerMask))
        {
            OnGround = true;
            GroundedTagName = raycast_hit.collider.gameObject.tag;
        }
        else
        {
            OnGround = false;
            GroundedTagName = null;
        }
    }
}
