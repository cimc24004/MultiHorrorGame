using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public float Distance = 0.1f;
    private RaycastHit raycast_hit;
    public CustomGravity custom_gravity;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Physics.Raycast(this.transform.position, Vector3.down,out raycast_hit, Distance))
        {
            custom_gravity.enabled = false;
        }
        else
        {
            custom_gravity.enabled = true;
        }
    }
}
