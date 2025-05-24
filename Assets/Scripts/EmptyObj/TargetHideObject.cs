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
    private PlayerStatus player_status;
    private CrossHairEvent cross_hair_event;
    private AudioSource HideActionAudioSource;
    private AudioDatas HideActionAudioDatas;

    // void Start()
    // {
    //     player_status = PlayerStatus.LocalInstance;
    //     cross_hair_event = player_status.cross_hair_event;
    //     HideActionAudioDatas = player_status.HideActionAudioDatas;
    //     HideActionAudioSource = HideActionAudioDatas.audioSource;
    // }
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

        HideActionAudioDatas  = _playerStatus.HideActionAudioDatas;
        HideActionAudioSource = HideActionAudioDatas.audioSource;
    }

    // プレイヤー側から必ず呼んで初期化する
    public void PlayerSetInit(PlayerStatus _player_status)
    {
        player_status         = _player_status;
        cross_hair_event      = player_status.cross_hair_event;

    }

    // ──────────────────────────────────
    // クライアント→サーバー：隠れるリクエスト
    // ──────────────────────────────────
    [ServerRpc(RequireOwnership = false)]
    public void HideRequestServerRpc(ulong playerNetId)
    {
        // サーバー側でプレイヤーを隠す
        GameObject playerObj = NetworkManager.Singleton.SpawnManager
                               .SpawnedObjects[playerNetId].gameObject;
        HideTarget(playerObj);
        Debug.Log("HideRequestServerRpc");
        // 隠れた本人にだけクライアント処理を飛ばす
        HideForClientsClientRpc(playerNetId);
    }

    // ──────────────────────────────────
    // サーバー→クライアント：隠れた本人向け
    // ──────────────────────────────────
    [ClientRpc]
    private void HideForClientsClientRpc(ulong playerNetId)
    {
        Debug.Log("HideForClientsClientRpc");
        // 呼び出されたクライアントが隠れた本人かチェック
        HideActionAudioSource.PlayOneShot(
            HideActionAudioDatas.audioClipDatasWithKey[AudioClipKey]);
        HideAddEvent.Invoke();
        if (NetworkManager.Singleton.LocalClientId != playerNetId) return;
        // クライアント用処理
        HideTargetForClientsForSender();

    }

    // ──────────────────────────────────
    // クライアント→サーバー：出るリクエスト
    // ──────────────────────────────────
    [ServerRpc(RequireOwnership = false)]
    public void UnHideRequestServerRpc(ulong playerNetId)
    {
        // サーバー側でプレイヤーを出す
        GameObject playerObj = NetworkManager.Singleton.SpawnManager
                               .SpawnedObjects[playerNetId].gameObject;
        UnHideTarget(playerObj);
        // 出た本人にだけクライアント処理を飛ばす
        UnHideForClientsClientRpc(playerNetId);
    }

    // ──────────────────────────────────
    // サーバー→クライアント：出た本人向け
    // ──────────────────────────────────
    [ClientRpc]
    private void UnHideForClientsClientRpc(ulong playerNetId)
    {
        HideActionAudioSource.PlayOneShot(
            HideActionAudioDatas.audioClipDatasWithKey[AudioClipKey]);

        UnHideAddEvent.Invoke();
        if (NetworkManager.Singleton.LocalClientId != playerNetId) return;
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
