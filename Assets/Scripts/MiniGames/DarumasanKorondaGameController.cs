using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;
using UnityEngine.InputSystem;
using System.Linq;
using Unity.Netcode;
public class DarumasanKorondaGameController : NetworkBehaviour
{
    [SerializeField]
    private CollideredForPlayers collideredForPlayers;

    [SerializeField]
    private bool GamePlayNow;
    private DarumasanKorondaGameData darumasan_koronda_game_data;
    private float TimePassedNowFromVoiceStart;

    private AudioSource audio_source;

    [SerializeField]
    private float VoiceStartFromVoiceEndTime = 1;
    [SerializeField]
    private float VoiceStartFromVoiceEndTimeNow;

    [SerializeField]
    private bool VoiceStart;
    [SerializeField]
    private bool VoiceEnd = true;

    [SerializeField]
    private GameObject DarumasanParent;
    [SerializeField]
    private Transform DefaultLookAtTransform;
    [SerializeField]
    private CharacterMovePermit character_move_permit;
    private Tween DefaultLookTween;
    private Tween LookAtPlayerTween;
    private DiscoveryPlayerController discovery_player_controller;
    private PlayerAllController playerAllController;
    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        playerAllController = PlayerAllController.instance;
        audio_source = this.gameObject.GetComponent<AudioSource>();
        discovery_player_controller = DarumasanParent.GetComponent<DiscoveryPlayerController>();
        darumasan_koronda_game_data = this.gameObject.GetComponent<DarumasanKorondaGameData>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;
        if (collideredForPlayers.CollideredObjes.Count != 0)
        {
            //ゲーム範囲内になったら。
            if (!GamePlayNow)
            {
                GamePlayNow = true;
            }
        }
        else
        {
            //ゲーム範囲外になったら
            GamePlayNow = false;
        }


        if (GamePlayNow)
        {
            for (int i = 0; i < collideredForPlayers.CollideredObjes.Count; i++)
            {
                if (VoiceStartFromVoiceEndTime > VoiceStartFromVoiceEndTimeNow)
                {
                    //Voiceしている最中
                    //こっちを振り返っている間。
                    //プレイヤーが動いたらこっちも動作し始める。
                    if (LookAtPlayerTween != null)
                    {
                        if (!LookAtPlayerTween.IsPlaying())
                        {
                            GameObject player = collideredForPlayers.CollideredObjes[i].gameObject;
                            if (playerAllController.SpawnedPlayersWithPlayerStatuses[player].playerCharacterController.velocity.magnitude > 0)
                            {
                                Debug.Log("ボイス あなたは動きましたね？？");
                                character_move_permit.Move();
                                DOVirtual.DelayedCall(0.1f, () =>
                                {
                                    discovery_player_controller.AddPlayerToDiscoveryPlayerLists(player);
                                });
                                MoveKorondaClientRpc();
                                this.NetworkObject.Despawn(true);
                                KorondaVoiceReset();
                            }
                        }
                    }

                }

            }
            if (VoiceEnd && !VoiceStart)
            {
                VoiceStartFromVoiceEndTimeNow += Time.deltaTime;
                Debug.Log("ボイス VoiceEnd && !VoiceStart");
                Debug.Log("ボイス VoiceStartFromVoiceEndTimeNow: " + VoiceStartFromVoiceEndTimeNow);
                if (VoiceStartFromVoiceEndTime <= VoiceStartFromVoiceEndTimeNow)
                {
                    //こっちを振り返っているのが終わったら
                    DefaultLookTween = DarumasanParent.transform.DODynamicLookAt(DefaultLookAtTransform.position, 0.3f).SetEase(Ease.Linear).SetLink(this.gameObject);
                    Debug.Log("ボイス こっちを見ているのが終わった！");
                    KorondaVoiceStart();
                    VoiceStartFromVoiceEndTimeNow = 0;
                }
                else
                {

                    // Debug.Log("player_character_controllerのMagnitude!?!?:  : " + player_character_controller.velocity.magnitude);

                }
            }

            if (VoiceStart)
            {
                TimePassedNowFromVoiceStart += Time.deltaTime;
                if(TimePassedNowFromVoiceStart > darumasan_koronda_game_data.darumasangakoronda_clips_with_play_time[audio_source.clip])
                {
                    //ボイス発言が終了したら
                    KorondaVoiceEnd();
                    TimePassedNowFromVoiceStart = 0;
                }
            }

        }
        else
        {
            //ゲームプレイ中でなければ再生をストップ
            KorondaVoiceReset();
        }
    }
    private void KorondaVoiceReset()
    {
        audio_source.Stop();
        VoiceEnd = true;
        VoiceStart = false;
        VoiceStartFromVoiceEndTimeNow = VoiceStartFromVoiceEndTime;
        LookAtPlayerTween.Kill();
        DefaultLookTween.Kill();
        TimePassedNowFromVoiceStart = 0;
        //DarumasanParent.transform.DODynamicLookAt(DefaultLookAtTransform.position, 0.3f).SetEase(Ease.Linear).SetLink(this.gameObject);
    }
    private void KorondaVoiceStart()
    {
        int i = Random.Range(0, darumasan_koronda_game_data.darumasangakoronda_clips_with_play_time.Count);
        KorondaVoiceClientRpc(i);
        VoiceStart = true;
        VoiceEnd = false;

    }
    [ClientRpc]
    private void KorondaVoiceClientRpc(int i)
    {
        audio_source.clip = darumasan_koronda_game_data.darumasangakoronda_clips_with_play_time.Keys.ToList()[i];
        audio_source.Play();

    }
    [ClientRpc]
    private void MoveKorondaClientRpc()
    {
        DarumasanParent.transform.SetParent(null);
    }
    private void KorondaVoiceEnd()
    {
        Debug.Log("ボイス終了した");
        int i = 0;
        // Sequence を作成
        var seq = DOTween.Sequence()
            .SetLink(this.gameObject);    // オブジェクト破棄時に自動kill

        // 各プレイヤーを順番に見つめる
        foreach (var idx in collideredForPlayers.CollideredObjes)
        {
            // カメラ位置取得
            var targetPos = playerAllController
                .SpawnedPlayersWithPlayerStatuses[idx]
                .CameraRoot.transform.position;

            // 0.3秒でそのプレイヤーを見つめる tween を追加
            seq.Append(
                LookAtPlayerTween = DarumasanParent.transform
                    .DODynamicLookAt(targetPos, 0.3f)
                    .SetEase(Ease.Linear)
            );

            // 好きな秒数だけ待機（ここでは例として2秒）
            seq.AppendInterval(0.3f);
        }
        // ループ後にフラグ操作
        seq.OnComplete(() =>
        {
            VoiceStart = false;
            VoiceEnd = true;
        });

    }
}
