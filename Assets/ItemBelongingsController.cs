using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public class ItemBelongingsController : SerializedMonoBehaviour
{
    //アイテム持ち物コントローラー 

    //持っているアイテムの情報。
    public Dictionary<ItemStatus, int> belonging_items = new Dictionary<ItemStatus, int>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //アイテムをこのコントローラーに追加する関数 外部から実行する。　主にCrossHairEvent.csから
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
    //コントローラーにあるアイテム所持品からアイテムを消費する関数。
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
