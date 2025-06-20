using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Butsudan : MonoBehaviour
{
    private PrayToDeliverItemAppearPlace pray_to_deliver_item_appear_place;
    private EnemyAllController enemy_all_controller;
    private SubNoticedUIController sub_noticed_ui_controller;
    [SerializeField]private SilhouetteChangeController silhouette_change_controller;
    [SerializeField] private PrayController pray_controller;
    private HousesController houses_controller;
    private AudioSource audioSource;
    private ObjectLayerControl object_layer_control;
    private enum ButsudanDefenseType
    {
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
    private void InitOnPlayerSpawn(PlayerStatus _playerStatus)
    {
        pray_to_deliver_item_appear_place = PrayToDeliverItemAppearPlace.instance;
        enemy_all_controller = EnemyAllController.instance;
        sub_noticed_ui_controller = PlayerStatus.LocalInstance.sub_noticed_ui_controller;
        houses_controller = HousesController.Instance;
        audioSource = pray_controller.gameObject.GetComponent<AudioSource>();
        object_layer_control = pray_controller.gameObject.GetComponent<ObjectLayerControl>();
        pray_controller.PrayCompletedEvent.AddListener(() =>
        {
            ButsudanPrayCompletedForClients();
            SilhouetteDelete();
            audioSource.Play();
            object_layer_control.ChangeLayerByName("Default");

        });
        pray_controller.PrayCompletedEventForServer.AddListener(() =>
        {
            enemy_all_controller.IncreaseEnemiesMoveSpeed();
        });
    }

    public void ButsudanPrayCompletedForClients()
    {
        //クライアントから実行される。 PrayController.OnCompletedEventとおなじタイミングなので
        Debug.Log("ButsudanPrayCompleted()を実行！");
        Debug.Log("deliver_item_apperance: " + pray_to_deliver_item_appear_place) ;
        pray_to_deliver_item_appear_place.OnPrayCompleted();
        sub_noticed_ui_controller.Notice("仏壇にお祈りした");
        sub_noticed_ui_controller.Notice("敵の能力が向上した....。", 1);
    }

    public void SilhouetteDelete(){
        silhouette_change_controller.SilhouetteChange(false);
        houses_controller.ButudansSilhouetteChangeControllers.Remove(silhouette_change_controller);
    }
}
