using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public class ItemBelongingsController : SerializedMonoBehaviour
{
    public Dictionary<ItemStatus, int> belonging_items = new Dictionary<ItemStatus, int>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [Button]
    public void ItemAddToBelonging(ItemStatus item_status)
    {
        if (!belonging_items.ContainsKey(item_status))
        {
            belonging_items.Add(item_status, 1);
        }
        else
        {
            int BelongingCountNow = belonging_items[item_status];
            BelongingCountNow += 1;
            belonging_items[item_status] = BelongingCountNow;
        }
    }
    [Button]
    public void ItemUseFromBelonging(ItemStatus item_status)
    {
        int BelongingCountNow = belonging_items[item_status];
        BelongingCountNow -= 1;
        if(BelongingCountNow > 0)
        {
            belonging_items[item_status] = BelongingCountNow;
        }
        else
        {
            belonging_items.Remove(item_status);
        }
    }
}
