using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Sirenix.OdinInspector;
using Unity.Netcode;
public class EnemyActionForDoorController : NetworkBehaviour
{
    //�G���h�A���J������A�󂵂��肷��R���g���[���[
    
    [SerializeField] private float RaycastDistance = 1f;
    [SerializeField] private int BrokeDoorPlusCount = 1;
    public DoorOpenCloseController door_open_close_controller;
    public TheDoorController the_door_controller;
    public DoorAttackedController door_attacked_controller;
    public List<Transform> rayStarts;
    public RaycastHit raycast_hit;
    public GameObject raycast_hit_obj;
    public LayerMask layer_mask;
    public bool DoorTouchNow = false;
    public bool DoorAttackNow;
    public bool DoorOpenClosePermit = true;
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
    private NetworkObject networkObject;
    private EnemySoundSensor enemy_sound_sensor;
    private CharacterController character_controller;

    [SerializeField] private GameObject AttackedDoorEffectPrefab;
    // Start is called before the first frame update
    void Start()
    {
        networkObject = this.gameObject.GetComponent<NetworkObject>();
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
        if(DoorTouchNow) DoorTouchNow = false;
        anim.SetBool("DoorAttack", false);
        DoorAttackNow = false;


        foreach (Transform start in rayStarts)
        {
            if (start == null) continue;                     // �O�̂��ߋ�`�F�b�N
            Debug.DrawRay(start.position, start.forward * RaycastDistance, Color.red);
            
            if (Physics.Raycast(start.position, start.forward, out raycast_hit, RaycastDistance, layer_mask))
            {
                Debug.Log("raycast_hit_collider.gameObject: " + raycast_hit.collider.gameObject);
                if (raycast_hit.collider.gameObject.tag == "Door")
                {
                    DoorTouchNow = true;
                    // ���� ���������͌��̈�A�̏������̂܂� ������������������
                    Debug.Log("DoorHit���܂����B�^�[�Q�b�g��: " + raycast_hit.collider.gameObject);

                    if (raycast_hit_obj != raycast_hit.collider.gameObject || door_open_close_controller == null)
                    {
                        door_open_close_controller = raycast_hit.collider.gameObject.GetComponent<DoorOpenCloseController>();
                    }
                    raycast_hit_obj = raycast_hit.collider.gameObject;
                    if(door_open_close_controller != null){
                        the_door_controller = door_open_close_controller.the_door_controller;
                    }
                    if(DoorOpenClosePermit){
                        if ((the_door_controller.OpenedClosedCompleted && !the_door_controller.NeededKey.Value) || !discovery_player_controller.DiscoveryPlayerNow)
                        {
                            if (!discovery_player_controller.DiscoveryPlayerNow)
                            {
                                the_door_controller.DoorOpenClosedChangeByStatusServerRPC(networkObject.NetworkObjectId);
                            }
                        }
                    }


                    if (the_door_controller.NeededKey.Value || discovery_player_controller.DiscoveryPlayerNow)
                    {
                        Debug.Log("�����󂵂����Ǝv���܂��B");

                        DoorAttackNow = true;

                    }
                    else
                    {
                        DoorAttackNow = false;
                    }

                    // �ǂꂩ 1 �{���������� break �Ń��[�v�I��
                    break;
                }
            }
        }
        if(DoorAttackNow && the_door_controller != null){
            discovery_player_controller.enabled = false;
            follow_object_by_nav.ChaseToTargetSet(true, the_door_controller.gameObject.transform.position, 0);
            follow_object_by_nav.StoppingDistance = 2;
            character_controller.Move(Vector3.zero);
            anim.SetBool("DoorAttack", true);
            enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Stop;
            discovery_player_controller.enabled = true;
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
