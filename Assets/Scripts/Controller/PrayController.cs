using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Sirenix.OdinInspector;
public class PrayController : NetworkBehaviour
{
    [Header("Prayするときに表示するメッセージ")]
    [SerializeField] public string NoticeMessage = "祈る（長押し）";

    [Header("祈るのに必要な時間(秒)")]
    [SerializeField] public float NeedToPrayTime = 3f;
    [SerializeField, ShowInInspector]public NetworkList<ulong> PrayNowPlayers = new NetworkList<ulong>();
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
    [SerializeField]
    private NetworkVariable<bool> PrayNow = new NetworkVariable<bool>(
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
        playerStatus = _playerStatus;
        crossHairStatus = playerStatus.cross_hair_event.cross_hair_status;
        gageCanvas = playerStatus.GageCanvas;
        gageImageUI = playerStatus.GageImage;
        noticeUI = playerStatus.main_noticed_ui_controller;
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
            float SpeedPerExtraPlayer = 0.1f; // 追加1人ごとに+10%
            int n = (PrayNowPlayers != null) ? PrayNowPlayers.Count : 0;

            float factor;
            if (n == 0)       factor = 0f;                         // 誰も祈ってない→進まない
            else if (n == 1)  factor = 1f;                         // 1人→等速
            else              factor = 1f + (n - 1) * SpeedPerExtraPlayer; // 2人以上→加速

            PrayTimeNow.Value += Time.deltaTime * factor;

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
    public void PrayNowTrueRequestServerRpc(ulong _requestClientId)
    {
        if (!PrayPermit.Value || AlreadyPrayCompleted.Value) return;
        PrayNowPlayersSet(_requestClientId, true);
        RefreshPrayNowFlag();
    }

    [ServerRpc(RequireOwnership = false)]
    public void PrayNowFalseRequestServerRpc(ulong _requestClientId)
    {
        PrayNowPlayersSet(_requestClientId, false);
        RefreshPrayNowFlag();
        if (!PrayNow.Value) prayStartInvoked = false; // 全員離したら次回開始イベントを再解放
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
        PrayNowPlayers?.Clear(); 
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
    private void PrayNowPlayersSet(ulong clientId, bool _set)
    {
        //サーバー側で実行
        if (!IsServer) return;
        if (!_set)
        {
            if (PrayNowPlayers.Contains(clientId))
            {
                PrayNowPlayers.Remove(clientId);
            }
        }
        else
        {
            if (!PrayNowPlayers.Contains(clientId))
            {
                PrayNowPlayers.Add(clientId);
            }
        }

    }
    private void RefreshPrayNowFlag()
    {
        PrayNow.Value = PrayNowPlayers != null && PrayNowPlayers.Count > 0;
    }

}
