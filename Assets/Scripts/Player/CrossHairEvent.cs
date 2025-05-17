using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class CrossHairEvent : NetworkBehaviour
{
    //�^�񒆂̃N���X�w�A���烌�C�L���X�g���΂����ꂪ���̂ɓ��������Ƃ��ɔ���������C�x���g�B
    public LayerMask TargetLayerMaskViewToPoint;
    public ItemBelongingsController item_belonging_controller;
    public AudioDatas KeyAudioDatas;
    private PlayerInput player_input;
    public PlayerHideObjectAction player_hide_object_action;
    private PlayerStatus player_status;
    private bool InitPlayer;
    private GameObject Player;
    //�N���X�w�A�����΂����C�L���X�g�̋����B
    public float RaycastDistance = 3;
    //クリックイベント
    public bool CrossHairEventRaycastPermit = true;
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
    private NetworkObject networkObject;
    // Start is called before the first frame update
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
    private void InitOnPlayerSpawn(PlayerStatus _playerStatus){
        player_status = _playerStatus;
        player_input = player_status.player_input;
        main_noticed_ui_controller = player_status.main_noticed_ui_controller;
        this.Player = player_status.gameObject;
        MainCamera = Camera.main;
        networkObject = this.Player.GetComponent<NetworkObject>();
        InitPlayer = true;
    }
    // Update is called once per frame
    void Update()
    {
        if(!InitPlayer) return;
        if(!IsOwner) return;
        //�J�����̒������烌�C�L���X�g���΂��B
        main_ray = MainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Debug.DrawRay(MainCamera.transform.position,MainCamera.transform.forward * RaycastDistance, Color.yellow);
        if (Physics.Raycast(main_ray, out raycast_hit, RaycastDistance, TargetLayerMaskViewToPoint))
        {
            //触れてる間に色々するとこ↓
            if (raycast_hit.collider.gameObject.tag == "Pray")
            {
                if (RaycasstedObj != raycast_hit.collider.gameObject)
                {
                    pray_controller = raycast_hit.collider.gameObject.GetComponent<PrayController>();
                    player_status.GageImage.fillAmount = 0;

                }
                pray_controller.OnTouchOn();
            }
            if(raycast_hit.collider.gameObject.tag == "HideActionObject"){
                main_noticed_ui_controller.Notice("隠れる", 1);
            }
            RaycasstedObj = raycast_hit.collider.gameObject;
            if (raycast_hit.collider.gameObject.layer == LayerMask.NameToLayer("Item") ||
                raycast_hit.collider.gameObject.tag == "HideActionObject" ||
                raycast_hit.collider.gameObject.tag == "Door" || raycast_hit.collider.gameObject.tag == "Drawer" ||
                raycast_hit.collider.gameObject.tag == "Light" || raycast_hit.collider.gameObject.tag == "Pray" || raycast_hit.collider.gameObject.tag == "DeliverPlace"
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

            if(!CrossHairEventRaycastPermit) return;
            if (player_input.actions["RaycastAction"].triggered)
            {
                Debug.Log("RaycastAction---------------------Triggered!!!!!!!!!!!!!!!!!!!!!");

                if (raycast_hit.collider.gameObject.layer == LayerMask.NameToLayer("Item"))
                {

                    ItemObject item_object = raycast_hit.collider.gameObject.GetComponent<ItemObject>();
                    bool item_geted = item_object.TryPickup(item_belonging_controller);

                    if (item_geted)
                    {
                        ItemGetSource.Play();
                    }

                    return;
                }
                if(raycast_hit.collider.gameObject.tag == "DeliverPlace")
                {
                    DeliverMemoryPlace deliver_memory_place = raycast_hit.collider.gameObject.GetComponent<DeliverMemoryPlace>();
                    deliver_memory_place.Deliver(item_belonging_controller);
                    return;
                }
                if (raycast_hit.collider.gameObject.tag == "HideActionObject")
                {

                    player_hide_object_action.HideAction = true;
                    player_hide_object_action.TargetHideObjectSet(raycast_hit.collider.gameObject);
                    return;

                }
                if (raycast_hit.collider.gameObject.tag == "Door" || raycast_hit.collider.gameObject.tag == "Cabinet" || raycast_hit.collider.gameObject.tag == "Window")
                {

                    DoorOpenCloseController door_open_close_controlelr = raycast_hit.collider.gameObject.GetComponent<DoorOpenCloseController>();
                    the_door_controller = door_open_close_controlelr.the_door_controller;
                    
                    the_door_controller.DoorOpenClosedChangeByStatusServerRPC(networkObject.NetworkObjectId);

                    return;

                }
                if (raycast_hit.collider.gameObject.tag == "Drawer")
                {
                    ItemBoxOpenCloseController drawer_open_close_controller = raycast_hit.collider.gameObject.GetComponent<ItemBoxOpenCloseController>();
                    drawer_open_close_controller.RequestToggle();
                    return;
                }

                if(raycast_hit.collider.gameObject.tag == "Light")
                {

                    TurningOnOffLightController turn_on_off_by_status = raycast_hit.collider.gameObject.GetComponent<TurningOnOffLightController>();
                    Debug.Log("NetworkObject Light: " + turn_on_off_by_status.gameObject.GetComponent<NetworkObject>());
                    turn_on_off_by_status.ToggleLight();
                    return;
                }
                if(raycast_hit.collider.gameObject.tag == "ReadEvent")
                {
                    ReadEventController read_event_controller = raycast_hit.collider.gameObject.GetComponent<ReadEventController>();
                    read_event_controller.ReadEventStart();
                }
                if(raycast_hit.collider.gameObject.tag == "GameClear"){
                    SceneManager.LoadScene("GameClear");
                }
            }



            if (player_input.actions["RaycastAction"].IsPressed())
            {
                //こっちは長押し用のイベント
                if(raycast_hit.collider.gameObject.tag == "Pray")
                {
                    Debug.Log("Pray------------COl");
                    if (pray_controller != null)
                    {
                        pray_controller.Pray();
                    }
                }
            }else{
            }
        }
        else
        {
            cross_hair_status.RaycastHitedExistsByCrossHairEvent = false;
            RaycasstedObj = null;
        }
    }
}
