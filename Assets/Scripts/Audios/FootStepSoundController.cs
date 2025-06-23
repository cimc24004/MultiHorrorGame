// FootStepSoundController.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Unity.Netcode;

public class FootStepSoundController : NetworkBehaviour
{
    // 足音を鳴らすためだけのスクリプト

    public enum MoveStatus
    {
        Walk,
        Run,
        Crouch,
        Stop
    }

    // 1 = Run 0.5 = Walk  Crouch/Stop = 0
    public NetworkVariable<float> FootStepSoundLevel = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public bool AutoFootStepSoundLevelChange = true;

    public AudioSource audio_source;

    [Header("Update関数から足音関数を実行するかどうか(Animation Eventから実行するなら不要)")]
    public bool FootStepPlayFromUpdate;

    [Header("足音は一定で一個しかない場合")]
    public bool OnlyOneFootStepSound;
    [Header("AnimationEventで足音を鳴らすかどうか")]
    [SerializeField] private bool PlayFootStepFromAnimationEvent = true;
    [HideIf("OnlyOneFootStepSound")]
    [Header("Groundの名前によって再生するクリップを変える、")]
    public FootStepControllerStatus foot_step_controller_status;
    public GroundCheck ground_check;

    public NetworkVariable<MoveStatus> move_status = new NetworkVariable<MoveStatus>(MoveStatus.Stop, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [Header("Run,Walkの再生する間の時間")]
    public float RunPlayDelay = 0.7f;
    public float WalkPlayDelay = 1f;

    public float PlaySoundDelayTimeNow;

    private string GroundedName;
    private float FootStepCantTimeNow;
    private float FootStepCantTime;
    private bool HasFootstepAuthority
    {
        get
        {
            return IsOwner || (IsServer && !IsOwner);
        }
    }
    

    void Update()
    {

        // ★重要★ オーナーが状態を管理し、サーバーRPCで通知する
        if (IsOwner)
        {
            // PlayerStatusから移動状態を取得（オーナーが最新状態を持っている）
            // UpdateMoveStatusFromPlayerStatus();

            // レベル自動切替
            if (AutoFootStepSoundLevelChange)
            {
                if (move_status.Value == MoveStatus.Run)
                    FootStepSoundLevel.Value = 1;
                else if (move_status.Value == MoveStatus.Walk)
                    FootStepSoundLevel.Value = 0.5f;
                else // Crouch or Stop
                    FootStepSoundLevel.Value = 0;
            }
            else
            {
                FootStepCantTimeNow += Time.deltaTime;
                if (FootStepCantTimeNow > FootStepCantTime)
                {
                    AutoFootStepSoundLevelChange = true;
                    FootStepCantTimeNow = 0;
                }
            }

            if (!FootStepPlayFromUpdate) return;

            // 一個しかない足音
            if (OnlyOneFootStepSound)
            {
                if ((move_status.Value == MoveStatus.Run || move_status.Value == MoveStatus.Walk)
                    && !audio_source.isPlaying)
                {
                    audio_source.Play();
                    NotifyFootStepServerRpc("Single", "");
                }
                else if ((move_status.Value == MoveStatus.Stop || move_status.Value == MoveStatus.Crouch)
                         && audio_source.isPlaying)
                {
                    audio_source.Stop();
                    NotifyFootStepServerRpc("StopSingle", "");
                }
                return;
            }

            if (FootStepSoundLevel.Value == 0) return;

            if (ground_check.OnGround)
            {
                // 地面タグ変化した瞬間
                if (ground_check.GroundedTagName != GroundedName)
                {
                    GroundedName = ground_check.GroundedTagName;
                    var mode = (move_status.Value == MoveStatus.Run) ? "Run" : "Walk";
                    PlayLocalAndNotify(mode, GroundedName);
                }

                // 間隔ごと
                float delay = (move_status.Value == MoveStatus.Run) ? RunPlayDelay : WalkPlayDelay;
                PlaySoundDelayTimeNow += Time.deltaTime;
                if (PlaySoundDelayTimeNow > delay)
                {
                    PlaySoundDelayTimeNow = 0;
                    var mode = (move_status.Value == MoveStatus.Run) ? "Run" : "Walk";
                    PlayLocalAndNotify(mode, GroundedName);
                }
            }
            else
            {
                PlaySoundDelayTimeNow = 0;
            }
        }
    }



    [Button]
    public void FootStepSoundPlay(string _MoveMode)
    {
        if (!HasFootstepAuthority) return;
        if (FootStepPlayFromUpdate || OnlyOneFootStepSound || !PlayFootStepFromAnimationEvent) return;
        if (!ground_check.OnGround) return;

        var tagName = ground_check.GroundedTagName;
        if (!foot_step_controller_status.audio_random_with_grounded_name.ContainsKey(tagName)) return;
        var list = foot_step_controller_status.audio_random_with_grounded_name[tagName][_MoveMode];
        if (list.Count == 0) return;

        int r = Random.Range(0, list.Count);
        audio_source.PlayOneShot(list[r]);
        NotifyFootStepServerRpc(_MoveMode, tagName);
    }

    public void FootStepCant(float _time)
    {
        FootStepSoundLevel.Value = 0;
        AutoFootStepSoundLevelChange = false;
        FootStepCantTime = _time;
        FootStepCantTimeNow = 0;
    }

    // ローカル再生＋サーバー通知
    private void PlayLocalAndNotify(string moveMode, string tagName)
    {
        Debug.Log("PlayLocalAndNotify-------------------------moveMode: " + moveMode + "tagName: " + tagName);
        if (!foot_step_controller_status.audio_random_with_grounded_name.ContainsKey(tagName)) return;
        var list = foot_step_controller_status.audio_random_with_grounded_name[tagName][moveMode];
        if (list.Count == 0) return;
        int r = Random.Range(0, list.Count);
        audio_source.PlayOneShot(list[r]);
        // オーナーからサーバーRPCで通知
        // NPC（サーバー所有）ならサーバーがそのままブロードキャスト
        if (IsServer && !IsOwner)
        {
            // NPC：サーバーが直接全クライアントへ
            BroadcastFootStepClientRpc(moveMode, tagName);
        }
        else if (IsOwner)
        {
            // プレイヤー所有：クライアント→サーバー→全体
            NotifyFootStepServerRpc(moveMode, tagName);
        }
    }

    // ★重要★ オーナーからサーバーへの通知
    [ServerRpc]
    private void NotifyFootStepServerRpc(string moveMode, string groundedName)
    {
        // サーバーから全クライアントへブロードキャスト
        BroadcastFootStepClientRpc(moveMode, groundedName);
    }

    [ClientRpc]
    private void BroadcastFootStepClientRpc(string moveMode, string groundedName)
    {
        // 自分のクライアント（音を既に再生済み）では再生しない
        if (IsOwner) return;

        if (OnlyOneFootStepSound)
        {
            if (moveMode == "Single" && !audio_source.isPlaying) 
                audio_source.Play();
            else if (moveMode == "StopSingle" && audio_source.isPlaying) 
                audio_source.Stop();
            return;
        }

        if (!foot_step_controller_status.audio_random_with_grounded_name.ContainsKey(groundedName)) return;
        var list = foot_step_controller_status.audio_random_with_grounded_name[groundedName][moveMode];
        if (list.Count == 0) return;

        int r = Random.Range(0, list.Count);
        audio_source.PlayOneShot(list[r]);
    }
}