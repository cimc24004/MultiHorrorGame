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
        Key, //扉を開けるための鍵
        Event //イベント用 桜の木の下に納めるやつ。
    }
    public enum ApperanceRatio
    {
        EasyToHit,
        NormalToHit,
        RareToHit,
        VeryRareToHit
    }
    public ItemType item_type;

    [Required] public Sprite MenuSprite;
    [Required] public string ItemName;
    public string ItemDescription;
    [Header("ランダムで引き出しなどにアイテムを召喚させるか。")]
    public bool PutInItemBox = true;
    [HideIf("item_type", ItemType.Equip)]
    [Header("ワールド上に置くアイテムのPrefabのこと。")]
    public GameObject ItemPutObjectPrefab;
    [HideIf("item_type", ItemType.Equip)]
    [Header("アイテムを使用したときのPrefab(投げたりのときは)")]
    public GameObject ItemUseObjectPrefab;


    [ShowIf("item_type", ItemType.CanThrow)]
    [Header("敵を停止させることができるアイテムかどうか")]
    public bool MakingStopEnemyItem;

    [ShowIf("MakingStopEnemyItem", true)]
    public StatusOfMakingStopEnemy status_of_making_stop_enemy;

    [ShowIf("PutInItemBox")]
    [Header("ランダムでこのアイテムが引かれる可能性。")]
    [Required] public ApperanceRatio apperance_ratio;
}
