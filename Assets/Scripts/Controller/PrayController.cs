using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Cinemachine;

public class PrayController : NetworkBehaviour
{
    [Header("Notice用メッセージ")]
    [SerializeField] private string NoticeMessage;
    [Header("必要な祈り時間")]
    public float NeedToPrayTime;

    // ─── ネット同期したい変数 ───
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

    public UnityEvent2 PrayCompletedEvent;
    [SerializeField] private UnityEvent2 PrayStartEvent;

    // ローカル用
    private bool Inited;
    private bool PrayNow;               // サーバー処理用フラグ
    private bool PrayForStartOnce;
    private PlayerStatus player_status;
    private CrossHairStatus cross_hair_status;
    private GameObject GageCanvas;
    private Image GageImageUI;
    private MainNoticedUIController main_noticed_ui_controller;
    private bool once;

    void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InitOnPlayerSpawn;
    }
    void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitOnPlayerSpawn;
    }
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
        // ネットワーク変数の変化を監視
        PrayTimeNow.OnValueChanged += OnPrayTimeNowChanged;
        PrayPermit.OnValueChanged += OnPrayPermitChanged;
        AlreadyPrayCompleted.OnValueChanged += OnAlreadyPrayCompletedChanged;
    }

    void Update()
    {
        if (!Inited) return;

        // 最初だけゲージ非表示
        if (!once)
        {
            GageCanvas.SetActive(false);
            once = true;
        }

        // クロスヘアが祈る対象を向いていたら Notice 表示
        if (cross_hair_status.HitedTag == "Pray"
            && cross_hair_status.RaycastHitedExistsByCrossHairEvent
            && cross_hair_status.HitedObj == this.gameObject)
        {
            GageCanvas.SetActive(true);
            main_noticed_ui_controller.Notice(NoticeMessage, 1);
            once = false;
        }

        // ?? サーバー側の祈り進行処理 ?? 
        if (IsServer && PrayNow && PrayPermit.Value && !AlreadyPrayCompleted.Value)
        {
            GageCanvas.SetActive(true);
            if (!PrayForStartOnce)
            {
                PrayStartEvent.Invoke();
                PrayForStartOnce = true;
            }

            // 祈り時間を増やす
            PrayTimeNow.Value += Time.deltaTime;

            // 完了判定
            if (PrayTimeNow.Value >= NeedToPrayTime)
            {
                // 完了フラグ立て & リセット
                AlreadyPrayCompleted.Value = true;
                PrayPermit.Value = false;
                PrayTimeNow.Value = 0f;

                // 全クライアントに完了イベント通知
                PrayCompletedClientRpc();
                PrayForStartOnce = false;
            }

            PrayNow = false; // 一度だけ処理
        }
    }

    // クライアント側のボタンなどから呼ぶ
    public void Pray()
    {
        if (!IsOwner) return;
        PrayRequestServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void PrayRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        PrayNow = true;
    }

    [ClientRpc]
    private void PrayCompletedClientRpc(ClientRpcParams rpcParams = default)
    {
        // 完了時のUI更新とイベント発火
        GageImageUI.fillAmount = 0f;
        PrayCompletedEvent.Invoke();
        gameObject.tag = "Untagged";
    }

    // ─── ネット変数変更時のハンドラ ───
    private void OnPrayTimeNowChanged(float oldVal, float newVal)
    {
        if (IsOwner)
            GageImageUI.fillAmount = newVal / NeedToPrayTime;
    }
    private void OnPrayPermitChanged(bool oldVal, bool newVal)
    {
        // たとえば許可フラグが off になったら UI を非表示にするとか
        if (IsOwner && !newVal)
            GageCanvas.SetActive(false);
    }
    private void OnAlreadyPrayCompletedChanged(bool oldVal, bool newVal)
    {
        // 既に祈り終わってたら二度と祈れない etc.
    }

    void OnDestroy()
    {
        // 忘れずに解除
        PrayTimeNow.OnValueChanged -= OnPrayTimeNowChanged;
        PrayPermit.OnValueChanged -= OnPrayPermitChanged;
        AlreadyPrayCompleted.OnValueChanged -= OnAlreadyPrayCompletedChanged;
    }
}
