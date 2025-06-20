using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Cinemachine;

public class PrayController : NetworkBehaviour
{
    [Header("Prayするときに表示するメッセージ")]
    [SerializeField] private string NoticeMessage;
    [Header("祈るのに必要な時間")]
    public float NeedToPrayTime;

    // ������ �l�b�g�����������ϐ� ������
    public NetworkVariable<float> PrayTimeNow = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public NetworkVariable<bool> PrayPermit = new NetworkVariable<bool>(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public NetworkVariable<bool> AlreadyPrayCompleted = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    [Header("祈りコンプリートEvent 全クライアントに共有")]
    public UnityEvent2 PrayCompletedEvent;
    [Header("祈りコンプリートEvent サーバー側でのみ実行される")]
    public UnityEvent2 PrayCompletedEventForServer;
    [SerializeField] private UnityEvent2 PrayStartEvent;

    // ���[�J���p
    private bool Inited;
    private NetworkVariable<bool> PrayNow = new NetworkVariable<bool>();
    private bool PrayForStartOnce;
    private bool NotPrayForStartOnce;
    private PlayerStatus player_status;
    private CrossHairStatus cross_hair_status;
    private GameObject GageCanvas;
    private Image GageImageUI;
    private MainNoticedUIController main_noticed_ui_controller;
    //?????????????
    private bool PushingAction = false;
    private bool once;
    void OnEnable() => PlayerStatus.OnLocalPlayerSpawned += InitOnPlayerSpawn;
    void OnDisable() => PlayerStatus.OnLocalPlayerSpawned -= InitOnPlayerSpawn;
    void InitOnPlayerSpawn(PlayerStatus _playerStatus)
    {
        player_status = _playerStatus;
        cross_hair_status = player_status.cross_hair_event.cross_hair_status;
        GageCanvas = player_status.GageCanvas;
        GageImageUI = player_status.GageImage;
        main_noticed_ui_controller = player_status.main_noticed_ui_controller;

        Inited = true;
        
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            PrayTimeNow.OnValueChanged         += OnPrayTimeNowChanged;
            PrayPermit.OnValueChanged          += OnPrayPermitChanged;
        }

        AlreadyPrayCompleted.OnValueChanged += OnAlreadyPrayCompletedChanged;
    }

    void Update()
    {
        if (!Inited) return;

        if (!once)
        {
            GageCanvas.SetActive(false);
            once = true;
        }
        if (cross_hair_status.HitedTag == "Pray" &&
            cross_hair_status.RaycastHitedExistsByCrossHairEvent &&
            cross_hair_status.HitedObj == this.gameObject)
        {
            //Pray?????????????
            GageImageUI.fillAmount = PrayTimeNow.Value / NeedToPrayTime;
            GageCanvas.SetActive(true);
            main_noticed_ui_controller.Notice(NoticeMessage, 1);
            once = false;
            NotPrayForStartOnce = false;
        }
        else
        {
            if (PushingAction)
            {
                //???????????
                if (PrayNow.Value)
                {
                    //???Pray??????? PrayNow?false?? ??
                    if (!NotPrayForStartOnce)
                    {
                        PrayNowFalseRequestServerRpc();
                        NotPrayForStartOnce = true;
                    }
                }
            }
        }

        if (IsServer && PrayNow.Value && PrayPermit.Value && !AlreadyPrayCompleted.Value)
        {
            if (!PrayForStartOnce)
            {
                PrayStartEvent.Invoke();
                PrayForStartOnce = true;
            }

            PrayTimeNow.Value += Time.deltaTime;

            if (PrayTimeNow.Value >= NeedToPrayTime)
            {

                PrayCompleted();
                
                PrayForStartOnce = false;
            }
        }
        if (PushingAction)
        {
            PushingAction = false;
        }
    }
    public void Pushing()
    {
        PushingAction = true;
    }
    [ServerRpc(RequireOwnership = false)]
    public void PrayNowTrueRequestServerRpc()
    {
        PrayNow.Value = true;
    }
    [ServerRpc(RequireOwnership = false)]
    public void PrayNowFalseRequestServerRpc()
    {
        PrayNow.Value = false;
    }
    [ClientRpc]
    private void PrayCompletedClientRpc(ClientRpcParams rpcParams = default)
    {
        GageImageUI.fillAmount = 0f;
        //???????????????????
        PrayCompletedEvent.Invoke();
        this.gameObject.tag = "Untagged";
    }
    private void PrayCompleted()
    {
        PrayCompletedEventForServer.Invoke();
        AlreadyPrayCompleted.Value = true;
        PrayPermit.Value = false;
        PrayTimeNow.Value = 0f;
        PrayCompletedClientRpc();
    }
    private void OnPrayTimeNowChanged(float _, float newVal)
    {
    }
    private void OnPrayPermitChanged(bool _, bool newVal)
    {
        if (IsOwner && !newVal) GageCanvas.SetActive(false);
    }
    private void OnAlreadyPrayCompletedChanged(bool _, bool __) { }

    void OnDestroy()
    {
        PrayTimeNow.OnValueChanged         -= OnPrayTimeNowChanged;
        PrayPermit.OnValueChanged          -= OnPrayPermitChanged;
        AlreadyPrayCompleted.OnValueChanged -= OnAlreadyPrayCompletedChanged;
    }
}
