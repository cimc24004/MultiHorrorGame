using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class CrossHairEvent : NetworkBehaviour
{
    // 画面中央からのレイキャストで調べる対象レイヤーマスク
    public LayerMask TargetLayerMaskViewToPoint;
    public ItemBelongingsController item_belonging_controller;
    public AudioDatas KeyAudioDatas;
    private PlayerInput player_input;
    public PlayerHideObjectAction player_hide_object_action;
    private PlayerStatus player_status;
    private bool InitPlayer;
    private GameObject Player;
    // クロスヘアのレイキャスト到達距離
    public float RaycastDistance = 3;
    //クリックイベント
    public NetworkVariable<bool> CrossHairEventRaycastPermit = new NetworkVariable<bool>(true);
    //死んだときのCrossHairEvent
    public NetworkVariable<bool> CrossHairEventPermitForDeath = new NetworkVariable<bool>(true);
    Camera MainCamera;
    private RaycastHit raycast_hit;
    public CrossHairStatus cross_hair_status;
    public GameObject RaycasstedObj;

    public AudioSource ItemGetSource;


    private PrayController pray_controller;
    private TheDoorController the_door_controller;
    private Ray main_ray;
    private GameStateController game_state_controller;
    private MainNoticedUIController main_noticed_ui_controller;
    private NetworkObject PlayerNetworkObject;
    private PrayController pushing_raycasted_pray_controller;
    private PlayerAllController playerAllController;
    // Start は最初のフレーム前に呼ばれる
    void Start()
    {
    }
    void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InitOnPlayerSpawn;
    }
    void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitOnPlayerSpawn;
    }
    private void InitOnPlayerSpawn(PlayerStatus _playerStatus)
    {
        playerAllController = PlayerAllController.instance;
        player_status = _playerStatus;
        player_input = player_status.player_input;
        main_noticed_ui_controller = player_status.main_noticed_ui_controller;
        this.Player = player_status.gameObject;
        MainCamera = Camera.main;
        PlayerNetworkObject = this.Player.GetComponent<NetworkObject>();
        InitPlayer = true;
        if (IsOwner)
        {
            player_input.actions["RaycastAction"].started += RaycastActionStart;
            player_input.actions["RaycastAction"].canceled += RaycastActionCanceled;
        }

    }
    // 毎フレーム呼び出される
    void Update()
    {
        if (!InitPlayer) return;
        if (!IsOwner) return;
        // 画面中心からレイを飛ばす
        main_ray = MainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Debug.DrawRay(MainCamera.transform.position, MainCamera.transform.forward * RaycastDistance, Color.yellow);
        if (Physics.Raycast(main_ray, out raycast_hit, RaycastDistance, TargetLayerMaskViewToPoint))
        {
            //触れてる間に色々するとこ↓
            if (CrossHairEventPermitForDeath.Value)
            {
                if (raycast_hit.collider.gameObject.tag == "Pray")
                {
                    if (RaycasstedObj != raycast_hit.collider.gameObject)
                    {
                        pray_controller = raycast_hit.collider.gameObject.GetComponent<PrayController>();
                        player_status.GageImage.fillAmount = 0;

                    }
                    // pray_controller.OnTouchOn();
                }
            }
            if (CrossHairEventPermitForDeath.Value)
            {
                if (raycast_hit.collider.gameObject.tag == "CantFirstAidKit")
                {
                    PlayerStatus targetPlayerStatus = playerAllController.SpawnedPlayersWithPlayerStatuses[raycast_hit.collider.gameObject];
                    if (targetPlayerStatus.Dead.Value && !player_status.firstAidKitForPlayer.CanFirstAid)
                    {
                        main_noticed_ui_controller.Notice("救急箱が必要だ");
                    }
                }
            }
            if (CrossHairEventPermitForDeath.Value)
            {
                if (raycast_hit.collider.gameObject.tag == "HideActionObject")
                {
                    main_noticed_ui_controller.Notice("隠れる", 1);
                }
            }

            RaycasstedObj = raycast_hit.collider.gameObject;
            if (raycast_hit.collider.gameObject.layer == LayerMask.NameToLayer("Item") && CrossHairEventPermitForDeath.Value ||
                raycast_hit.collider.gameObject.tag == "HideActionObject" && CrossHairEventPermitForDeath.Value ||
                raycast_hit.collider.gameObject.tag == "Door" || raycast_hit.collider.gameObject.tag == "Drawer" ||
                raycast_hit.collider.gameObject.tag == "Light" || raycast_hit.collider.gameObject.tag == "Pray" && CrossHairEventPermitForDeath.Value || raycast_hit.collider.gameObject.tag == "DeliverPlace" && CrossHairEventPermitForDeath.Value
                || raycast_hit.collider.gameObject.tag == "ReadEvent" || raycast_hit.collider.gameObject.tag == "Window"
                || raycast_hit.collider.gameObject.tag == "Cabinet" || raycast_hit.collider.gameObject.tag == "GameClear")
            {
                //ターゲットのオブジェクトにレイキャストが当たっているなら当たってますよ変数をTrueにしておく
                cross_hair_status.HitedTag = RaycasstedObj.tag;
                cross_hair_status.HitedObj = RaycasstedObj;
                cross_hair_status.RaycastHitedExistsByCrossHairEvent = true;
            }
            else
            {
                cross_hair_status.RaycastHitedExistsByCrossHairEvent = false;
            }

            if (!CrossHairEventRaycastPermit.Value) return;
            if (player_input.actions["RaycastAction"].triggered)
            {
                Debug.Log("RaycastAction---------------------Triggered!!!!!!!!!!!!!!!!!!!!!");

                if (raycast_hit.collider.gameObject.layer == LayerMask.NameToLayer("Item"))
                {
                    if (CrossHairEventPermitForDeath.Value)
                    {
                        ItemObject item_object = raycast_hit.collider.gameObject.GetComponent<ItemObject>();
                        item_object.TryPickup();
                    }
                    return;
                }
                if (raycast_hit.collider.gameObject.tag == "DeliverPlace")
                {
                    if (CrossHairEventPermitForDeath.Value)
                    {
                        DeliverMemoryPlace deliver_memory_place = raycast_hit.collider.gameObject.GetComponent<DeliverMemoryPlace>();
                        deliver_memory_place.Deliver(item_belonging_controller);
                    }
                    return;
                }
                if (raycast_hit.collider.gameObject.tag == "HideActionObject")
                {
                    if (CrossHairEventPermitForDeath.Value)
                    {
                        if (!player_status.HidingNow.Value)
                        {
                            NetworkObject networkObject = raycast_hit.collider.gameObject.GetComponent<NetworkObject>();
                            player_hide_object_action.HideUnHideRayCastActionPerformedRequestServerRpc(networkObject.NetworkObjectId);
                        }
                    }
                    return;

                }
                if (raycast_hit.collider.gameObject.tag == "Door" || raycast_hit.collider.gameObject.tag == "Cabinet" || raycast_hit.collider.gameObject.tag == "Window")
                {

                    DoorOpenCloseController door_open_close_controller = raycast_hit.collider.gameObject.GetComponent<DoorOpenCloseController>();
                    the_door_controller = door_open_close_controller.the_door_controller;

                    the_door_controller.DoorOpenClosedChangeByStatusServerRPC(PlayerNetworkObject.NetworkObjectId);
                    return;

                }
                if (raycast_hit.collider.gameObject.tag == "Drawer")
                {
                    ItemBoxOpenCloseController drawer_open_close_controller = raycast_hit.collider.gameObject.GetComponent<ItemBoxOpenCloseController>();
                    drawer_open_close_controller.RequestToggle();
                    return;
                }

                if (raycast_hit.collider.gameObject.tag == "Light")
                {
                    TurningOnOffLightController turn_on_off_by_status = raycast_hit.collider.gameObject.GetComponent<TurningOnOffLightController>();
                    Debug.Log("NetworkObject Light: " + turn_on_off_by_status.gameObject.GetComponent<NetworkObject>());
                    turn_on_off_by_status.ToggleLight();
                    return;
                }
                if (raycast_hit.collider.gameObject.tag == "ReadEvent")
                {
                    ReadEventController read_event_controller = raycast_hit.collider.gameObject.GetComponent<ReadEventController>();
                    read_event_controller.ReadEventStart();
                }
                if (raycast_hit.collider.gameObject.tag == "GameClear")
                {
                    SceneManager.LoadScene("GameClear");
                }
            }



            if (player_input.actions["RaycastAction"].IsPressed())
            {
                //こっちはUpdate関数内で設定した
                if(raycast_hit.collider.gameObject.tag == "Pray")
                {
                    Debug.Log("Pray------------COl");
                    if (pray_controller != null)
                    {
                        pray_controller.Pushing();
                    }
                }
            }
        }
        else
        {
            cross_hair_status.RaycastHitedExistsByCrossHairEvent = false;
            RaycasstedObj = null;
        }
    }
    private void RaycastActionStart(InputAction.CallbackContext ctx) {
        RaycastActionPushingEvent(true);
    }
    private void RaycastActionCanceled(InputAction.CallbackContext ctx) {
        RaycastActionPushingEvent(false);
    }
    private void RaycastActionPushingEvent(bool _Pushing)
    {
        Debug.Log("RaycastActionPushingEvent_Mode: " + _Pushing);
        if (_Pushing)
        {
            // 長押し開始時だけ RaycasstedObj から PrayController を拾って保持
            if (RaycasstedObj != null && RaycasstedObj.tag == "Pray")
            {
                pushing_raycasted_pray_controller = RaycasstedObj.GetComponent<PrayController>();
                if (pushing_raycasted_pray_controller != null)
                    pushing_raycasted_pray_controller.PrayNowTrueRequestServerRpc();
            }
        }
        else
        {
            // 長押し終了時は、RaycasstedObj なんか見ずに、
            // 最初に保持したコントローラに必ず「やめたよ」を投げる
            if (pushing_raycasted_pray_controller != null)
            {
                pushing_raycasted_pray_controller.PrayNowFalseRequestServerRpc();
                pushing_raycasted_pray_controller = null;
            }
        }
    }
}
