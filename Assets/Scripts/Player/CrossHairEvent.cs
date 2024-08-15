using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class CrossHairEvent : MonoBehaviour
{
    //�^�񒆂̃N���X�w�A���烌�C�L���X�g���΂����ꂪ���̂ɓ��������Ƃ��ɔ���������C�x���g�B
    public LayerMask TargetLayerMaskViewToPoint;
    public ItemBelongingsController item_belonging_controller;
    public PlayerInput player_input;
    public PlayerHideObjectAction player_hide_object_action;
    public GameObject Player;
    //�N���X�w�A�����΂����C�L���X�g�̋����B
    public float RaycastDistance = 3;

    Camera MainCamera;
    private RaycastHit raycast_hit;
    private CrossHairStatus cross_hair_status;
    public GameObject RaycasstedObj;

    // Start is called before the first frame update
    void Start()
    {
        MainCamera = Camera.main;
        cross_hair_status = this.gameObject.GetComponent<CrossHairStatus>();
    }

    // Update is called once per frame
    void Update()
    {
        //�J�����̒������烌�C�L���X�g���΂��B
        Ray ray = MainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Debug.DrawRay(MainCamera.transform.position,MainCamera.transform.forward * RaycastDistance, Color.green);
        if (Physics.Raycast(ray, out raycast_hit, RaycastDistance, TargetLayerMaskViewToPoint))
        {
            RaycasstedObj = raycast_hit.collider.gameObject;

            if (raycast_hit.collider.gameObject.layer == LayerMask.NameToLayer("Item") ||
                raycast_hit.collider.gameObject.tag == "HideActionObject" ||
                raycast_hit.collider.gameObject.tag == "Door")
            {
                //ターゲットのオブジェクトにレイキャストが当たっているなら当たってますよ変数をTrueにしておく
                cross_hair_status.RaycastHitedExistsByCrossHairEvent = true;
            }
            else
            {
                cross_hair_status.RaycastHitedExistsByCrossHairEvent = false;
            }


            if (player_input.currentActionMap["RaycastAction"].triggered)
            {

                //���C�L���X�g���������ɓ��������ꍇ�B

                //���ɓ������Ă��̎��ɃN���b�N����Ƃ��낢��C�x���g�����s�B
                //�A�C�e������肷��B �����J����B ���ɉB���Ȃǂ̃C�x���g�������B
                if (raycast_hit.collider.gameObject.layer == LayerMask.NameToLayer("Item"))
                {
                    //���̗�L���X�g�ɓ������������A�C�e���Ȃ�΁@�A�C�e�����擾�B


                    ItemObject item_object = raycast_hit.collider.gameObject.GetComponent<ItemObject>();
                    item_object.ItemGet(item_belonging_controller);
                    Destroy(item_object.gameObject);
                }
                if (raycast_hit.collider.gameObject.tag == "HideActionObject")
                {
                    //�B���I�u�W�F�N�g�Ƀ��C�L���X�g���������Ă��� �B���A�N�V�������s�B

                    player_hide_object_action.HideAction = true;
                    player_hide_object_action.TargetHideObjectSet(raycast_hit.collider.gameObject);
                }
                if(raycast_hit.collider.gameObject.tag == "Door")
                {
                    //�h�A���N���b�N������h�A���J����B

                    DoorOpenCloseController door_open_close_controlelr = raycast_hit.collider.gameObject.GetComponent<DoorOpenCloseController>();
                    TheDoorController the_door_controller = door_open_close_controlelr.the_door_controller;
                    the_door_controller.DoorOpenClosedChangeByStatus();

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
