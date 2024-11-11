using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public class ItemRandomCreateIndividual : SerializedMonoBehaviour
{
    [Header("アイテムを親要素にくっつける。 親要素を指定")]
    public Transform ItemToParent;
    //引き出しなどにつけるスクリプト。　アイテムランダム生成されたもの
    private ItemDatabases item_databases;
    
    private void Start()
    {
        item_databases = ItemDatabases.instance;
    }
    [Button]
    public void ItemRandomCreateFromIndividual(bool _AlreadyOpended)
    {
        int r = Random.Range(0, 6);
        if (_AlreadyOpended) return;
        if (r != 0)
        {
            Debug.Log("空でした。");
            return;
        }
        Debug.Log("ItemRandomCreateFromIndividual-------------------");
        GameObject random_created_item = item_databases.RandomItemCreate(ItemToParent);

    }
}
