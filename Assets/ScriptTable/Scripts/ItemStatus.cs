using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class StatusOfMakingStopEnemy
{
    //敵を停止させるためのステータスクラス。

    [Header("敵を停止させる時間")]
    public float MakingStopTime;

}

[CreateAssetMenu(fileName = "ItemStatus", menuName = "ScriptableObjects/CreateItemStatusAsset")]
public class ItemStatus : SerializedScriptableObject
{
    public enum ItemType
    {
        Consumable, //消耗品(カギとか)
        CanThrow, //投げられるもん
        Equip, //装備品(懐中電灯とか)
        Key //扉を開けるための鍵
    }
    public ItemType item_type;

    [Required] public Sprite MenuSprite;
    [Required] public string ItemName;
     public string ItemDescription;
    [Header("ワールド上に置くアイテムのオブジェクトのこと。")]
     public GameObject ItemObjectPrefab;

    [ShowIf("item_type", ItemType.CanThrow)]
    [Header("敵を停止させることができるアイテムかどうか")]
    public bool MakingStopEnemyItem;
    [ShowIf("MakingStopEnemyItem", true)]
    public StatusOfMakingStopEnemy status_of_making_stop_enemy;


}
