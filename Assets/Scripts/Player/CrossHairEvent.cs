using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class CrossHairEvent : MonoBehaviour
{
    //真ん中のクロスヘアからレイキャストを飛ばしそれが物体に当たったときに発生させるイベント。
    public LayerMask TargetLayerMaskViewToPoint;
    public ItemBelongingsController item_belonging_controller;
    public PlayerInput player_input;
    public PlayerHideObjectAction player_hide_object_action;
    public GameObject Player;
    //クロスヘアから飛ばすレイキャストの距離。
    public float RaycastDistance = 3;

    Camera MainCamera;
    private RaycastHit raycast_hit;
    public GameObject RaycasstedObj;
    // Start is called before the first frame update
    void Start()
    {
        MainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        //カメラの中央からレイキャストを飛ばす。
        Ray ray = MainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Debug.DrawRay(MainCamera.transform.position,MainCamera.transform.forward * RaycastDistance, Color.green);
        if (Physics.Raycast(ray, out raycast_hit, RaycastDistance, TargetLayerMaskViewToPoint))
        {
            RaycasstedObj = raycast_hit.collider.gameObject;
            if (player_input.currentActionMap["RaycastAction"].triggered)
            {

                //レイキャストが何か物に当たった場合。

                //物に当たってその時にクリックするといろいろイベントを実行。
                //アイテムを入手する。 扉を開ける。 机に隠れるなどのイベントを書く。

                if (raycast_hit.collider.gameObject.layer == LayerMask.NameToLayer("Item"))
                {
                    //その例キャストに当たった物がアイテムならば　アイテムを取得。
                    ItemObject item_object = raycast_hit.collider.gameObject.GetComponent<ItemObject>();
                    item_object.ItemGet(item_belonging_controller);
                    Destroy(item_object.gameObject);
                }
                if(raycast_hit.collider.gameObject.tag == "HideActionObject")
                {
                    //隠れるオブジェクトにレイキャストが当たってたら 隠れるアクション実行。
                    player_hide_object_action.HideAction = true;
                    player_hide_object_action.TargetHideObjectSet(raycast_hit.collider.gameObject);
                }
                if(raycast_hit.collider.gameObject.tag == "Door")
                {
                    //ドアをクリックしたらドアを開ける。
                    DoorOpenCloseController door_open_close_controlelr = raycast_hit.collider.gameObject.GetComponent<DoorOpenCloseController>();
                    TheDoorController the_door_controller = door_open_close_controlelr.the_door_controller;
                    the_door_controller.DoorOpenClosedChangeByStatus();
                }
            }

        }
        else
        {
            RaycasstedObj = null;
        }

    }
}
