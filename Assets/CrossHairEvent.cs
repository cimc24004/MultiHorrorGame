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
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //カメラの中央からレイキャストを飛ばす。
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit,Mathf.Infinity, TargetLayerMaskViewToPoint))
        {
            //レイキャストが何か物に当たった場合。
            if (player_input.currentActionMap["RaycastAction"].triggered)
            {
                //物に当たってその時にクリックするといろいろイベントを実行。

                //アイテムを入手する。 扉を開ける。 机に隠れるなどのイベントを書く。
                if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Item"))
                {
                    //その例キャストに当たった物がアイテムならば　アイテムを取得。
                    ItemObject item_object = hit.collider.gameObject.GetComponent<ItemObject>();
                    item_object.ItemGet(item_belonging_controller);
                    Destroy(item_object.gameObject);
                }

            }

            Debug.Log(hit.collider.gameObject);
        }
    }
}
