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
    public LayerMask layer_mask;


    public AudioSource DoorAttackSource;
    public AudioClip DoorAttackClip;
    public AudioClip DoorBrokenClip;

    private FollowObjectByNav follow_object_by_nav;
    private EnemyStatus enemy_status;
    private NavMeshAgent nav_mesh_agent;
    private Animator anim;
    private DiscoveryPlayerController discovery_player_controller;
    private EnemySoundSensor enemy_sound_sensor;
    private CharacterController character_controller;
    // Start is called before the first frame update
    void Start()
    {
        enemy_status = this.gameObject.GetComponent<EnemyStatus>();
        follow_object_by_nav = this.gameObject.GetComponent<FollowObjectByNav>();
        nav_mesh_agent = this.gameObject.GetComponent<NavMeshAgent>();
        discovery_player_controller = this.gameObject.GetComponent<DiscoveryPlayerController>();
        anim = this.gameObject.GetComponent<Animator>();
        enemy_sound_sensor = this.gameObject.GetComponent<EnemySoundSensor>();
        character_controller = this.gameObject.GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetBool("DoorAttack", false);
        //レイキャストを正面から当てて、ドアに当たったら開けるようにする。
        if (Physics.Raycast(RaycastStartTransform.position, RaycastStartTransform.transform.forward, out raycast_hit, 1f, layer_mask))
        {

            if (raycast_hit.collider.gameObject.tag == "Door")
            {
                Debug.Log("DoorHit: " + raycast_hit.collider.gameObject);
                if(raycast_hit_obj != raycast_hit.collider.gameObject || door_open_close_controller == null)
                {
                    door_open_close_controller = raycast_hit.collider.gameObject.GetComponent<DoorOpenCloseController>();
                }
                raycast_hit_obj = raycast_hit.collider.gameObject;

                the_door_controller = door_open_close_controller.the_door_controller;
                if ((the_door_controller.OpenedClosedCompleted && !the_door_controller.NeededKey) || !discovery_player_controller.DiscoveryPlayerNow)
                {
                    //ドアの開け閉めアニメーションが終わっていた場合だけ開け閉めできるようにするよ。
                    the_door_controller.DoorOpenClosedChangeByStatus(this.gameObject);
                }
                if(the_door_controller.NeededKey || discovery_player_controller.DiscoveryPlayerNow)
                {
                    Debug.Log("扉を壊したいと思います。");
                    follow_object_by_nav.ChaseToTargetSet(true,the_door_controller.gameObject.transform.position,3);
                    follow_object_by_nav.StoppingDistance = 2;
                    character_controller.Move(Vector3.zero);
                    anim.SetBool("DoorAttack", true);
                    enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Stop;
                }

            }
            else
            {

            }
        }
        else
        {

        }
        Debug.DrawRay(RaycastStartTransform.position, RaycastStartTransform.transform.forward * 1f, Color.red);
    }
    public void AttackToDoor()
    {
        DoorAttackedController door_attacked_controller = the_door_controller.door_attacked_controller;
        if(the_door_controller == null || door_attacked_controller == null)
        {
            return;
        }
        bool broken = the_door_controller.door_attacked_controller.DoorAttacked();
        if (broken)
        {
            follow_object_by_nav.StoppingDistance = follow_object_by_nav.DefaultStoppingDistance;
            follow_object_by_nav.MovePermit = true;
            DoorAttackSource.PlayOneShot(DoorBrokenClip);
            DoorAttackSource.PlayOneShot(DoorAttackClip);
        }
        else
        {
            DoorAttackSource.PlayOneShot(DoorAttackClip);
        }
    }
}
