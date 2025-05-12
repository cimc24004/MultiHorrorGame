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
    
    [Header("桜の木の下に納めるアイテムたち")]
    public ItemStatus ForDeliverItem;
#if UNITY_EDITOR
    [Button("すべてのアイテムを登録")]
        public void ItemsDBMigrateReset()
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
    public ItemStatus ItemFindByType(ItemStatus.ItemType item_type){
        return ForPutItems.Find((x) => x.item_type == item_type);
    }
    public GameObject ItemCreate(ItemStatus item_status, Transform Parent, Vector3 Position){
        GameObject item_obj = Instantiate(item_status.ItemPutObjectPrefab,Position,item_status.ItemPutObjectPrefab.transform.rotation, Parent.transform);
        item_obj.transform.localPosition = Vector3.zero;
        item_obj.GetComponent<Rigidbody>().isKinematic = true;
        return item_obj;
    }
    public GameObject RandomItemCreate(Transform Parent)
    {
        int r = Random.Range(0, 100);
        List<ItemStatus> resulted_items = new List<ItemStatus>();
        
        //ランダムされたrの値でレア度をチェックする。

        foreach(ItemStatus item_status in ForPutItems)
        {
            Debug.Log("item_status: " + item_status);
            if(r>=0 && r <= 5)
            {
                //激レアアイテムはいま存在しないため。エラーがでるかな
                //if(ItemStatus.ApperanceRatio.VeryRareToHit == item_status.apperance_ratio)
                //{
                //    resulted_items.Add(item_status);
                //}
                return null;
            }
            else if(r > 5 && r <= 25)
            {
                if (ItemStatus.ApperanceRatio.RareToHit == item_status.apperance_ratio)
                {
                    resulted_items.Add(item_status);
                }
                else
                {
                    Debug.Log(item_status.ItemName + "は存在しませんでした");
                }

            }
            else if(r > 25 && r <= 55)
            {
                if (ItemStatus.ApperanceRatio.NormalToHit == item_status.apperance_ratio)
                {
                    resulted_items.Add(item_status);
                }
                else
                {
                    Debug.Log(item_status.ItemName + "は存在しませんでした");
                }


            }
            else if(r>55 && r <= 100)
            {
                if (ItemStatus.ApperanceRatio.EasyToHit == item_status.apperance_ratio)
                {
                    resulted_items.Add(item_status);
                }
                else
                {
                    Debug.Log(item_status.ItemName + "は存在しませんでした");
                }

            }
        }
        Debug.Log($"resulted_itemsCOunt: {resulted_items.Count} r={r}");
        int result_r = Random.Range(0,resulted_items.Count);
        ItemStatus decided_result_item = resulted_items[result_r];

        Debug.Log($"アイテムは....{decided_result_item.apperance_ratio.ToString()}の{decided_result_item.ItemName}が当たりました！");
        return ItemCreate(decided_result_item, Parent, Parent.transform.position);
    }

}
