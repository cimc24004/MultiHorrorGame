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
    
    // 同期用の変数
    private NetworkVariable<float> syncedVoiceStartTime = new NetworkVariable<float>();
    private NetworkVariable<int> currentClipIndex = new NetworkVariable<int>(-1);
    private NetworkVariable<bool> isLookingAtPlayers = new NetworkVariable<bool>();
    private NetworkVariable<int> currentLookTargetIndex = new NetworkVariable<int>(-1);
    private Sequence lookSequence;
    private HorrorDirector horrorDirector;
    public override void OnNetworkSpawn()
    {
        audio_source = this.gameObject.GetComponent<AudioSource>();
        darumasan_koronda_game_data = this.gameObject.GetComponent<DarumasanKorondaGameData>();
        
        if (!IsServer) 
        {
            // クライアント側での同期監視
            syncedVoiceStartTime.OnValueChanged += OnVoiceStartTimeChanged;
            currentClipIndex.OnValueChanged += OnClipIndexChanged;
            isLookingAtPlayers.OnValueChanged += OnLookingStateChanged;
            currentLookTargetIndex.OnValueChanged += OnLookTargetChanged;
            
            return;
        }
        playerAllController = PlayerAllController.instance;
        collideredForPlayers.OnCollideredAddedObjectEvent.AddListener((t) =>
        {
            PlayerStatus playerStatus = playerAllController.SpawnedPlayersWithPlayerStatuses[t];
            ulong[] targets = new ulong[] { playerStatus.NetworkObject.OwnerClientId }; // 1,5,9 は送りたいクライアントID

            // ClientRpcParams を組み立てる
            var rpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = targets
                }
            };
            OnColliderEventClientRpc(true, rpcParams);
            KorondaVolumeVoiceClientRpc(1f, rpcParams);
        });
        collideredForPlayers.OnCollideredRemovedObjectEvent.AddListener((t) =>
        {
            PlayerStatus playerStatus = playerAllController.SpawnedPlayersWithPlayerStatuses[t];
            ulong[] targets = new ulong[] { playerStatus.NetworkObject.OwnerClientId }; // 1,5,9 は送りたいクライアントID
            var rpcParams = new ClientRpcParams {
                Send = new ClientRpcSendParams {
                    TargetClientIds = targets
                }
            };
            OnColliderEventClientRpc(false,rpcParams);
            KorondaVolumeVoiceClientRpc(0f, rpcParams);
        });
        discovery_player_controller = DarumasanParent.GetComponent<DiscoveryPlayerController>();
    }

    void Update()
    {
        if (!IsServer) 
        {
            HandleClientSideSync();
            return;
        }
        
        HandleServerSideLogic();
    }

    private void HandleServerSideLogic()
    {
        if (collideredForPlayers.CollideredObjes.Count != 0)
        {
            if (!GamePlayNow)
            {
                GamePlayNow = true;
            }
        }
        else
        {
            GamePlayNow = false;
        }

        if (GamePlayNow)
        {
            CheckPlayerMovement();
            HandleVoiceTimers();
        }
        else
        {
            KorondaVoiceReset();
        }
    }

    private void HandleClientSideSync()
    {
        // クライアント側での同期処理
        if (currentClipIndex.Value >= 0 && VoiceStart)
        {
            float elapsedTime = Time.time - syncedVoiceStartTime.Value;
            var clipsList = darumasan_koronda_game_data.darumasangakoronda_clips_with_play_time.Keys.ToList();
            
            if (elapsedTime > darumasan_koronda_game_data.darumasangakoronda_clips_with_play_time[clipsList[currentClipIndex.Value]])
            {
                // 音声終了をサーバーに通知
                RequestVoiceEndServerRpc();
            }
        }
    }

    private void CheckPlayerMovement()
    {
        for (int i = 0; i < collideredForPlayers.CollideredObjes.Count; i++)
        {
            if (VoiceStartFromVoiceEndTime > VoiceStartFromVoiceEndTimeNow)
            {
                if (isLookingAtPlayers.Value)
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
                        foreach (GameObject g in collideredForPlayers.CollideredObjes)
                        {
                            collideredForPlayers.OnCollideredRemovedObjectEvent.Invoke(g);
                        }
                        DarumasanParent.transform.SetParent(null);
                        this.NetworkObject.Despawn(true);
                        KorondaVoiceReset();
                    }
                }
            }
        }
    }

    private void HandleVoiceTimers()
    {
        if (VoiceEnd && !VoiceStart)
        {
            VoiceStartFromVoiceEndTimeNow += Time.deltaTime;
            
            if (VoiceStartFromVoiceEndTime <= VoiceStartFromVoiceEndTimeNow)
            {
                StartDefaultLookAnimation();
                KorondaVoiceStart();
                VoiceStartFromVoiceEndTimeNow = 0;
            }
        }

        if (VoiceStart)
        {
            TimePassedNowFromVoiceStart += Time.deltaTime;
            var clipsList = darumasan_koronda_game_data.darumasangakoronda_clips_with_play_time.Keys.ToList();
            
            if (TimePassedNowFromVoiceStart > darumasan_koronda_game_data.darumasangakoronda_clips_with_play_time[clipsList[currentClipIndex.Value]])
            {
                KorondaVoiceEnd();
                TimePassedNowFromVoiceStart = 0;
            }
        }
    }

    private void StartDefaultLookAnimation()
    {
        StartDefaultLookClientRpc();
    }

    [ClientRpc]
    private void StartDefaultLookClientRpc()
    {
        DefaultLookTween = DarumasanParent.transform.DODynamicLookAt(DefaultLookAtTransform.position, 0.3f)
            .SetEase(Ease.Linear)
            .SetLink(this.gameObject);
    }

    private void KorondaVoiceReset()
    {
        KorondaVoiceStopClientRpc();
        VoiceEnd = true;
        VoiceStart = false;
        VoiceStartFromVoiceEndTimeNow = VoiceStartFromVoiceEndTime;
        TimePassedNowFromVoiceStart = 0;
        
        // NetworkVariableをリセット
        syncedVoiceStartTime.Value = 0;
        currentClipIndex.Value = -1;
        isLookingAtPlayers.Value = false;
        currentLookTargetIndex.Value = -1;
        
        StopAllTweensClientRpc();
    }

    [ClientRpc]
    private void KorondaVoiceStopClientRpc()
    {
        audio_source.Stop();
    }

    [ClientRpc]
    private void StopAllTweensClientRpc()
    {
        LookAtPlayerTween?.Kill();
        DefaultLookTween?.Kill();
        lookSequence?.Kill();
    }

    private void KorondaVoiceStart()
    {
        int i = Random.Range(0, darumasan_koronda_game_data.darumasangakoronda_clips_with_play_time.Count);
        
        // 同期用の値を設定
        syncedVoiceStartTime.Value = Time.time;
        currentClipIndex.Value = i;
        
        KorondaVoiceClientRpc(i);
        VoiceStart = true;
        VoiceEnd = false;
    }

    [ClientRpc]
    private void KorondaVoiceClientRpc(int i)
    {
        var clipsList = darumasan_koronda_game_data.darumasangakoronda_clips_with_play_time.Keys.ToList();
        audio_source.clip = clipsList[i];
        audio_source.Play();
    }
    [ClientRpc]
    private void KorondaVolumeVoiceClientRpc(float _volume, ClientRpcParams clientRpcParams)
    {
        audio_source.volume = _volume;
        
    }
    private void KorondaVoiceEnd()
    {
        Debug.Log("ボイス終了した");
        isLookingAtPlayers.Value = true;
        StartLookSequenceClientRpc();
    }

    [ClientRpc]
    private void StartLookSequenceClientRpc()
    {
        StartCoroutine(LookAtPlayersSequence());
    }
    [ClientRpc]
    private void OnColliderEventClientRpc(bool _collider, ClientRpcParams clientRpcParams)
    {
        // ulong clientId = clientRpcParams.Send.TargetClientIds[0];
        // GameObject Player = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerNetworkObjectId].gameObject;
        horrorDirector = HorrorDirector.instance;
        if (_collider)
        {
            horrorDirector.DarumasanGame(true);
        }
        else
        {
            horrorDirector.DarumasanGame(false);
        }
    }
    private IEnumerator LookAtPlayersSequence()
    {
        lookSequence = DOTween.Sequence().SetLink(this.gameObject);
        
        for (int i = 0; i < collideredForPlayers.CollideredObjes.Count; i++)
        {
            if (IsServer)
            {
                currentLookTargetIndex.Value = i;
            }
            
            var targetPos = playerAllController
                .SpawnedPlayersWithPlayerStatuses[collideredForPlayers.CollideredObjes[i]]
                .CameraRoot.transform.position;

            LookAtPlayerTween = DarumasanParent.transform
                .DODynamicLookAt(targetPos, 0.3f)
                .SetEase(Ease.Linear);
            
            yield return new WaitForSeconds(0.3f);
            yield return new WaitForSeconds(0.3f);
        }
        
        if (IsServer)
        {
            VoiceStart = false;
            VoiceEnd = true;
            isLookingAtPlayers.Value = false;
            currentLookTargetIndex.Value = -1;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestVoiceEndServerRpc()
    {
        if (VoiceStart)
        {
            KorondaVoiceEnd();
            TimePassedNowFromVoiceStart = 0;
        }
    }
    // NetworkVariable変更時のコールバック
    private void OnVoiceStartTimeChanged(float oldValue, float newValue)
    {
        // 必要に応じて処理
    }

    private void OnClipIndexChanged(int oldValue, int newValue)
    {
        // 必要に応じて処理
    }

    private void OnLookingStateChanged(bool oldValue, bool newValue)
    {
        // 必要に応じて処理
    }

    private void OnLookTargetChanged(int oldValue, int newValue)
    {
        // 必要に応じて処理
    }
}