using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public float Distance = 0.1f;
    private RaycastHit raycast_hit;
    public bool OnGround;
    public Transform RaycastStartTransform;

    public string GroundedTagName;
    public LayerMask GroundLayerMask;

    public Collider hitted_obj_collider;

    public LayerMask default_hitted_obj_exclude_layers;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("other.gameobject.lauerGround?: " + other.gameObject);
        if(other.gameObject.layer == GroundLayerMask)
        {
            OnGround = true;
            hitted_obj_collider = other.gameObject.GetComponent<Collider>();
            default_hitted_obj_exclude_layers = hitted_obj_collider.excludeLayers;
            GroundedTagName = other.gameObject.tag;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == GroundLayerMask)
        {
            OnGround = false;
            hitted_obj_collider = null;
            GroundedTagName = null;
        }
    }
}
