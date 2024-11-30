using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class CrossHairEvent : MonoBehaviour
{
    //�^�񒆂̃N���X�w�A���烌�C�L���X�g���΂����ꂪ���̂ɓ��������Ƃ��ɔ���������C�x���g�B
    public LayerMask TargetLayerMaskViewToPoint;
    public ItemBelongingsController item_belonging_controller;
    private PlayerInput player_input;
    public PlayerHideObjectAction player_hide_object_action;
    public GameObject Player;
    //�N���X�w�A�����΂����C�L���X�g�̋����B
    public float RaycastDistance = 3;

    Camera MainCamera;
    private RaycastHit raycast_hit;
    private CrossHairStatus cross_hair_status;
    public GameObject RaycasstedObj;

    public AudioSource ItemGetSource;


    private PrayController pray_controller;
    private TheDoorController the_door_controller;
    private Ray main_ray;
    private GameStateController game_state_controller;
    // Start is called before the first frame update
    void Start()
    {
        MainCamera = Camera.main;
        player_input = GameControllerOnAllScene.instance.player_input;
        cross_hair_status = this.gameObject.GetComponent<CrossHairStatus>();
    }

    // Update is called once per frame
    void Update()
    {
        //�J�����̒������烌�C�L���X�g���΂��B
        main_ray = MainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Debug.DrawRay(MainCamera.transform.position,MainCamera.transform.forward * RaycastDistance, Color.green);
        if (Physics.Raycast(main_ray, out raycast_hit, RaycastDistance, TargetLayerMaskViewToPoint))
        {

            if (raycast_hit.collider.gameObject.tag == "Pray")
            {
                if (RaycasstedObj != raycast_hit.collider.gameObject)
                {
                    pray_controller = raycast_hit.collider.gameObject.GetComponent<PrayController>();
                }
                pray_controller.OnTouchOn();
            }

            RaycasstedObj = raycast_hit.collider.gameObject;
            if (raycast_hit.collider.gameObject.layer == LayerMask.NameToLayer("Item") ||
                raycast_hit.collider.gameObject.tag == "HideActionObject" ||
                raycast_hit.collider.gameObject.tag == "Door" || raycast_hit.collider.gameObject.tag == "Drawer" ||
                raycast_hit.collider.gameObject.tag == "Light" || raycast_hit.collider.gameObject.tag == "Pray" || raycast_hit.collider.gameObject.tag == "DeliverPlace")
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


            if (player_input.currentActionMap["RaycastAction"].triggered)
            {
                Debug.Log("RaycastAction---------------------Triggered!!!!!!!!!!!!!!!!!!!!!");

                if (raycast_hit.collider.gameObject.layer == LayerMask.NameToLayer("Item"))
                {
                    ItemGetSource.Play();

                    ItemObject item_object = raycast_hit.collider.gameObject.GetComponent<ItemObject>();
                    item_object.ItemGet(item_belonging_controller);
                    Destroy(item_object.gameObject);
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
                if (raycast_hit.collider.gameObject.tag == "Door")
                {

                    DoorOpenCloseController door_open_close_controlelr = raycast_hit.collider.gameObject.GetComponent<DoorOpenCloseController>();
                    the_door_controller = door_open_close_controlelr.the_door_controller;
                    the_door_controller.DoorOpenClosedChangeByStatus(Player);

                    return;

                }
                if (raycast_hit.collider.gameObject.tag == "Drawer")
                {
                    ItemBoxOpenCloseController drawer_open_close_controller = raycast_hit.collider.gameObject.GetComponent<ItemBoxOpenCloseController>();
                    drawer_open_close_controller.OpenCloseChangeEventByStatus();
                    return;
                }

                if(raycast_hit.collider.gameObject.tag == "Light")
                {
                    TurningOnOffLightController turn_on_off_by_status = raycast_hit.collider.gameObject.GetComponent<TurningOnOffLightController>();
                    turn_on_off_by_status.TurnOnOffByStatus();
                    return;
                }

            }



            if (player_input.currentActionMap["RaycastAction"].IsPressed())
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
            }
        }
        else
        {
            cross_hair_status.RaycastHitedExistsByCrossHairEvent = false;
            RaycasstedObj = null;
        }
    }
}
