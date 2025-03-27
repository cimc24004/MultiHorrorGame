using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitGateController : MonoBehaviour
{
    private Animator m_Animator;
    private PrayController pray_controller;
    [SerializeField] private GameObject ThroughPreventCollider;

    [SerializeField] private bool OnCompleteDoorOpen;
    private GameProgressController game_progress_controller;
    // Start is called before the first frame update
    void Start()
    {
        m_Animator = this.gameObject.GetComponent<Animator>();
        pray_controller = this.gameObject.GetComponent<PrayController>();
        game_progress_controller = GameProgressController.instance;
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
        if(!pray_controller.PrayNow){
            ExitGateOpenPause();
        }else{
            ExitGateOpen();
        }
    }

    public void ExitGateOpen(){
        //ゲートがオープンする。
        m_Animator.speed = 1;
    }

    public void ExitGateOpenPause(){
        //ゲートがオープンするのをpauseする。
        m_Animator.speed = 0;
    }

    public void OnCanThroughDoor(){
        //アニメーション再生中ですでにゲートを通れるとこを指定
        ThroughPreventCollider.SetActive(false);
    }

    public void OnCompleteDoorOpended(){
        m_Animator.speed = 0;
        this.gameObject.tag = "Untagged";
        this.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        OnCompleteDoorOpen = true;
    }
}
