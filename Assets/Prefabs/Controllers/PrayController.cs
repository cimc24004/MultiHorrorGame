using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Events;
using UnityEngine.UI;
public class PrayController : MonoBehaviour
{
    [Header("必要な祈り時間")]
    public float NeedToPrayTime;
    public bool PrayNow;
    public bool PrayPermit = true;

    public float PrayTimeNow;

    public UnityEvent2 PrayCompletedEvent;

    private bool touch;
    private PlayerStatus player_status;
    private GameObject GageCanvas;
    private NoticedUIController noticed_ui_controller;
    private bool AlreadyPrayCompleted;
    // Start is called before the first frame update
    void Start()
    {
        player_status = PlayerStatus.instance;
        GageCanvas = player_status.GageCanvas;
        noticed_ui_controller = player_status.noticed_ui_controller;
    }

    // Update is called once per frame
    void Update()
    {

        if (touch)
        {
            GageCanvas.SetActive(false);
            noticed_ui_controller.Notice("お祈りする。");
            if (!AlreadyPrayCompleted)
            {
                GageCanvas.SetActive(true);
            }
            touch = false;
        }
        if (PrayNow && PrayPermit && !AlreadyPrayCompleted)
        {
            GageCanvas.SetActive(true);
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
