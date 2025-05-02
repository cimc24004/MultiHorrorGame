using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Events;
using UnityEngine.UI;
public class PrayController : MonoBehaviour
{
    [Header("Notice用メッセージ")]
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
    private CharacterMovePermit player_character_move_permit;
    private bool once;
    private bool PrayForStartOnce;
    private bool PrayNowOnce;
    [Header("PrayNowがTrueの時はプレイヤーは動けないかどうか")]
    [SerializeField] private bool PlayerStopOnPrayNow;

    // Start is called before the first frame update
    void Start()
    {
        player_status = PlayerStatus.instance;
        player_character_move_permit = player_status.gameObject.GetComponent<CharacterMovePermit>();
        cross_hair_status = player_status.cross_hair_event.cross_hair_status;
        GageCanvas = player_status.GageCanvas;
        GageImageUI = player_status.GageImage;
        main_noticed_ui_controller = player_status.main_noticed_ui_controller;
    }

    // Update is called once per frame
    void Update()
    {
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
            if(PlayerStopOnPrayNow){
                player_character_move_permit.Stop();
            }
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
            if(PlayerStopOnPrayNow){
                player_character_move_permit.Move();
            }

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
