using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

public class ItemEquipController : SerializedMonoBehaviour
{
    // 装備アイテムとその状態を保持する辞書
    [SerializeField] private Dictionary<ItemStatus, GameObject> equip_items_with_item_status;

    // 現在の装備アイテムのGameObject
    [SerializeField]
    private GameObject equip_item;
    
    // 直前に装備されていたアイテムのGameObject
    [SerializeField]
    private GameObject BeforeEquip;

    // // Startメソッド：初期化処理を行う（今回は未使用）
    // void Start()
    // {
        
    // }

    // // Updateメソッド：毎フレーム呼ばれる（今回は未使用）
    // void Update()
    // {
        
    // }
    
    // Odin InspectorのButton属性を使ってエディタ上から呼び出せるメソッド
    // 引数のItemStatusに基づき装備を変更する処理
    [Button]
    public void EquipChange(ItemStatus item_status)
    {
        // 渡されたItemStatusが装備用でない場合、現在の装備を解除する
        if(item_status.item_type != ItemStatus.ItemType.Equip)
        {
            // 既に装備されているアイテムがあれば非表示にする
            if(BeforeEquip != null)
            {
                BeforeEquip.SetActive(false);
            }
            // 装備解除後、BeforeEquipをリセットする
            BeforeEquip = null;
            return;
        }
        
        // 辞書から該当する装備アイテムを取得する
        equip_item = equip_items_with_item_status[item_status];
        
        // 新たに装備するアイテムが既に装備中の場合は何もしない
        if(equip_item == BeforeEquip) return;
        
        // 新たな装備アイテムを表示する
        equip_item.gameObject.SetActive(true);
        
        // 既に装備されていたアイテムがあれば非表示にする
        if(BeforeEquip != null)
        {
            BeforeEquip.SetActive(false);
        }

        // 現在の装備アイテムをBeforeEquipに設定する
        BeforeEquip = equip_item;
    }
}
