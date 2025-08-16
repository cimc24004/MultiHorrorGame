using Unity.Entities.UniversalDelegates;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class FirstAidKitForPlayer : NetworkBehaviour
{
    public ItemStatus FirstAidKitItemStatus;
    private ItemBelongingsController PrayerItemBelongingsController;
    private ItemBelongingsController MyItemBelongingsController;
    private PrayController prayController;
    private PlayerStatus parentPlayerStatus;
    public bool CanFirstAid;
    private PlayerStatus myPlayerStatus;
    private PlayerAllController playerAllController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public override void OnNetworkSpawn()
    {
        PlayerStatus.OnLocalPlayerSpawned += InItOnPlayerSpawn;
        if (PlayerStatus.LocalInstance != null)
        {
            InItOnPlayerSpawn(PlayerStatus.LocalInstance);
        }
    }
    public override void OnNetworkDespawn()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InItOnPlayerSpawn;
    }
    private void InItOnPlayerSpawn(PlayerStatus _playerStatus)
    {
        parentPlayerStatus = this.gameObject.transform.parent.gameObject.GetComponent<PlayerStatus>();
        prayController = this.gameObject.GetComponent<PrayController>();
        myPlayerStatus = _playerStatus;
        playerAllController = PlayerAllController.instance;
        MyItemBelongingsController = myPlayerStatus.item_menu_controller.item_belongings_controller;
        prayController.PrayCompletedEventForServer.AddListener(() =>
        {
            // ★ ここはサーバーで実行される前提（Server側イベント）
            if (prayController.PrayNowPlayers == null || prayController.PrayNowPlayers.Count == 0)
            {
                Debug.LogWarning("PrayNowPlayersが空。payerを決められない");
                return;
            }

            ulong prayerId = prayController.PrayNowPlayers[0]; // 先頭を代表者に（好きにルール変更OK）
            prayController.PrayReset();

            // 代表者クライアントにだけRPCを送る
            var rpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { prayerId }
                }
            };
            ConsumeFirstAidClientRpc(rpcParams);

        });
        // prayController.PrayCompletedEvent.AddListener(() =>
        // {
        //     prayController.ReopenPrayServerRpc();
        // });

    }
    // Update is called once per frame
    void Update()
    {
        
        if (parentPlayerStatus.Dead.Value)
        {
            if (CheckItem())
            {
                this.gameObject.tag = "Pray";
                CanFirstAid = true;
            }
            else
            {
                this.gameObject.tag = "CantFirstAidKit";
                CanFirstAid = false;
            }
        }
        else
        {
            this.gameObject.tag = "Untagged";
            CanFirstAid = false;
        }

    }
    private bool CheckItem()
    {
        if (MyItemBelongingsController.belonging_items.Count != 0)
        {
            if (MyItemBelongingsController.belonging_items.ContainsKey(FirstAidKitItemStatus))
            {
                return true;
            }
        }
        return false;
    }
    [ClientRpc]
    private void ConsumeFirstAidClientRpc(ClientRpcParams rpcParams = default)
    {
        // 念のため初期化遅延対策
        if (MyItemBelongingsController == null && PlayerStatus.LocalInstance != null)
        {
            MyItemBelongingsController = PlayerStatus.LocalInstance.item_menu_controller.item_belongings_controller;
        }

        if (MyItemBelongingsController != null && CheckItem())
        {
            MyItemBelongingsController.ItemUseFromBelonging(FirstAidKitItemStatus);
            myPlayerStatus?.sub_noticed_ui_controller?.Notice($"{parentPlayerStatus.SteamUserName.Value} を応急処置した");
        }
        else
        {
            Debug.LogWarning("FirstAid消費できない（所持なし or 未初期化）");
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void OnCompletedPrayRequestServerRpc()
    {
    }
}
