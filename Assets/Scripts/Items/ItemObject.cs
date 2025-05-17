using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(NetworkObject))]
public class ItemObject : NetworkBehaviour
{
    public ItemStatus item_status;
    public UnityEvent ItemGetAddEvent;

    private NeedKeysController need_keys_controller;
    private NetworkObject      netObj;

    void Awake() => netObj = GetComponent<NetworkObject>();

    public override void OnNetworkSpawn()
    {
        need_keys_controller = NeedKeysController.instance;
    }
    public override void OnNetworkDespawn()
    {
        gameObject.SetActive(false);
    }
    /*──────── クライアント入口 ───────*/
    public void RequestItemGet()
    {
        if (!IsSpawned) return;
        RequestItemGetServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    /*──────── Client → Server ───────*/
    [ServerRpc(RequireOwnership = false)]
    void RequestItemGetServerRpc(ulong clientId, ServerRpcParams _ = default)
    {
        var playerObj = NetworkManager.Singleton.SpawnManager
                              .GetPlayerNetworkObject(clientId).gameObject;

        var inv = playerObj.GetComponent<PlayerStatus>()
                           .ItemController.GetComponent<ItemBelongingsController>();

        int  unique = inv.belonging_items
                         .Where(kvp => kvp.Key.item_type != ItemStatus.ItemType.Event)
                         .Count();
        bool already = inv.belonging_items.ContainsKey(item_status);
        bool slotLim = !already && unique >= inv.maxCarryableItems;
        bool reached = slotLim || IsItemAtMax(inv);

        bool success = !reached || item_status.item_type == ItemStatus.ItemType.Event;
        int  newCnt  = 0;

        if (success)
        {
            if (item_status.item_type == ItemStatus.ItemType.Key)
                need_keys_controller.GottenKeyCount++;

            inv.ItemAddToBelonging(item_status);
            newCnt = inv.belonging_items[item_status].item_count;
        }

        var cp = new ClientRpcParams {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
        };
        ItemGetResponseClientRpc(success, newCnt, cp);

        if (success) netObj.Despawn(true);
    }

    /*──────── Server → Client（拾い主）────────*/
    [ClientRpc]
    void ItemGetResponseClientRpc(bool success, int newCnt, ClientRpcParams _ = default)
    {
        var invUI = PlayerStatus.LocalInstance
                                .ItemController.GetComponent<ItemBelongingsController>();

        if (success)
        {
            invUI.ForceSyncFromServer(item_status, newCnt);
            ItemGetAddEvent.Invoke();
        }
        invUI.item_menu_controller?.OnItemGetChangeUI(!success, item_status);
    }

    /*──────── 補助 ───────*/
    private bool IsItemAtMax(ItemBelongingsController inv)
    {
        return inv.belonging_items.TryGetValue(item_status, out var e) &&
               e.item_count + 1 > item_status.MaxCanHave;
    }
}
