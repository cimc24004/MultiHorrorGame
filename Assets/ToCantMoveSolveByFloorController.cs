using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToCantMoveSolveByFloorController : MonoBehaviour
{
    public GroundCheck ground_check;
    public Transform Parent;
    public LayerMask test_mask;
    private void Update()
    {
        if(ground_check.GroundedTagName == "Can'tMove")
        {
            //ground_check.hitted_obj_collider.excludeLayers = ground_check.default_hitted_obj_exclude_layers;
            test_mask = LayerMaskLayerAdded();
        }
    }

    private LayerMask LayerMaskLayerAdded()
    {

        return (ground_check.default_hitted_obj_exclude_layers + Parent.transform.gameObject.layer);
    }
}
