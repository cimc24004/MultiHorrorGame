using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Sirenix.OdinInspector;
using Unity.Netcode;
using Unity.Entities.UniversalDelegates;
public class EnemyActionForDoorController : NetworkBehaviour
{
    // 扉を攻撃したり開閉したりする処理

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
    private CameraShakeController cameraShakeController;
    private ToPlayersDistanceTracker toPlayersDistanceTracker;
    private PlayerAllController playerAllController;
    [SerializeField] private GameObject AttackedDoorEffectPrefab;
    // Start は最初のフレーム前に呼ばれる
    public override void OnNetworkSpawn()
    {
        cameraShakeController = CameraShakeController.instance;
        if (!IsServer) return;
        playerAllController = PlayerAllController.instance;
        toPlayersDistanceTracker = this.gameObject.GetComponent<ToPlayersDistanceTracker>();
        networkObject = this.gameObject.GetComponent<NetworkObject>();
        enemy_status = this.gameObject.GetComponent<EnemyStatus>();
        follow_object_by_nav = this.gameObject.GetComponent<FollowObjectByNav>();
        nav_mesh_agent = this.gameObject.GetComponent<NavMeshAgent>();
        discovery_player_controller = this.gameObject.GetComponent<DiscoveryPlayerController>();
        anim = this.gameObject.GetComponent<Animator>();
        enemy_sound_sensor = this.gameObject.GetComponent<EnemySoundSensor>();
        character_controller = this.gameObject.GetComponent<CharacterController>();
    }

    // 毎フレーム呼び出される
    void Update()
    {
        if (!IsServer) return;
        if (!AutoDoorAttack) return;
        if (DoorTouchNow) DoorTouchNow = false;
        anim.SetBool("DoorAttack", false);
        DoorAttackNow = false;


        foreach (Transform start in rayStarts)
        {
            if (start == null) continue;                     // null のトランスフォームは無視
            Debug.DrawRay(start.position, start.forward * RaycastDistance, Color.red);

            if (Physics.Raycast(start.position, start.forward, out raycast_hit, RaycastDistance, layer_mask))
            {
                Debug.Log("raycast_hit_collider.gameObject: " + raycast_hit.collider.gameObject);
                if (raycast_hit.collider.gameObject.tag == "Door")
                {
                    DoorTouchNow = true;
                    // 扉にヒットしたので処理を行う
                    Debug.Log("Door hit target: " + raycast_hit.collider.gameObject);

                    if (raycast_hit_obj != raycast_hit.collider.gameObject || door_open_close_controller == null)
                    {
                        door_open_close_controller = raycast_hit.collider.gameObject.GetComponent<DoorOpenCloseController>();
                    }
                    raycast_hit_obj = raycast_hit.collider.gameObject;
                    if (door_open_close_controller != null)
                    {
                        the_door_controller = door_open_close_controller.the_door_controller;
                    }
                    if (DoorOpenClosePermit)
                    {
                        if ((the_door_controller.OpenedClosedCompleted && !the_door_controller.NeededKey.Value) || !discovery_player_controller.DiscoveryPlayerNowCheck())
                        {
                            if (!discovery_player_controller.DiscoveryPlayerNowCheck())
                            {
                                the_door_controller.DoorOpenClosedChangeByStatusServerRPC(networkObject.NetworkObjectId);
                            }
                        }
                    }


                    if (the_door_controller.NeededKey.Value || discovery_player_controller.DiscoveryPlayerNowCheck())
                    {
                        Debug.Log("鍵が掛かっているかプレイヤーを発見したので攻撃します");

                        DoorAttackNow = true;

                    }
                    else
                    {
                        DoorAttackNow = false;
                    }

                    // 1つ処理したらループを抜ける
                    break;
                }
            }
        }
        if (DoorAttackNow && the_door_controller != null)
        {
            discovery_player_controller.enabled = false;
            follow_object_by_nav.ChaseToTargetSet(true, the_door_controller.gameObject.transform.position, 0, FollowObjectByNav.MoveStyle.Stop);
            follow_object_by_nav.StoppingDistance = 2;
            character_controller.Move(Vector3.zero);
            anim.SetBool("DoorAttack", true);
            // enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Stop;
            discovery_player_controller.enabled = true;
        }
    }
    public void AttackToDoor()
    {
        //Animation Eventで実行される。
        if (the_door_controller != null)
        {
            door_attacked_controller = the_door_controller.door_attacked_controller;
        }
        if (door_attacked_controller == null)
        {
            return;
        }
        DoorBroken = this.door_attacked_controller.DoorAttacked(BrokeDoorPlusCount);
        follow_object_by_nav.StoppingDistance = follow_object_by_nav.DefaultStoppingDistance;
        if (DoorBroken)
        {
            follow_object_by_nav.StoppingDistance = follow_object_by_nav.DefaultStoppingDistance;
        }
        DoorAttackForAllClientRpc(DoorBroken);

        List<ulong> targetClientIds = new List<ulong>();
        foreach (KeyValuePair<GameObject, float> distanceWithPlayer in toPlayersDistanceTracker.ToPlayerDistanceLists)
        {
            GameObject player = distanceWithPlayer.Key;
            float distance = distanceWithPlayer.Value;
            if (distance <= 6)
            {
                //ある程度プレイヤーとの距離が近かったらそのプレイヤーに対してDoorAttackのカメラの揺れを送信する
                targetClientIds.Add(playerAllController.SpawnedPlayersWithPlayerStatuses[player].OwnerClientId);
            }
        }
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = targetClientIds }
        };
        DoorAttackForNearClientRpc(DoorBroken, clientRpcParams);

    }

    [ClientRpc]
    private void DoorAttackForAllClientRpc(bool _doorBroken)
    {
        if (_doorBroken)
        {
            //ドアを破壊した
            DoorAttackSource.PlayOneShot(DoorBrokenClip);
            DoorAttackSource.PlayOneShot(DoorAttackClip);
        }
        else
        {
            //ドアを攻撃中
            DoorAttackSource.PlayOneShot(DoorAttackClip);
        }
    }
    [ClientRpc]
    private void DoorAttackForNearClientRpc(bool _doorBroken, ClientRpcParams clientRpcParams)
    {
        //この敵の近くのプレイヤーに対して送信する
        //ドアの破壊による揺れエフェクト
        if (_doorBroken)
        {
            cameraShakeController.ShakeCamera(0.2f, CameraShakeController.CameraImpulseSourceType.DoorAttack);
        }
        else
        {
            cameraShakeController.ShakeCamera(0.1f, CameraShakeController.CameraImpulseSourceType.DoorAttack);
        }
    }
}
