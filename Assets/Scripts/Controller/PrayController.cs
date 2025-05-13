using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Events;
using UnityEngine.UI;
using Unity.Netcode;
public class PrayController : NetworkBehaviour
{
    [Header("Notice用メッセージ")]
    private bool Inited;
    [SerializeField]
    private string NoticeMessage;
    [Header("必要な祈り時間")]
    public float NeedToPrayTime;
    public bool PrayNow;
    public bool PrayPermit = true;

    public float PrayTimeNow;

    public UnityEvent2 PrayCompletedEvent;
    [SerializeField] private UnityEvent2 PrayStartEvent;
    private bool touch;
    private PlayerStatus player_status;
    private GameObject GageCanvas;
    private Image GageImageUI;
    private MainNoticedUIController main_noticed_ui_controller;
    private bool AlreadyPrayCompleted;
    private CrossHairStatus cross_hair_status;
    private bool once;
    private bool PrayForStartOnce;
    private bool OnSpawnNetwork;
    // Start is called before the first frame update

    void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InitOnPlayerSpawn;
    }
    void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitOnPlayerSpawn;
    }

    public void InitOnPlayerSpawn(PlayerStatus _playerStatus){
        player_status = _playerStatus;
        cross_hair_status = player_status.cross_hair_event.cross_hair_status;
        GageCanvas = player_status.GageCanvas;
        GageImageUI = player_status.GageImage;
        OnSpawnNetwork = true;
        main_noticed_ui_controller = player_status.main_noticed_ui_controller;
        Inited = true;
    }


    // Update is called once per frame
    void Update()
    {
        if(!Inited) return;
        if (!once)
        {
           GageCanvas.SetActive(false);
            once = true;
        }

        if (cross_hair_status.HitedTag == "Pray")
        {
            if (cross_hair_status.RaycastHitedExistsByCrossHairEvent && cross_hair_status.HitedObj == this.gameObject)
            {
                //触れていたら
                GageCanvas.SetActive(true);
                main_noticed_ui_controller.Notice(this.NoticeMessage, 1);
                once = false;
            }
        }
        if (PrayNow && PrayPermit && !AlreadyPrayCompleted){
            Debug.Log("PrayNowwwwwwwwwww");
            GageCanvas.SetActive(true);
            if(!PrayForStartOnce){
                PrayStartEvent.Invoke();
                PrayForStartOnce = true;
            }
            player_status.GageImage.fillAmount = PrayTimeNow / NeedToPrayTime;
            PrayTimeNow += Time.deltaTime;
            if (PrayTimeNow > NeedToPrayTime)
            {
                Debug.Log("CompletedPray!!!");
                PrayCompleted();
            }
            PrayNow = false;
        }
    }
    public void OnTouchOn()
    {
        //このオブジェクトに触れていたら
        touch = true;
    }
    public void Pray()
    {
        Debug.Log("Prayyyyyyyyyyyyyyyyyyyyyy");
        PrayNow = true;
    }

    private void PrayCompleted()
    {
        player_status.GageImage.fillAmount = 0;
        PrayCompletedEvent.Invoke();
        this.gameObject.tag = "Untagged";
        AlreadyPrayCompleted = true;
        PrayNow = false;
        PrayPermit = false;
        PrayTimeNow = 0;
    }
}
