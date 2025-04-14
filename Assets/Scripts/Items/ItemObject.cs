using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class ItemObject : MonoBehaviour
{
    
    //ワールド上に置かれているアイテムにつけておくクラス。
    public ItemStatus item_status;
    public UnityEvent ItemGetAddEvent;
    private void Start()
    {
    }
    //この関数が呼び出されれば自分のアイテムメニューにitem_statusのアイテムを追加する。
    public bool ItemGet(ItemBelongingsController item_belonging_controller)
    {
        var differentItems = item_belonging_controller.belonging_items
        .Where(kvp => kvp.Key != item_status)
        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        bool NotHaveMore = (item_belonging_controller.maxCarryableItems <= differentItems.Count) 
                            || IsItemAtMaxCapacity(item_belonging_controller);

        item_belonging_controller.item_menu_controller.OnItemGetChangeUI(NotHaveMore, item_status);
        
        if (!NotHaveMore)
        {
            item_belonging_controller.ItemAddToBelonging(item_status);
            ItemGetAddEvent.Invoke();
            Destroy(this.gameObject, 0.1f);
            return true;
        }
        else
        {
            //このアイテムはこれ以上持てません
            return false;
        }
    }
    private bool IsItemAtMaxCapacity(ItemBelongingsController item_belonging_controller){


        bool hasItemAlready  = item_belonging_controller.belonging_items.ContainsKey(item_status);
        bool isOverCapacity = false;
        if (hasItemAlready )
        {
            isOverCapacity = (
                item_belonging_controller.belonging_items[item_status].item_count + 1 
                > item_status.MaxCanHave
            );
        }
        return isOverCapacity;
    }
}
