using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitGateController : Singleton<ExitGateController>
{
    private Animator m_Animator;
    private PrayController pray_controller;
    [SerializeField] private GameObject ThroughPreventCollider;

    [SerializeField] private bool OnCompleteDoorOpen;
    private GameProgressController game_progress_controller;
    private float BeforePrayNow = 0;

    [SerializeField] private AudioSource OpenAudioSource;
    [SerializeField] private AudioSource CloseAudioSource;
    [SerializeField] private AudioSource OpenCompletedAudioSource;

    // Start is called before the first frame update
    void Start()
    {
        m_Animator = this.gameObject.GetComponent<Animator>();
        pray_controller = this.gameObject.GetComponent<PrayController>();
        game_progress_controller = GameProgressController.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if(OnCompleteDoorOpen) return;
        if(game_progress_controller.DeliveredToSakura){
            //すでにサクラの木に納めてるならドアを開けられる
            this.gameObject.tag = "Pray";
            this.gameObject.layer = LayerMask.NameToLayer("Object");
        }else{
            //サクラの木に納めてないならドアを開けられない
            this.gameObject.tag = "Untagged";
            this.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            
        }
        // if(this.gameObject.name == "Gate"){
        // }
        if(pray_controller.PrayTimeNow.Value == BeforePrayNow){
            ExitGateOpenPause();
        }else{
            ExitGateOpen();
        }
        BeforePrayNow = pray_controller.PrayTimeNow.Value;
    }

    public void ExitGateOpen(){
        //ゲートがオープンする。
        m_Animator.speed = 1;
        OpenAudioSource.enabled = true;
    }

    public void ExitGateOpenPause(){
        //ゲートがオープンするのをpauseする。
        m_Animator.speed = 0;
        OpenAudioSource.enabled = false;
    }

    public void OnCanThroughDoor(){
        //アニメーション再生中ですでにゲートを通れるとこを指定
        ThroughPreventCollider.SetActive(false);
    }

    public void OnCompleteDoorOpended(){
        m_Animator.speed = 0;
        this.gameObject.tag = "Untagged";
        this.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        OpenCompletedAudioSource.enabled = true;
        OnCompleteDoorOpen = true;
        OpenCompletedAudioSource.gameObject.SetActive(true);
    }
}
