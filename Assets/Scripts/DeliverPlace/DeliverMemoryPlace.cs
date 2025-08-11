using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;
using Unity.Netcode;
public class DeliverMemoryPlace : NetworkBehaviour
{
    [Header("届けなければいけないアイテムを指定")]
    public ItemStatus DeliverEventItemStatus;
    [Header("届ける数")]
    public int DeliverEventItemCount;
    [Header("すでに届けたアイテム数")]
    public NetworkVariable<int> AlreadyDeliveredItemCount = new NetworkVariable<int>();
    private PlayerStatus player_status;
    private ItemDatabases item_databases;
    private GameProgressController game_progress_controller;
    public static DeliverMemoryPlace instance { get; private set; }
    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        game_progress_controller = GameController.instance.gameObject.GetComponent<GameProgressController>();
        PlayerStatus.OnLocalPlayerSpawned += InitOnPlayerSpawn;
        if (instance == null)
        {
            instance = this;
        }
    }
    void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitOnPlayerSpawn;
    }
    private void InitOnPlayerSpawn(PlayerStatus _playerStatus)
    {
        player_status = _playerStatus;
        item_databases = ItemDatabases.instance;
        
    }

    private void Awake()
    {
        // Note: ここで Instance を設定しても NetworkObject がまだ生きてない可能性がある
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [Button]
    public void Deliver(ItemBelongingsController item_belongings_controller)
    {
        //クライアントで実行される
        Debug.Log("item_belongings_controller: " + item_belongings_controller);
        Debug.Log("item_databases: " + item_databases);
        if (item_belongings_controller.ExistBelongingItemByFind(item_databases.ForDeliverItem))
        {
            //持ち物に納めるアイテムが存在したらアイテムを使用します。

            player_status.sub_noticed_ui_controller.Notice("サクラの思い出の品を納めた。");
            item_belongings_controller.ItemUseFromBelonging(item_databases.ForDeliverItem);
        }
        else
        {
            //なかったらできないこととする。
            player_status.sub_noticed_ui_controller.Notice("思い出の品を持っていない。");
            return;
        }
        CompletedItemsDeliveredRequestServerRpc();

    }
    [ServerRpc(RequireOwnership = false)]
    public void CompletedItemsDeliveredRequestServerRpc()
    {
        //アイテムがすべて納められたら？
        Debug.Log("クリアしました！！！！！！\n");
        //SceneManager.LoadScene("GameClear");
        AlreadyDeliveredItemCount.Value += 1;
        if (DeliverEventItemCount - AlreadyDeliveredItemCount.Value <= 0)
        {
            game_progress_controller.OnDeliverSakura();
        }
    }
}
