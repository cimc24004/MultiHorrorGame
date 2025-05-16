using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Unity.Netcode;

public class FootStepSoundController : NetworkBehaviour
{
    public enum MoveStatus { Walk, Run, Crouch, Stop }

    // ↓ 変数名はすべて元のまま
    public float FootStepSoundLevel = 0;
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

    public MoveStatus move_status;

    [Header("Run,Walkの再生する間の時間")]
    public float RunPlayDelay = 0.7f;
    public float WalkPlayDelay = 1f;

    public float PlaySoundDelayTimeNow;

    private string GroundedName;
    private float FootStepCantTimeNow;
    private float FootStepCantTime;

    void Update()
    {
        if (!IsOwner) return;    // オーナーだけ実行

        // ─── サウンドレベル自動切替 ───
        if (AutoFootStepSoundLevelChange)
        {
            if (move_status == MoveStatus.Run)    FootStepSoundLevel = 1;
            else if (move_status == MoveStatus.Walk) FootStepSoundLevel = 0.5f;
            else FootStepSoundLevel = 0;
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

        // ─── 一個しかない足音 ───
        if (OnlyOneFootStepSound)
        {
            if ((move_status == MoveStatus.Run || move_status == MoveStatus.Walk)
                && !audio_source.isPlaying)
            {
                audio_source.Play();
                NotifyFootStepServerRpc("Single", "");
            }
            else if ((move_status == MoveStatus.Stop || move_status == MoveStatus.Crouch)
                     && audio_source.isPlaying)
            {
                audio_source.Stop();
                NotifyFootStepServerRpc("StopSingle", "");
            }
            return;
        }

        if (FootStepSoundLevel == 0) return;

        if (ground_check.OnGround)
        {
            // 地面タグが変わった瞬間
            if (ground_check.GroundedTagName != GroundedName)
            {
                GroundedName = ground_check.GroundedTagName;
                var mode = move_status == MoveStatus.Run ? "Run" : "Walk";
                PlayAndNotify(mode, GroundedName);
            }

            // 間隔ごとに再生
            float delay = (move_status == MoveStatus.Run) ? RunPlayDelay : WalkPlayDelay;
            PlaySoundDelayTimeNow += Time.deltaTime;
            if (PlaySoundDelayTimeNow > delay)
            {
                PlaySoundDelayTimeNow = 0;
                var mode = move_status == MoveStatus.Run ? "Run" : "Walk";
                PlayAndNotify(mode, GroundedName);
            }
        }
        else
        {
            PlaySoundDelayTimeNow = 0;
        }
    }

    // ローカルで再生してからサーバーに通知
    private void PlayAndNotify(string _MoveMode, string groundedName)
    {
        if (!foot_step_controller_status.audio_random_with_grounded_name.ContainsKey(groundedName)) return;
        var list = foot_step_controller_status.audio_random_with_grounded_name[groundedName][_MoveMode];
        if (list.Count == 0) return;

        int r = Random.Range(0, list.Count);
        audio_source.PlayOneShot(list[r]);
        NotifyFootStepServerRpc(_MoveMode, groundedName);
    }

    [Button]
    public void FootStepSoundPlay(string _MoveMode)
    {
        if (!IsOwner) return;
        if (FootStepPlayFromUpdate || OnlyOneFootStepSound || !PlayFootStepFromAnimationEvent) return;
        if (!ground_check.OnGround) return;

        string tagName = ground_check.GroundedTagName;
        if (!foot_step_controller_status.audio_random_with_grounded_name.ContainsKey(tagName)) return;
        var list = foot_step_controller_status.audio_random_with_grounded_name[tagName][_MoveMode];
        if (list.Count == 0) return;

        int r = Random.Range(0, list.Count);
        audio_source.PlayOneShot(list[r]);
        NotifyFootStepServerRpc(_MoveMode, tagName);
    }

    public void FootStepCant(float _time)
    {
        FootStepSoundLevel = 0;
        AutoFootStepSoundLevelChange = false;
        FootStepCantTime = _time;
        FootStepCantTimeNow = 0;
    }

    // クライアント→サーバー通知
    [ServerRpc(RequireOwnership = true)]
    private void NotifyFootStepServerRpc(string moveMode, string groundedName, ServerRpcParams rpcParams = default)
    {
        BroadcastFootStepClientRpc(OwnerClientId, moveMode, groundedName);
    }

    // サーバー→他クライアント通知
    [ClientRpc]
    private void BroadcastFootStepClientRpc(ulong ownerClientId, string moveMode, string groundedName, ClientRpcParams rpcParams = default)
    {
        // オーナーはすでに再生済みなのでスキップ
        if (IsOwner && NetworkManager.Singleton.LocalClientId == ownerClientId) return;

        if (OnlyOneFootStepSound)
        {
            if (moveMode == "Single" && !audio_source.isPlaying) audio_source.Play();
            else if (moveMode == "StopSingle" && audio_source.isPlaying) audio_source.Stop();
            return;
        }

        if (!foot_step_controller_status.audio_random_with_grounded_name.ContainsKey(groundedName)) return;
        var list = foot_step_controller_status.audio_random_with_grounded_name[groundedName][moveMode];
        if (list.Count == 0) return;

        int r = Random.Range(0, list.Count);
        audio_source.PlayOneShot(list[r]);
    }
}
