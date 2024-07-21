using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public bool OnGround;
    public float Distance = 0.03f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if(Physics.Raycast(this.transform.position, Vector3.down, Distance))
        {
            OnGround = true;
        }
        else
        {
            OnGround = false;
        }
        Debug.DrawRay(this.transform.position, Vector3.down *Distance, Color.yellow);
    }
}
