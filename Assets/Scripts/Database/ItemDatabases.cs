using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using System.IO;

[ExecuteInEditMode]
public class ItemDatabases : Singleton<ItemDatabases>
{
    //コノゲームに出現するアイテムすべて。
    public List<ItemStatus> AllItems;
    
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
    [Button("RandomItemCreate100回")]
    public void GO()
    {
        for(int i = 0; i < 100; i++)
        {
            RandomItemCreate(null);
        }
    }
    [Button]
    public GameObject RandomItemCreate(Transform ItemCreatePos)
    {
        int r = Random.Range(0, 100);
        List<ItemStatus> resulted_items = new List<ItemStatus>();
        
        //ランダムされたrの値でレア度をチェックする。

        foreach(ItemStatus item_status in AllItems)
        {
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

            }
            else if(r > 25 && r <= 55)
            {
                if (ItemStatus.ApperanceRatio.NormalToHit == item_status.apperance_ratio)
                {
                    resulted_items.Add(item_status);
                }


            }
            else if(r>55 && r <= 100)
            {
                if (ItemStatus.ApperanceRatio.EasyToHit == item_status.apperance_ratio)
                {
                    resulted_items.Add(item_status);
                }

            }
        }

        int result_r = Random.Range(0,resulted_items.Count);
        ItemStatus decided_result_item = resulted_items[result_r];
        Debug.Log($"アイテムは....{decided_result_item.apperance_ratio.ToString()}の{decided_result_item.ItemName}が当たりました！");
        //return null;
        return Instantiate(decided_result_item.ItemObjectPrefab, ItemCreatePos.position, Quaternion.identity);
    }
}
