using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using Unity.Netcode;
public class PrayToDeliverItemAppearPlace : NetworkBehaviour
{
    public Transform DeliverItemApperancePlace;
    private ItemDatabases item_databases;
    private AudioSource audio_source;

    private GameProgressController game_progress_controller;
    public static PrayToDeliverItemAppearPlace instance { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        item_databases = ItemDatabases.instance;
        audio_source = this.gameObject.GetComponent<AudioSource>();
        game_progress_controller = GameController.instance.gameObject.GetComponent<GameProgressController>();
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
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (instance == null)
        {
            instance = this;
        }
    }
    [Button]
    public void OnPrayCompleted()
    {
        //サーバー側で実行されます
        game_progress_controller.OnPrayedButsudan();
        //どこかの仏壇で祈りがコンプリートされたとき。
        if (game_progress_controller.NeedPrayCount <= game_progress_controller.PrayCountNow.Value)
        {
            //必要な祈り回数が必要になったら最終イベントアイテムを出現させる。
            DeliverItemAppear();
        }
    }
    public void DeliverItemAppear()
    {

        //int r = Random.Range(0, item_databases.ForDeliverItem.Count);

        GameObject item = Instantiate(item_databases.ForDeliverItem.ItemPutObjectPrefab, DeliverItemApperancePlace.transform.position, Quaternion.identity);
        NetworkObject itemNetworkObject = item.GetComponent<NetworkObject>();
        itemNetworkObject.Spawn();
        //アイテムを取得したときのイベント。
        ItemGetedClientRpc(itemNetworkObject.NetworkObjectId);

        audio_source.Play();
    }

    [ClientRpc]
    private void ItemGetedClientRpc(ulong itemNetworkObjectId)
    {
        ItemObject item_object_script = NetworkManager.Singleton.SpawnManager.SpawnedObjects[itemNetworkObjectId].GetComponent<ItemObject>();
        item_object_script.ItemGetAddEvent.AddListener(() =>
        {
            GameProgressOnItemGetedRequestServerRpc();
        });
    }
    [ServerRpc(RequireOwnership = false)]
    private void GameProgressOnItemGetedRequestServerRpc()
    {
        game_progress_controller.OnItemGeted();
    }
}
