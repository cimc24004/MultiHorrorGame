using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEventObjectRaycast : MonoBehaviour
{
    //てきからレイキャストを飛ばし、当たった物でイベントを変えていく。 PlayerでいうCrossHairEvent.csのような奴。
    public float RaycastDistance = 1;
    public Transform RaycastStartPos;
    public LayerMask layer_mask;
    private RaycastHit raycast_hit;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Physics.Raycast(RaycastStartPos.position, RaycastStartPos.transform.forward, out raycast_hit, RaycastDistance, layer_mask))
        {
            if(raycast_hit.collider.gameObject.tag == "Door")
            {
                //ドアに触れた場合はドアを開けるようにする。
                DoorOpenCloseController door_open_close_controlelr = raycast_hit.collider.gameObject.GetComponent<DoorOpenCloseController>();
                door_open_close_controlelr.the_door_controller.DoorOpenClosedChangeByStatus();
            }
        }
    }
}
