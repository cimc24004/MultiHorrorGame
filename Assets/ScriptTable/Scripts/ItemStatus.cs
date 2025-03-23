using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class StatusOfMakingStopEnemy
{
    //�G���~�����邽�߂̃X�e�[�^�X�N���X�B

    [Header("�G���~�����鎞��")]
    public float MakingStopTime;

}


[CreateAssetMenu(fileName = "ItemStatus", menuName = "ScriptableObjects/CreateItemStatusAsset")]
public class ItemStatus : SerializedScriptableObject
{
    public class StealthStatus
    {
        [Header("�G���������������Ȃ��Ȃ�悤�ɂ���b��")]
        public float FootStepStealthTime;
    }

    public enum ItemType
    {
        Consumable, //���Օi(�J�M�Ƃ�)
        CanThrow, //�����������
        Equip, //�����i(�����d���Ƃ�)
        Key, //�����J���邽�߂̌�
        Event, //�C�x���g�p ���̖؂̉��ɔ[�߂��B
        EmergencyAttack //�߂܂����Ƃ��ɍU���ł���A�C�e��
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
    [TextArea]
    public string ItemDescription;
    [Header("�����_���ň����o���ȂǂɃA�C�e�������������邩�B")]
    public bool PutInItemBox = true;
    [HideIf("item_type", ItemType.Equip)]
    [Header("���[���h��ɒu���A�C�e����Prefab�̂��ƁB")]
    public GameObject ItemPutObjectPrefab;
    [HideIf("item_type", ItemType.Equip)]
    [Header("�A�C�e�����g�p�����Ƃ���Prefab(��������̂Ƃ���)")]
    public GameObject ItemThrowPrefab;
    [ShowIf("@item_type == ItemType.CanThrow || item_type == ItemType.EmergencyAttack")]
    public float StopEnemyTime = 3;

    [ShowIf("item_type", ItemType.Consumable)]
    [Header("�A�C�e���̌��ʎ���")]
    public float ConsumableEffectTime;


    [ShowIf("item_type", ItemType.CanThrow)]
    [Header("�G���~�����邱�Ƃ��ł���A�C�e�����ǂ���")]
    public bool MakingStopEnemyItem;

    [ShowIf("MakingStopEnemyItem", true)]
    public StatusOfMakingStopEnemy status_of_making_stop_enemy;

    [ShowIf("PutInItemBox")]
    [Header("�����_���ł��̃A�C�e��������������B")]
    [Required] public ApperanceRatio apperance_ratio;

    [Header("�A�C�e�����E�N���b�N����g�p�ł��邩")]
    public bool ItemUseFromClick = true;

    [Header("�A�C�e�����g�p�����Ƃ��ɃX�N���v�g�����s����p��prefab")]
    [SerializeField] public GameObject InstantiateItemScriptPrefab;

    [Header("�A�C�e�������Ă���E��")]
    [SerializeField] public int MaxCanHave = 2;
}
