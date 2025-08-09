using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PrayController : NetworkBehaviour
{
    [Header("Prayするときに表示するメッセージ")]
    [SerializeField] public string NoticeMessage = "祈る（長押し）";

    [Header("祈るのに必要な時間(秒)")]
    [SerializeField] public float NeedToPrayTime = 3f;

    // 進行状況（サーバー書き込み / 全員読み取り）
    public NetworkVariable<float> PrayTimeNow = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // 祈れるかどうか（サーバー書き込み / 全員読み取り）
    public NetworkVariable<bool> PrayPermit = new NetworkVariable<bool>(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // 既に完了済みか（サーバー書き込み / 全員読み取り）
    public NetworkVariable<bool> AlreadyPrayCompleted = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [Header("祈りコンプリートEvent（全クライアントで発火）")]
    public UnityEvent2 PrayCompletedEvent;

    [Header("祈りコンプリートEvent（サーバーのみ発火）")]
    public UnityEvent2 PrayCompletedEventForServer;

    [Header("祈り開始時Event（サーバーのみ最初の一回だけ発火）")]
    [SerializeField] private UnityEvent2 PrayStartEvent;

    // 入力状態（サーバー書き込み / 全員読み取り）
    [SerializeField] private NetworkVariable<bool> PrayNow = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // ↓ ローカル参照（UI表示のため）
    private bool inited;
    private PlayerStatus playerStatus;
    private CrossHairStatus crossHairStatus;
    private GameObject gageCanvas;
    private Image gageImageUI;
    private MainNoticedUIController noticeUI;

    // 内部フラグ
    private bool prayStartInvoked; // サーバー側：開始イベントを一度だけ

    /*━━━━━━━━ ライフサイクル ━━━━━━━━*/
    private void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InitOnPlayerSpawn;
        if (PlayerStatus.LocalInstance != null)
        {
            InitOnPlayerSpawn(PlayerStatus.LocalInstance);
        }
    }

    private void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitOnPlayerSpawn;
    }

    private void InitOnPlayerSpawn(PlayerStatus _playerStatus)
    {
        playerStatus   = _playerStatus;
        crossHairStatus = playerStatus.cross_hair_event.cross_hair_status;
        gageCanvas     = playerStatus.GageCanvas;
        gageImageUI    = playerStatus.GageImage;
        noticeUI       = playerStatus.main_noticed_ui_controller;
        inited = true;

        // 最初は非表示
        if (gageCanvas != null) gageCanvas.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        // クライアント側でもUI閉じたいので誰でもハンドラを付ける
        PrayPermit.OnValueChanged += OnPrayPermitChanged;
        AlreadyPrayCompleted.OnValueChanged += OnAlreadyPrayCompletedChanged;
    }

    private void OnDestroy()
    {
        PrayPermit.OnValueChanged -= OnPrayPermitChanged;
        AlreadyPrayCompleted.OnValueChanged -= OnAlreadyPrayCompletedChanged;
    }

    /*━━━━━━━━ Update ━━━━━━━━*/
    private void Update()
    {
        if (!inited) return;

        // 自分のクロスヘアが当たっている時だけゲージとメッセージを表示
        // ① このPrayを見ているか？
        bool isLookingThis =
            crossHairStatus != null &&
            crossHairStatus.RaycastHitedExistsByCrossHairEvent &&
            crossHairStatus.HitedTag == "Pray" &&
            crossHairStatus.HitedObj == this.gameObject;

        // ② どれかのPrayを見ているか？（非表示制御専用）
        bool lookingAtAnyPray =
            crossHairStatus != null &&
            crossHairStatus.RaycastHitedExistsByCrossHairEvent &&
            crossHairStatus.HitedTag == "Pray";

        if (!lookingAtAnyPray)
        {
            gageCanvas.SetActive(false);
        }
        // ---- UI制御 ----
        if (isLookingThis)
        {
            if (gageCanvas != null) gageCanvas.SetActive(true);

            if (gageImageUI != null)
            {
                float denom = Mathf.Max(NeedToPrayTime, 0.0001f); // 0割り防止
                gageImageUI.fillAmount = Mathf.Clamp01(PrayTimeNow.Value / denom);
            }

            if (noticeUI != null && PrayPermit.Value && !AlreadyPrayCompleted.Value)
            {
                noticeUI.Notice(NoticeMessage, 1);
            }
        }
        else
        {
            // 別のPrayを見ている間は「触らない」。何も見てない時だけ閉じる。
            if (!lookingAtAnyPray && gageCanvas != null)
            {
                gageCanvas.SetActive(false);
            }
        }



        // ★ サーバーだけ：押下中は毎フレーム時間を進める（RPC頻度に依存しない）
        if (IsServer && PrayNow.Value && PrayPermit.Value && !AlreadyPrayCompleted.Value)
        {
            if (!prayStartInvoked)
            {
                PrayStartEvent?.Invoke();
                prayStartInvoked = true;
            }

            PrayTimeNow.Value += Time.deltaTime;

            if (PrayTimeNow.Value >= NeedToPrayTime)
            {
                CompletePrayServer();
            }
        }
    }

    /*━━━━━━━━ API ━━━━━━━━*/
    public void PrayReset()
    {
        if (!IsServer) return;

        prayStartInvoked = false;
        PrayNow.Value = false;
        PrayTimeNow.Value = 0f;
        AlreadyPrayCompleted.Value = false;
        PrayPermit.Value = true;
    }

    /*━━━━━━━━ ServerRpc ━━━━━━━━*/
    [ServerRpc(RequireOwnership = false)]
    public void PrayNowTrueRequestServerRpc()
    {
        if (!PrayPermit.Value || AlreadyPrayCompleted.Value) return;
        PrayNow.Value = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void PrayNowFalseRequestServerRpc()
    {
        PrayNow.Value = false;
        // 押し直した時に開始イベントが再度出るように
        prayStartInvoked = false;
    }

    /*━━━━━━━━ 内部（サーバー） ━━━━━━━━*/
    private void CompletePrayServer()
    {
        PrayCompletedEventForServer?.Invoke();

        AlreadyPrayCompleted.Value = true;
        PrayPermit.Value = false;
        PrayNow.Value = false;
        PrayTimeNow.Value = 0f;
        prayStartInvoked = false;

        // 全クライアントで完了演出
        PrayCompletedClientRpc();
    }

    [ClientRpc]
    private void PrayCompletedClientRpc(ClientRpcParams rpcParams = default)
    {
        if (gageImageUI != null) gageImageUI.fillAmount = 0f;
        PrayCompletedEvent?.Invoke();
        // 以降ターゲットにならないように
        gameObject.tag = "Untagged";
    }

    /*━━━━━━━━ 変更通知 ━━━━━━━━*/
    private void OnPrayPermitChanged(bool _, bool newVal)
    {
        if (!newVal && gageCanvas != null)
        {
            gageCanvas.SetActive(false);
        }
    }

    private void OnAlreadyPrayCompletedChanged(bool oldVal, bool newVal)
    {
        // 必要ならここでローカル演出
    }
}
