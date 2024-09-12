using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Sirenix.OdinInspector;
public class EnemyActionForDoorController : SerializedMonoBehaviour
{
    //敵がドアを開けたり、壊したりするコントローラー
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
        //レイキャストを正面から当てて、ドアに当たったら開けるようにする。
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
                        //ドアの開け閉めアニメーションが終わっていた場合だけ開け閉めできるようにするよ。
                        the_door_controller.DoorOpenClosedChangeByStatus(this.gameObject);
                    }

                }
            }
            else
            {

            }
        }
        else
        {

        }

    }

}
