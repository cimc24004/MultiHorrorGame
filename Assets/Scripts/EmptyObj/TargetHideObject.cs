using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;
using Mono.Cecil.Cil;

public class TargetHideObject : NetworkBehaviour
{
    //どこに隠すか場所を指定！
    public Transform HideBeyondPos;
    //隠れた後その場所から出るときにどこに出るか。
    public Transform UnHideBeyondPos;

    //誰かが隠れているかどうか。
    public NetworkVariable<bool> SomeoneHided = new NetworkVariable<bool>();

    //隠れるタイプは何なのか。
    public enum HideType
    {
        Stand,
        Crouch,
        FaceDown
    };
    [Header("隠れるタイプ")]
    [Header("Stand: 立った状態 \n Crouch: 座った状態 \n FaceDown: うつ伏せ状態")]
    public HideType hide_type;
    public Vector3 TargetObjectOfAngle;
    [Header("Playerの子オブジェクトにあるAudioDatasに設定されてるaudioClipのKeyを指定。")]
    [SerializeField] private string AudioClipKey;
    [SerializeField] private UnityEvent2 HideAddEvent;
    [SerializeField] private UnityEvent2 UnHideAddEvent;
    [SerializeField] private UnityEvent2 LocalHideAddEvent;
    [SerializeField] private UnityEvent2 LocalUnHideAddEvent;
    private CrossHairEvent cross_hair_event;
    private AudioSource HideActionAudioSource;
    private AudioDatas HideActionAudioDatas;

    void Start()
    {

    }
    void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InitOnPlayerSpawn;
    }
    void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitOnPlayerSpawn;
    }

    private void InitOnPlayerSpawn(PlayerStatus _playerStatus)
    {
        LocalUnHideAddEvent.AddListener(() =>
        {
            Debug.Log("LocalUnHideAddEvent");
        });
        LocalHideAddEvent.AddListener(() =>
        {
            Debug.Log("LocalHideAddEvent");
        });
        HideActionAudioDatas = _playerStatus.HideActionAudioDatas;
        HideActionAudioSource = HideActionAudioDatas.audioSource;
        cross_hair_event      = _playerStatus.cross_hair_event;
    }

    // ──────────────────────────────────
    // クライアント→サーバー：隠れるリクエスト
    // ──────────────────────────────────
    [ServerRpc(RequireOwnership = false)]
    public void HideRequestServerRpc(ulong playerNetId)
    {
        // サーバー側でプレイヤーを隠す
        NetworkObject playerNetworkObj = NetworkManager.Singleton.SpawnManager
                               .SpawnedObjects[playerNetId];
        HideTarget(playerNetworkObj.gameObject);
        Debug.Log("HideRequestServerRpc");
        // 隠れた本人にだけクライアント処理を飛ばす
        HideForClientsClientRpc(playerNetworkObj.OwnerClientId);
    }

    // ──────────────────────────────────
    // サーバー→クライアント：隠れた本人向け
    // ──────────────────────────────────
    [ClientRpc]
    private void HideForClientsClientRpc(ulong _PlayerOwnerClientId)
    {
        Debug.Log("HideForClientsClientRpc");
        // 呼び出されたクライアントが隠れた本人かチェック
        HideActionAudioSource.PlayOneShot(
            HideActionAudioDatas.audioClipDatasWithKey[AudioClipKey]);
        HideAddEvent.Invoke();
        if (NetworkManager.Singleton.LocalClientId != _PlayerOwnerClientId) return;
        // クライアント用処理
        Debug.Log("NetworkManager.Singleton.LocalClientId == playerNetId");
        LocalHideAddEvent.Invoke();
        HideTargetForClientsForSender();
    }

    // ──────────────────────────────────
    // クライアント→サーバー：出るリクエスト
    // ──────────────────────────────────
    [ServerRpc(RequireOwnership = false)]
    public void UnHideRequestServerRpc(ulong playerNetId)
    {
        // サーバー側でプレイヤーを出す
        NetworkObject playerNetworkObj = NetworkManager.Singleton.SpawnManager
                               .SpawnedObjects[playerNetId];
        UnHideTarget(playerNetworkObj.gameObject);
        // 出た本人にだけクライアント処理を飛ばす
        UnHideForClientsClientRpc(playerNetworkObj.OwnerClientId);
    }

    // ──────────────────────────────────
    // サーバー→クライアント：出た本人向け
    // ──────────────────────────────────
    [ClientRpc]
    private void UnHideForClientsClientRpc(ulong _PlayerOwnerClientId)
    {
        HideActionAudioSource.PlayOneShot(
            HideActionAudioDatas.audioClipDatasWithKey[AudioClipKey]);
        
        UnHideAddEvent.Invoke();
        if (NetworkManager.Singleton.LocalClientId != _PlayerOwnerClientId) return;
        LocalUnHideAddEvent.Invoke();
        UnHideTargetForClientsForSender();
    }

    // ──────────────────────────────────
    // サーバーで動かすメソッド
    // ──────────────────────────────────
    public void HideTarget(GameObject Target)
    {
        // サーバーでのみ実行
        Target.transform.position = HideBeyondPos.transform.position;
        SomeoneHided.Value        = true;
    }
    public void UnHideTarget(GameObject Target)
    {
        // サーバーでのみ実行
        Target.transform.position = UnHideBeyondPos.transform.position;
        SomeoneHided.Value        = false;
    }

    // ──────────────────────────────────
    // クライアント用：音とUI、Raycast停止
    // ──────────────────────────────────
    public void HideTargetForClientsForSender()
    {
        cross_hair_event.CrossHairEventRaycastPermit = false;
    }
    public void UnHideTargetForClientsForSender()
    {
        cross_hair_event.CrossHairEventRaycastPermit = true;
    }
}
