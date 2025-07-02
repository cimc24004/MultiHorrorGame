using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SisenAsobiDousiController : NetworkBehaviour
{
    // ────────────────────────────────────────────────────────────────────
    // ★ 追加：ビジュアル同期用の列挙体
    private enum BodyState : byte { Normal = 0, Chase = 1 }

    // ★ 追加：直近に同期した値を保存しておく変数
    private byte _syncedState = 255;   // 255 = 未同期
    private float _syncedBlendWeight = -1f;
    private bool _syncedScream = false;
    // ────────────────────────────────────────────────────────────────────

    private DiscoveryPlayerController discovery_player_controller;
    private FollowObjectByNav follow_object_by_nav;
    private EnemyRoutingController enemy_routing_controller;
    private HeadLookatTargetController head_look_at_target_controller;
    [SerializeField]
    private Material NormalBodyMaterial;
    [SerializeField]
    private Material ChasePlayerBodyMaterial;
    [SerializeField]
    private PlayerNearSensor player_near_sensor;
    [SerializeField]
    private SkinnedMeshRenderer HumanKaradaMeshRenderer;
    [SerializeField]
    private AudioSource ScreamAudioSource;
    [SerializeField]
    private bool MouthOpenNow;
    [SerializeField]
    private float LookNowTime = 2;
    [SerializeField]
    private float LookNowTimeNow = 0;
    [SerializeField]
    private float MouthOpenTimeNow = 0;

    private Vector3 BeforePosition;
    private EnemyStatus enemy_status;
    private EnemyToPlayerKillController enemy_to_player_kill_controller;
    private CharacterController character_controller;
    private bool _once_look;
    [SerializeField]
    private bool previousFlinchNow = false;
    [SerializeField]
    private bool PermitMouthOpen = true;

    private URPFxController urp_fx_controller;
    private EnemyFlinchController enemy_flinch_controller;

    private EnemySoundSensor enemy_sound_sensor;
    private Material BeforeHumanKaradaMaterial;
    private GameObject HeadLookAtTargetControllerPlayer;
    private PlayerAllController playerAllController;
    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        urp_fx_controller = GameControllerOnAllScene.instance.gameObject.GetComponent<URPFxController>();
        if (!IsServer) return;
        discovery_player_controller = this.gameObject.GetComponent<DiscoveryPlayerController>();
        head_look_at_target_controller = this.gameObject.GetComponent<HeadLookatTargetController>();
        enemy_routing_controller = this.gameObject.GetComponent<EnemyRoutingController>();
        BeforePosition = this.transform.position;
        enemy_sound_sensor = this.gameObject.GetComponent<EnemySoundSensor>();
        enemy_to_player_kill_controller = this.gameObject.GetComponent<EnemyToPlayerKillController>();
        enemy_status = this.gameObject.GetComponent<EnemyStatus>();
        playerAllController = PlayerAllController.instance;
        character_controller = this.gameObject.GetComponent<CharacterController>();
        follow_object_by_nav = this.gameObject.GetComponent<FollowObjectByNav>();
        enemy_flinch_controller = this.gameObject.GetComponent<EnemyFlinchController>();
        player_near_sensor.IsNearToKillEffect = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;
        if (!previousFlinchNow && enemy_flinch_controller.FlinchNow)
        {
            ResetValue();
            PermitMouthOpen = false;
            head_look_at_target_controller.enabled = false;
        }
        else if (previousFlinchNow && !enemy_flinch_controller.FlinchNow)
        {
            PermitMouthOpen = true;
            enemy_routing_controller.PermitFunction(true);
            head_look_at_target_controller.enabled = true;
        }
        // フレームの最後に、今回の値を previousFlinchNow に格納して次回比較に使う
        previousFlinchNow = enemy_flinch_controller.FlinchNow;


        if (discovery_player_controller.DiscoveryPlayerNowCheck() && !MouthOpenNow)
        {

            HumanKaradaMeshRenderer.SetBlendShapeWeight(0, 200);
        }
        if (MouthOpenNow || discovery_player_controller.DiscoveryPlayerNowCheck())
        {
            if (BeforeHumanKaradaMaterial != ChasePlayerBodyMaterial)
            {
                HumanKaradaMeshRenderer.material = ChasePlayerBodyMaterial;
            }
            BeforeHumanKaradaMaterial = ChasePlayerBodyMaterial;
        }
        else
        {
            if (!BeforeHumanKaradaMaterial != NormalBodyMaterial)
            {
                HumanKaradaMeshRenderer.material = NormalBodyMaterial;
            }
            BeforeHumanKaradaMaterial = NormalBodyMaterial;
        }
        if (!PermitMouthOpen)
        {
            TrySyncAppearance();   // ★ 追加：同期チェック
            return;
        }
        if (!discovery_player_controller.DiscoveryPlayerNowCheck())
        {
            player_near_sensor.IsNearToKillEffect = false;
            if (!MouthOpenNow)
            {
                if (head_look_at_target_controller.LookAtTargetNow)
                {
                    //近くに敵がいるなら という判定はhead_look_at_target_controllerがtargetを見てるかどうかで判定する。
                    //follow_object_by_nav.ChaseToTargetSet(true, BeforePosition, 0);
                    HeadLookAtTargetControllerPlayer = head_look_at_target_controller.Player;
                    enemy_routing_controller.PermitFunction(false);
                    LookNowTimeNow += Time.deltaTime;
                    enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Stop;
                    enemy_to_player_kill_controller.enabled = false;
                    character_controller.Move(Vector3.zero);
                    follow_object_by_nav.MovePermit = false;
                    //見ている間の時間が何秒か経ったら
                    if (LookNowTimeNow > LookNowTime)
                    {
                        //口を開ける 口を開けるtrueに
                        MouthOpenNow = true;
                        HumanKaradaMeshRenderer.SetBlendShapeWeight(0, 200);
                        ScreamAudioSource.Play();
                        LookNowTimeNow = 0;
                    }
                    _once_look = false;
                }
                else
                {
                    HumanKaradaMeshRenderer.SetBlendShapeWeight(0, 0);
                    if (!_once_look)
                    {
                        enemy_routing_controller.PermitFunction(true);
                        follow_object_by_nav.MovePermit = true;
                        LookNowTimeNow = 0;
                        _once_look = true;
                    }

                }
            }
        }
        else
        {
        }


        //口が開いているtrueなら 何秒か経った後に襲ってくる。
        if (MouthOpenNow)
        {

            MouthOpenTimeNow += Time.deltaTime;
            if (MouthOpenTimeNow > 1)
            {
                enemy_routing_controller.PermitFunction(true);
                enemy_to_player_kill_controller.enabled = true;
                follow_object_by_nav.MovePermit = true;
                enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Walk;
                discovery_player_controller.AddPlayerToDiscoveryPlayerLists(HeadLookAtTargetControllerPlayer);
                // 送りたいクライアント ID を並べる（ulong で書くこと）
                ulong[] targetIds = {playerAllController.SpawnedPlayersWithPlayerStatuses[HeadLookAtTargetControllerPlayer].NetworkObject.OwnerClientId};   // 例

                var rpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = targetIds
                    }
                };
                InteferencesChangeRequestForLookClientRpc(false);
                player_near_sensor.IsNearToKillEffect = true;
                MouthOpenNow = false;
                MouthOpenTimeNow = 0;
            }
            else
            {
                follow_object_by_nav.MovePermit = false;
                // 送りたいクライアント ID を並べる（ulong で書くこと）
                ulong[] targetIds = {playerAllController.SpawnedPlayersWithPlayerStatuses[HeadLookAtTargetControllerPlayer].NetworkObject.OwnerClientId};   // 例

                var rpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = targetIds
                    }
                };
                InteferencesChangeRequestForLookClientRpc(true);
                enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Stop;
            }

        }

        // ────────────────────────────────────────────────────────────
        // ★ 追加：毎フレーム最後に「見た目が変わったか」チェックして
        //          変わっていれば各クライアントへ RPC を送る
        TrySyncAppearance();
        // ────────────────────────────────────────────────────────────
    }

    // ────────────────────────────────────────────────────────────────
    // ★ 追加：状態変化を検出して同期するメソッド
    private void TrySyncAppearance()
    {
        // ★ renderer.sharedMaterial で正しく一致判定
        byte nowState = (HumanKaradaMeshRenderer.sharedMaterial == ChasePlayerBodyMaterial)
                              ? (byte)BodyState.Chase
                              : (byte)BodyState.Normal;

        float nowBlend = HumanKaradaMeshRenderer.GetBlendShapeWeight(0);
        bool nowScreamPlay = ScreamAudioSource.isPlaying;

        // 何か変わっていたら RPC 送信
        if (nowState != _syncedState ||
            Mathf.Abs(nowBlend - _syncedBlendWeight) > 0.1f ||
            nowScreamPlay != _syncedScream)
        {
            _syncedState = nowState;
            _syncedBlendWeight = nowBlend;
            _syncedScream = nowScreamPlay;

            SyncAppearanceClientRpc(nowState, nowBlend, nowScreamPlay);
        }
    }

    // ★ 追加：クライアントへ送る RPC
    [ClientRpc]
    private void SyncAppearanceClientRpc(byte state, float blendWeight, bool playScream)
    {
        // ★ 無条件に material をセット（1 行で確実に反映させる）
        HumanKaradaMeshRenderer.material =
            (state == (byte)BodyState.Chase) ? ChasePlayerBodyMaterial : NormalBodyMaterial;

        // ブレンドシェイプ
        HumanKaradaMeshRenderer.SetBlendShapeWeight(0, blendWeight);

        // 悲鳴
        if (playScream && !ScreamAudioSource.isPlaying)
        {
            ScreamAudioSource.Play();
        }
    }
    // ────────────────────────────────────────────────────────────────

    private void ResetValue()
    {
        MouthOpenNow = false;
        // 送りたいクライアント ID を並べる（ulong で書くこと）
        ulong[] targetIds = {playerAllController.SpawnedPlayersWithPlayerStatuses[HeadLookAtTargetControllerPlayer].NetworkObject.OwnerClientId};   // 例

        var rpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = targetIds
            }
        };
        InteferencesChangeRequestForLookClientRpc(false);
        // head_look_at_target_controller.LookAtTargetNow = false;
        enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Stop;
        LookNowTimeNow = 0;

        MouthOpenTimeNow = 0;
    }
    [ClientRpc]
    private void InteferencesChangeRequestForLookClientRpc
    (
        bool _change,
        ClientRpcParams rpcParams = default   // ← 追加
    )
    {
        urp_fx_controller.InteferecesChange(_change);
    }
}
