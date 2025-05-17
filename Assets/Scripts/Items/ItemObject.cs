using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class ItemObject : NetworkBehaviour
{
    private NeedKeysController need_keys_controller;
    //ワールド上に置かれているアイテムにつけておくクラス。
    public ItemStatus item_status;
    public UnityEvent ItemGetAddEvent;
    private NetworkObject ItemNetworkObject;
    public override void OnNetworkSpawn()
    {
        need_keys_controller = NeedKeysController.instance;
        ItemNetworkObject = this.gameObject.GetComponent<NetworkObject>();
    }
    [ServerRpc]
    private void ItemGetRequestServerRPC(ItemBelongingsController item_belonging_controller, Action<bool> callback)
    {
        var differentItems = item_belonging_controller.belonging_items
        .Where(kvp => kvp.Key != item_status && kvp.Key.item_type != ItemStatus.ItemType.Event)
        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        bool NotHaveMore = (item_belonging_controller.maxCarryableItems <= differentItems.Count)
                            || IsItemAtMaxCapacity(item_belonging_controller);
        if (!NotHaveMore || item_status.item_type == ItemStatus.ItemType.Event)
        {
            if (item_status.item_type == ItemStatus.ItemType.Key)
            {
                need_keys_controller.GottenKeyCount += 1;
            }

            ItemNetworkObject.Despawn(true);
        }
        else
        {
            //このアイテムはこれ以上持てません
        }
        callback.Invoke(NotHaveMore);
        ItemGetClientRPC(item_belonging_controller, NotHaveMore, item_status);
    }
    [ClientRpc]
    private void ItemGetClientRPC(ItemBelongingsController item_belonging_controller, bool NotHaveMore, ItemStatus _item_status)
    {
        if (!NotHaveMore)
        {
            item_belonging_controller.ItemAddToBelonging(_item_status);
        }
        ItemGetAddEvent.Invoke();
        item_belonging_controller.item_menu_controller.OnItemGetChangeUI(NotHaveMore, _item_status);
    }
    //この関数が呼び出されれば自分のアイテムメニューにitem_statusのアイテムを追加する。
    public bool ItemGet(ItemBelongingsController item_belonging_controller)
    {
        bool result = false;
        ItemGetRequestServerRPC(item_belonging_controller, (x) =>
        {
            result = x;
        });
        return result;

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
