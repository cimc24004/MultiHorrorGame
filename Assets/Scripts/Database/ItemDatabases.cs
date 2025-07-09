using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using System.IO;
using System.Linq;
public class ItemDatabases : Singleton<ItemDatabases>
{
    [Header("ワールドにランダム生成されるアイテムたち")]
    public List<ItemStatus> ForPutItems;

    [Header("すべてのアイテム")]
    public List<ItemStatus> AllItems;
    [Header("桜の木の下に納めるアイテムたち")]
    public ItemStatus ForDeliverItem;
#if UNITY_EDITOR
    [Button("すべてのPutアイテムを登録")]
        public void PutItemsDBMigrateReset()
        {
            ForPutItems.Clear();
            //UtenStatys[] all = AssetDatabase.LoadAssetAtPath<Itemstatus>("")
            string[] files = Directory.GetFiles("Assets/ScriptTable/Items");
            foreach(string file in files)
            {
                if (!file.Contains(".meta"))
                {
                    Debug.Log(file);
                    ItemStatus ItemStatus = (ItemStatus)AssetDatabase.LoadAssetAtPath<ItemStatus>(file) as ItemStatus;
                    ForPutItems.Add(ItemStatus);
                }
                //AssetDatabase.LoadAssetAtPath(file);
            }
        }
#endif
#if UNITY_EDITOR
    [Button("すべてのアイテムを登録")]
        public void ItemsDBMigrateReset()
        {
            AllItems.Clear();
            //UtenStatys[] all = AssetDatabase.LoadAssetAtPath<Itemstatus>("")
            string[] files = Directory.GetFiles("Assets/ScriptTable/Items");
            foreach(string file in files)
            {
                if (!file.Contains(".meta"))
                {
                    Debug.Log(file);
                    ItemStatus ItemStatus = (ItemStatus)AssetDatabase.LoadAssetAtPath<ItemStatus>(file) as ItemStatus;
                    AllItems.Add(ItemStatus);
                }
                //AssetDatabase.LoadAssetAtPath(file);
            }
        }
#endif
    public ItemStatus GetStatusByGlobalId(uint _gid)
    {   
        return ForPutItems.Find((x) => x.GlobalId == _gid);
    }
    public ItemStatus ItemFindByType(ItemStatus.ItemType item_type)
    {
        return ForPutItems.Find((x) => x.item_type == item_type);
    }
    public ItemStatus ItemFindByGlobalId(uint _item_globalId)
    {
        return ForPutItems.Find((x) => x.GlobalId == _item_globalId);
    }
    public GameObject ItemCreate(ItemStatus item_status, Transform Parent, Vector3 Position)
    {
        GameObject item_obj = Instantiate(item_status.ItemPutObjectPrefab, Position, item_status.ItemPutObjectPrefab.transform.rotation);
        // 親を先に設定

        // ローカル座標をゼロに設定（親の位置に配置）
        // item_obj.transform.localPosition = Vector3.zero;

        // Rigidbodyの設定
        var rigidbody = item_obj.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.isKinematic = true;
        }
        return item_obj;
    }


    // RandomItemCreate メソッドも同様に修正
    public GameObject RandomItemCreate(Transform Parent)
    {
        int r = Random.Range(0, 100);
        List<ItemStatus> resulted_items = new List<ItemStatus>();
        
        // レア度チェックロジック（既存のまま）
        foreach(ItemStatus item_status in ForPutItems)
        {
            Debug.Log("item_status: " + item_status);
            if(r >= 0 && r <= 5)
            {
                return null; // 激レアアイテムは未実装
            }
            else if(r > 5 && r <= 25)
            {
                if (ItemStatus.ApperanceRatio.RareToHit == item_status.apperance_ratio)
                {
                    resulted_items.Add(item_status);
                }
            }
            else if(r > 25 && r <= 55)
            {
                if (ItemStatus.ApperanceRatio.NormalToHit == item_status.apperance_ratio)
                {
                    resulted_items.Add(item_status);
                }
            }
            else if(r > 55 && r <= 100)
            {
                if (ItemStatus.ApperanceRatio.EasyToHit == item_status.apperance_ratio)
                {
                    resulted_items.Add(item_status);
                }
            }
        }
        
        if (resulted_items.Count == 0)
        {
            Debug.LogWarning("該当するレア度のアイテムが見つかりませんでした");
            return null;
        }
        
        Debug.Log($"resulted_itemsCount: {resulted_items.Count} r={r}");
        int result_r = Random.Range(0, resulted_items.Count);
        ItemStatus decided_result_item = resulted_items[result_r];

        Debug.Log($"アイテムは....{decided_result_item.apperance_ratio.ToString()}の{decided_result_item.ItemName}が当たりました！");
        
        // ItemCreate で親子関係も設定される
        return ItemCreate(decided_result_item, Parent, Parent.position);
    }
}
