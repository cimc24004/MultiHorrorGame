using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ExitGateController : NetworkSingleton<ExitGateController>
{
    private Animator m_Animator;
    private CollideredForPlayers m_players_collidered;
    private PrayController pray_controller;
    [SerializeField] private GameObject ThroughPreventCollider;

    [SerializeField] private NetworkVariable<bool> OnCompleteDoorOpen = new NetworkVariable<bool>();
    private GameProgressController game_progress_controller;
    private float BeforePrayNow = 0;

    [SerializeField] private AudioSource OpenAudioSource;
    [SerializeField] private AudioSource CloseAudioSource;
    [SerializeField] private AudioSource OpenCompletedAudioSource;
    private NetworkVariable<float> AnimatorSpeed = new NetworkVariable<float>();

    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        m_Animator = this.gameObject.GetComponent<Animator>();
        pray_controller = this.gameObject.GetComponent<PrayController>();
        game_progress_controller = GameController.instance.gameObject.GetComponent<GameProgressController>();
        m_players_collidered = this.gameObject.GetComponent<CollideredForPlayers>();
        m_players_collidered.OnTriggerStayEvent.AddListener(() =>
        {
            if (OnCompleteDoorOpen.Value)
            {
                foreach (GameObject collidered_player in m_players_collidered.CollideredObjes)
                {
                    //完全に開いているかつ、ドアに触れていたらクリア
                    NetworkObject collidered_player_networkObject = collidered_player.GetComponent<NetworkObject>();

                    game_progress_controller.ExitGateArrived(collidered_player_networkObject.OwnerClientId);
                }

            }
        });

    }
    void OnEnable() => AnimatorSpeed.OnValueChanged += (_, v) => m_Animator.speed = v;
    void OnDisable() => AnimatorSpeed.OnValueChanged -= (_, v) => m_Animator.speed = v;

    // Update is called once per frame
    void Update()
    {
        if (!OnCompleteDoorOpen.Value == true)
        {
            if (game_progress_controller.CanEscape)
            {
                //すでにサクラの木に納めてるならドアを開けられる
                this.gameObject.tag = "Pray";
                this.gameObject.layer = LayerMask.NameToLayer("Object");
            }
            else
            {
                //サクラの木に納めてないならドアを開けられない
                this.gameObject.tag = "Untagged";
                this.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

            } 
        }

        // if(this.gameObject.name == "Gate"){
        // }
        if (IsServer)
        {
            if (pray_controller.PrayTimeNow.Value == BeforePrayNow)
            {
                ExitGateOpenPause();
            }
            else
            {
                ExitGateOpen();
            }
        }

        BeforePrayNow = pray_controller.PrayTimeNow.Value;
    }

    public void ExitGateOpen()
    {
        //ゲートがオープンする。
        m_Animator.speed = 1;
        OpenAudioSource.enabled = true;
        AnimatorSpeed.Value = 1;
    }

    public void ExitGateOpenPause()
    {
        //ゲートがオープンするのをpauseする。
        m_Animator.speed = 0;
        OpenAudioSource.enabled = false;
        AnimatorSpeed.Value = 0;
    } 

    public void OnCanThroughDoor(){
        //アニメーション再生中ですでにゲートを通れるとこを指定
        ThroughPreventCollider.SetActive(false);
    }

    
    public void OnCompleteDoorOpended(){
        //Animation Eventから実行される サーバーから実行されると思う
        m_Animator.speed = 0;
        this.gameObject.tag = "Untagged";
        this.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        OpenCompletedAudioSource.enabled = true;
        OnCompleteDoorOpen.Value = true;
        OpenCompletedAudioSource.gameObject.SetActive(true);
    }
}
