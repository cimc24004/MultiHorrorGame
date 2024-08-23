using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Sirenix.OdinInspector;
public class EnemyActionForDoorController : SerializedMonoBehaviour
{
    //ドアを開けたり、壊したりするコントローラー
    public DoorOpenCloseController door_open_close_controller;
    public TheDoorController the_door_controller;
    public Transform RaycastStartTransform;
    public RaycastHit raycast_hit;
    public GameObject raycast_hit_obj;

    private FollowObjectByNav follow_object_by_nav;
    private EnemyStatus enemy_status;
    private NavMeshAgent nav_mesh_agent;
    // Start is called before the first frame update
    void Start()
    {
        enemy_status = this.gameObject.GetComponent<EnemyStatus>();
        follow_object_by_nav = this.gameObject.GetComponent<FollowObjectByNav>();
        nav_mesh_agent = this.gameObject.GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(RaycastStartTransform.position, RaycastStartTransform.transform.forward * 1, Color.gray);
        if(Physics.Raycast(RaycastStartTransform.position, RaycastStartTransform.transform.forward, out raycast_hit, 0.3f))
        {
            if(raycast_hit.collider.gameObject.tag == "Door")
            {
                Debug.Log("DoorHit: " + raycast_hit.collider.gameObject);
                if(door_open_close_controller == null && the_door_controller == null || raycast_hit_obj != raycast_hit.collider.gameObject)
                {
                    door_open_close_controller = raycast_hit.collider.gameObject.GetComponent<DoorOpenCloseController>();
                    the_door_controller = door_open_close_controller.the_door_controller;
                    if (the_door_controller.OpenedClosedCompleted)
                    {
                        the_door_controller.DoorOpenClosedChangeByStatus(this.gameObject);
                    }

                }
            }
            else
            {
                //the_door_controller = null;
                //door_open_close_controller = null;
            }
        }
        else
        {
            //the_door_controller = null;
            //door_open_close_controller = null;
        }
        //if(the_door_controller != null)
        //{
        //    if (the_door_controller.OpenedClosedCompleted)
        //    {
        //        follow_object_by_nav.enabled = true;
        //        nav_mesh_agent.enabled = true;
        //        enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Walk;
        //        the_door_controller = null;
        //        door_open_close_controller = null;
        //    }
        //    else
        //    {
        //        //開けてる最中だったら
        //        follow_object_by_nav.enabled = false;
        //        nav_mesh_agent.enabled = false;
        //        enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Stop;
        //    }
        //}

    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    Debug.Log("OnCollisionEnter: " + other.gameObject);
    //    if(other.gameObject.tag == "Door")
    //    {
    //        door_open_close_controller = other.gameObject.GetComponent<DoorOpenCloseController>();
    //        the_door_controller = door_open_close_controller.the_door_controller;

    //    }
    //}
    //private void OnTriggerExit(Collider other)
    //{
    //    if(other.gameObject.tag == "Door")
    //    {
    //        door_open_close_controller = null;
    //        the_door_controller = null;
    //    }
    //}
}
