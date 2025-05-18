using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Butsudan : NetworkBehaviour
{
    private PrayToDeliverItemAppearPlace pray_to_deliver_item_appear_place;
    private EnemyAllController enemy_all_controller;
    private SubNoticedUIController sub_noticed_ui_controller;
    [SerializeField]private SilhouetteChangeController silhouette_change_controller;
    private HousesController houses_controller;
    private enum ButsudanDefenseType{
        JapaneseNingyo,
        Darumasan,
        OshiireNoMouja,
        None
    }
    void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InitOnPlayerSpawn;
    }
    void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitOnPlayerSpawn;
    }
    private void InitOnPlayerSpawn(PlayerStatus _playerStatus){
        pray_to_deliver_item_appear_place = PrayToDeliverItemAppearPlace.instance;
        enemy_all_controller = EnemyAllController.Instance;
        sub_noticed_ui_controller = PlayerStatus.LocalInstance.sub_noticed_ui_controller;
        houses_controller = HousesController.Instance;
    }

    public void ButsudanPrayCompleted()
    {
        Debug.Log("deliver_item_apperance: " + pray_to_deliver_item_appear_place) ;
        pray_to_deliver_item_appear_place.OnPrayCompleted();
        sub_noticed_ui_controller.Notice("仏壇にお祈りした");
        enemy_all_controller.IncreaseEnemiesMoveSpeed();
        sub_noticed_ui_controller.Notice("敵の能力が向上した....。", 1);
    }

    public void SilhouetteDelete(){
        silhouette_change_controller.SilhouetteChange(false);
        houses_controller.ButudansSilhouetteChangeControllers.Remove(silhouette_change_controller);
    }
}
