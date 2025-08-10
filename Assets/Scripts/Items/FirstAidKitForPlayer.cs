using Unity.Entities.UniversalDelegates;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class FirstAidKitForPlayer : NetworkBehaviour
{
    public ItemStatus FirstAidKitItemStatus;
    private ItemBelongingsController MyitemBelongingsController;
    private PrayController prayController;
    private PlayerStatus parentPlayerStatus;
    public bool CanFirstAid;
    private PlayerStatus myPlayerStatus;

    void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InItOnPlayerSpawn;
        if (PlayerStatus.LocalInstance != null)
        {
            InItOnPlayerSpawn(PlayerStatus.LocalInstance);
        }
    }

    void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InItOnPlayerSpawn;

        // 購読解除（nullチェック）
        if (prayController != null)
        {
            prayController.OnPrayCompletedWithWinner -= OnPrayCompletedWithWinner;
        }
    }

    private void InItOnPlayerSpawn(PlayerStatus _playerStatus)
    {
        parentPlayerStatus = this.gameObject.transform.parent.gameObject.GetComponent<PlayerStatus>();
        MyitemBelongingsController = _playerStatus.item_menu_controller.item_belongings_controller;
        prayController = this.gameObject.GetComponent<PrayController>();
        myPlayerStatus = _playerStatus;

        // ★ 代表者ID付きイベントを購読
        if (prayController != null)
        {
            prayController.OnPrayCompletedWithWinner -= OnPrayCompletedWithWinner;
            prayController.OnPrayCompletedWithWinner += OnPrayCompletedWithWinner;
        }

        // 既存の引数なしイベント購読や、PrayNowPlayers[0] 判定は不要
        // prayController.PrayCompletedEvent.AddListener(...); ←消してOK
    }

    private void OnPrayCompletedWithWinner(ulong winnerClientId)
    {
        // 死体に対する応急処置だけに限定（必要なら）
        if (!parentPlayerStatus.Dead.Value) return;

        // 自分が代表者なら消費
        if (winnerClientId == myPlayerStatus.OwnerClientId)
        {
            if (CheckItem())
            {
                MyitemBelongingsController.ItemUseFromBelonging(FirstAidKitItemStatus);

                // ここはローカルUIで十分。全体に出したいなら別途UI系のClientRpc用意してもいい
                myPlayerStatus.sub_noticed_ui_controller.Notice($"{parentPlayerStatus.SteamUserName} を応急処置した");
            }
            else
            {
                Debug.LogWarning("FirstAidKitが手持ちにないため消費できず");
            }
        }
    }

    private bool CheckItem()
    {
        return MyitemBelongingsController.belonging_items.Count != 0
            && MyitemBelongingsController.belonging_items.ContainsKey(FirstAidKitItemStatus);
    }

    // これもう不要（サーバー側で完了処理やってる）
    //[ServerRpc]
    //private void OnCompletedPrayRequestServerRpc() { prayController.PrayReset(); }

    // これも自分通知だけなら不要。どうしてもRPCでやりたいなら残してOK
    //[ClientRpc]
    //private void FirstAidUsedClientRpc(){ ... }
}
