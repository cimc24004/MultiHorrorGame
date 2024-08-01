using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
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
    [Header("ワールド上に置くアイテムのオブジェクトのこと。")]
     public GameObject ItemObjectPrefab;
}
