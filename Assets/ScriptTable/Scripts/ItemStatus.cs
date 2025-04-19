using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 敵を停止させるためのステータスクラス。
/// </summary>
public class StatusOfMakingStopEnemy
{
    [Header("敵を止める時間")]
    public float MakingStopTime;
}


[CreateAssetMenu(fileName = "ItemStatus", menuName = "ScriptableObjects/CreateItemStatusAsset")]
public class ItemStatus : SerializedScriptableObject
{
    public class StealthStatus
    {
        [Header("足音を一定時間聞こえなくする")]
        public float FootStepStealthTime;
    }

    /// <summary>
    /// アイテムの種類
    /// </summary>
    public enum ItemType
    {
        Consumable,      // 消費アイテム (例：回復など)
        CanThrow,        // 投擲可能アイテム
        Equip,           // 装備アイテム（武器や防具など）
        Key,             // 扉の解錠などに使うキーアイテム
        Event,           // イベント用（特定の場面でのみ使用）
        EmergencyAttack  // 緊急時の攻撃アイテム
    }

    /// <summary>
    /// 出現率
    /// </summary>
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

    [TextArea]
    public string ItemDescription;

    [Header("フィールドに配置されるアイテムかどうか")]
    public bool PutInItemBox = true;

    // [HideIf("item_type", ItemType.Equip)]
    [Header("ワールド上に配置するときのアイテムPrefab")]
    public GameObject ItemPutObjectPrefab;

    [HideIf("item_type", ItemType.Equip)]
    [Header("アイテムを投げた時に生成されるPrefab（投擲用）")]
    public GameObject ItemThrowPrefab;

    [ShowIf("@item_type == ItemType.CanThrow || item_type == ItemType.EmergencyAttack")]
    [Header("敵を止めることができる時間")]
    public float StopEnemyTime = 3;

    [ShowIf("item_type", ItemType.Consumable)]
    [Header("アイテムの効果時間")]
    public float ConsumableEffectTime;

    [ShowIf("item_type", ItemType.CanThrow)]
    [Header("敵を一時停止させるアイテムかどうか")]
    public bool MakingStopEnemyItem;

    [ShowIf("MakingStopEnemyItem", true)]
    public StatusOfMakingStopEnemy status_of_making_stop_enemy;

    [ShowIf("PutInItemBox")]
    [Header("フィールドアイテムとしての出現率")]
    [Required] public ApperanceRatio apperance_ratio;

    [Header("アイテムをクリックで使用できるか")]
    public bool ItemUseFromClick = true;

    [Header("アイテム使用時にスクリプトを生成する際に使うPrefab")]
    [SerializeField] public GameObject InstantiateItemScriptPrefab;

    [Header("アイテムを所持できる最大数")]
    [SerializeField] public int MaxCanHave = 2;
}
