using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemObject : MonoBehaviour
{
    //ワールド上に置かれているアイテムにつけておくクラス。
    public ItemStatus item_status;
    private Rigidbody rigid;
    private void Start()
    {
    }
    //この関数が呼び出されれば自分のアイテムメニューにitem_statusのアイテムを追加する。
    public void ItemGet(ItemBelongingsController item_belonging_controller)
    {
        item_belonging_controller.ItemAddToBelonging(item_status);
    }
}
