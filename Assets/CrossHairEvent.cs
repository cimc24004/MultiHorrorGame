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
        RaycastHit hit;
        Debug.DrawRay(MainCamera.transform.position,MainCamera.transform.forward * RaycastDistance, Color.green);
        if (player_input.currentActionMap["RaycastAction"].triggered)
        {
            if (Physics.Raycast(ray, out hit,RaycastDistance, TargetLayerMaskViewToPoint))
            {
                //レイキャストが何か物に当たった場合。

                //物に当たってその時にクリックするといろいろイベントを実行。
                //アイテムを入手する。 扉を開ける。 机に隠れるなどのイベントを書く。

                Debug.Log(hit.collider.gameObject);
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Item"))
                {
                    //その例キャストに当たった物がアイテムならば　アイテムを取得。
                    ItemObject item_object = hit.collider.gameObject.GetComponent<ItemObject>();
                    item_object.ItemGet(item_belonging_controller);
                    Destroy(item_object.gameObject);
                }
                if(hit.collider.gameObject.tag == "HideActionObject")
                {
                    //隠れるオブジェクトにレイキャストが当たってたら 隠れるアクション実行。
                    player_hide_object_action.HideAction = true;
                    player_hide_object_action.TargetHideObjectSet(hit.collider.gameObject);
                }
            }

        }
    }
}
