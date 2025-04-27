using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Sirenix.OdinInspector;
public class EnemyActionForDoorController : SerializedMonoBehaviour
{
    //敵がドアを開けたり、壊したりするコントローラー
    
    [SerializeField] private float RaycastDistance = 1f;
    [SerializeField] private int BrokeDoorPlusCount = 1;
    public DoorOpenCloseController door_open_close_controller;
    public TheDoorController the_door_controller;
    public DoorAttackedController door_attacked_controller;
    public List<Transform> rayStarts;
    public RaycastHit raycast_hit;
    public GameObject raycast_hit_obj;
    public LayerMask layer_mask;

    public bool DoorAttackNow;
    public AudioSource DoorAttackSource;
    public AudioClip DoorAttackClip;
    public AudioClip DoorBrokenClip;
    public bool AutoDoorAttack = true;
    public bool DoorBroken = false;
    private FollowObjectByNav follow_object_by_nav;
    private EnemyStatus enemy_status;
    private NavMeshAgent nav_mesh_agent;
    private Animator anim;
    private DiscoveryPlayerController discovery_player_controller;
    private EnemySoundSensor enemy_sound_sensor;
    private CharacterController character_controller;

    [SerializeField] private GameObject AttackedDoorEffectPrefab;
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
        if (!AutoDoorAttack) return;

        anim.SetBool("DoorAttack", false);
        DoorAttackNow = false;


        foreach (Transform start in rayStarts)
        {
            if (start == null) continue;                     // 念のため空チェック
            Debug.DrawRay(start.position, start.forward * RaycastDistance, Color.red);
            
            if (Physics.Raycast(start.position, start.forward, out raycast_hit, RaycastDistance, layer_mask))
            {
                if (raycast_hit.collider.gameObject.tag == "Door")
                {
                    // ── ここから先は元の一連の処理そのまま ─────────
                    Debug.Log("DoorHit: " + raycast_hit.collider.gameObject);

                    if (raycast_hit_obj != raycast_hit.collider.gameObject || door_open_close_controller == null)
                    {
                        door_open_close_controller = raycast_hit.collider.gameObject.GetComponent<DoorOpenCloseController>();
                    }
                    raycast_hit_obj = raycast_hit.collider.gameObject;

                    the_door_controller = door_open_close_controller.the_door_controller;

                    if ((the_door_controller.OpenedClosedCompleted && !the_door_controller.NeededKey) || !discovery_player_controller.DiscoveryPlayerNow)
                    {
                        if (!discovery_player_controller.DiscoveryPlayerNow)
                        {
                            the_door_controller.DoorOpenClosedChangeByStatus(this.gameObject);
                        }
                    }

                    if (the_door_controller.NeededKey || discovery_player_controller.DiscoveryPlayerNow)
                    {
                        Debug.Log("扉を壊したいと思います。");
                        discovery_player_controller.enabled = false;
                        follow_object_by_nav.ChaseToTargetSet(true, the_door_controller.gameObject.transform.position, 1);
                        follow_object_by_nav.StoppingDistance = 2;
                        character_controller.Move(Vector3.zero);

                        DoorAttackNow = true;
                        anim.SetBool("DoorAttack", true);
                        enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Stop;
                        discovery_player_controller.enabled = true;
                    }
                    else
                    {
                        DoorAttackNow = false;
                    }

                    // どれか 1 本当たったら break でループ終了
                    break;
                }
            }
        }
    }
    public void AttackToDoor()
    {
        
        if(the_door_controller != null){
            door_attacked_controller = the_door_controller.door_attacked_controller;
        }
        if(door_attacked_controller == null)
        {
            return;
        }
        DoorBroken = this.door_attacked_controller.DoorAttacked(BrokeDoorPlusCount);
        follow_object_by_nav.StoppingDistance = follow_object_by_nav.DefaultStoppingDistance;
        if (DoorBroken)
        {
            follow_object_by_nav.StoppingDistance = follow_object_by_nav.DefaultStoppingDistance;
            follow_object_by_nav.MovePermit = true;
            DoorAttackSource.PlayOneShot(DoorBrokenClip);
            DoorAttackSource.PlayOneShot(DoorAttackClip);
        }
        else
        {
            // GameObject effect = Instantiate(AttackedDoorEffectPrefab, the_door_controller.gameObject.transform.position + (Vector3.up * 0.2f), Quaternion.identity);
            // effect.transform.forward = this.transform.forward;
            DoorAttackSource.PlayOneShot(DoorAttackClip);
        }
    }
}
