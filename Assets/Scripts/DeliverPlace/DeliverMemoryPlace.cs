using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;
public class DeliverMemoryPlace : SerializedMonoBehaviour
{
    [Header("届けなければいけないアイテムを指定")]
    public ItemStatus DeliverEventItemStatus;
    [Header("届ける数")]
    public int DeliverEventItemCount;
    [Header("すでに届けたアイテム数")]
    public int AlreadyDeliveredItemCount;
    private PlayerStatus player_status;
    private ItemDatabases item_databases;
    private GameProgressController game_progress_controller;
    // Start is called before the first frame update
    void Start()
    {
        player_status = PlayerStatus.instance;
        item_databases = ItemDatabases.instance;
        game_progress_controller = GameProgressController.instance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [Button]
    public void Deliver(ItemBelongingsController item_belongings_controller)
    {
        if (item_belongings_controller.ExistItemUIItemStatusByFind(item_databases.ForDeliverItem))
        {
            //持ち物に納めるアイテムが存在したらアイテムを使用します。
            
            player_status.noticed_ui_controller.Notice("サクラの思い出の品を納めた。");
            item_belongings_controller.ItemUseFromBelonging(item_databases.ForDeliverItem);
        }
        else
        {
            //なかったらできないこととする。
            player_status.noticed_ui_controller.Notice("思い出の品を持っていない。");
            return;
        }
        AlreadyDeliveredItemCount += 1;
        if(DeliverEventItemCount - AlreadyDeliveredItemCount <= 0)
        {
            CompletedItemsDelivered();
        }

    }
    public void CompletedItemsDelivered()
    {
        //アイテムがすべて納められたら？
        Debug.Log("クリアしました！！！！！！\n");
        //SceneManager.LoadScene("GameClear");
        game_progress_controller.OnDeliverSakura();
    }
}
