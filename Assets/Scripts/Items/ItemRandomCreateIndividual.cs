using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Unity.Netcode;

public class ItemRandomCreateIndividual : SerializedMonoBehaviour
{
    [Header("アイテムを親要素にくっつける。親要素を指定")]
    public Transform ItemToParent;

    [Header("アイテム出現確率用パラメータ")]
    public int FrequencyItemApperance = 6;

    private ItemBelongingsController item_belonging_controller;
    private ItemDatabases item_databases;
    private NeedKeysController need_keys_controller;

    void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InitOnPlayerSpawn;
    }

    void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitOnPlayerSpawn;
    }

    private void InitOnPlayerSpawn(PlayerStatus _playerStatus)
    {
        item_databases           = ItemDatabases.instance;
        item_belonging_controller = _playerStatus.ItemController.GetComponent<ItemBelongingsController>();
        need_keys_controller     = NeedKeysController.instance;

        // 親要素にも NetworkObject が必要
    }
    // ItemRandomCreateIndividual.cs の改良版
    [Button]
    public void ItemRandomCreateFromIndividual(bool _AlreadyOpened)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (_AlreadyOpened) return;

        // キー所持チェック
        bool keyHaved = item_belonging_controller.ItemFindByItemType(ItemStatus.ItemType.Key) != null;

        // 出現確率決定
        int r = Random.Range(0, FrequencyItemApperance);
        int key_r = Random.Range(0, 3);

        GameObject random_created_item = null;

        if (r != 0)
        {
            Debug.Log("空でした。");
            return;
        }

        // アイテム生成（親子関係は ItemCreate 内で設定済み）
        if (need_keys_controller.NeedKeyCheck() && !keyHaved)
        {
            if (key_r == 0)
                random_created_item = item_databases.ItemCreate(
                    item_databases.ItemFindByType(ItemStatus.ItemType.Key),
                    ItemToParent,
                    ItemToParent.position);
            else
                random_created_item = item_databases.RandomItemCreate(ItemToParent);
        }
        else
        {
            random_created_item = item_databases.RandomItemCreate(ItemToParent);
        }

        if (random_created_item == null)
        {
            Debug.LogError("アイテム生成に失敗したよ");
            return;
        }

        // NetworkObject をスポーン
        var netObj = random_created_item.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn();
            Debug.Log($"アイテム '{random_created_item.name}' を {ItemToParent.name} の子として生成しました");
        }
        else
        {
            Debug.LogError("NetworkObject コンポーネントが見つかりません");
        }
        netObj.TrySetParent(ItemToParent);
}
    // // アイテムと親をくっつける RPC
    // [ClientRpc]
    // void AttachToParentClientRpc(ulong itemId)
    // {
    //     var spawned = NetworkManager.Singleton.SpawnManager.SpawnedObjects;
    //     if (!spawned.TryGetValue(itemId,   out var itemObj))   return;

    //     // worldPositionStays = false でローカル座標 (0,0,0) にリセットしつつ SetParent
    //     spawned[itemId].TrySetParent(ItemToParent);
    // }
}
