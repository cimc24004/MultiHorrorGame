using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Unity.Entities.UniversalDelegates;
using System.Linq;
public class ItemRandomCreateIndividual : SerializedMonoBehaviour
{
    private ItemBelongingsController item_belonging_controller;
    [Header("アイテムを親要素にくっつける。 親要素を指定")]
    public Transform ItemToParent;
    //引き出しなどにつけるスクリプト。　アイテムランダム生成されたもの
    private ItemDatabases item_databases;
    private NeedKeysController need_keys_controller;
    public int FrequencyItemApperance = 6;
    private void Start()
    {
        item_databases = ItemDatabases.instance;
        item_belonging_controller = ItemBelongingsController.instance;
        need_keys_controller = NeedKeysController.instance;
    }
    [Button]
    public void ItemRandomCreateFromIndividual(bool _AlreadyOpended)
    {
        int r = Random.Range(0, FrequencyItemApperance);
        int key_r = Random.Range(0, 3);

        GameObject random_created_item = null;
        if (_AlreadyOpended) return;

        if (r != 0)
        {
            Debug.Log("空でした。");
            return;
        }
        bool Keyhaved = item_belonging_controller.ItemFindByItemType(ItemStatus.ItemType.Key) != null;
        
        //鍵が必要な個数分持っているか && 鍵をすでに一個持っているか
        //もし条件を満たせば鍵の出現確立を上げる。
        if(need_keys_controller.NeedKeyCheck() && !Keyhaved){
            if(key_r == 0){
                //キーが当たったら
                random_created_item = item_databases.ItemCreate(item_databases.ItemFindByType(ItemStatus.ItemType.Key),ItemToParent, ItemToParent.transform.position);
            }else{
                //キーが
                random_created_item = item_databases.RandomItemCreate(ItemToParent);
            }
        }else{
            random_created_item = item_databases.RandomItemCreate(ItemToParent);
        }
        Debug.Log("ItemRandomCreateFromIndividual-------------------");
        if(random_created_item != null){
            random_created_item.isStatic = true;
        }
    }
}
