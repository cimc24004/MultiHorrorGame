using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OpenPermitByKeyController : NetworkBehaviour
{
    //鍵によって開けるコントローラー 鍵つきのドアにしたい場合だけこのスクリプトをアタッチ。
    public NetworkVariable<bool> OpenPermit = new NetworkVariable<bool>(false);
    private PlayerStatus player_status;
    private SubNoticedUIController sub_noticed_ui_controller;
    private CrossHairEvent cross_hair_event;
    private AudioDatas key_audio_datas;
    private AudioSource KeyAudioSource;

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
        player_status = _playerStatus;
        cross_hair_event = player_status.cross_hair_event;
        sub_noticed_ui_controller = player_status.sub_noticed_ui_controller;
        key_audio_datas = cross_hair_event.KeyAudioDatas;
        KeyAudioSource = key_audio_datas.audioSource;
    }
    public override void OnNetworkSpawn()
    {


    }
    [ServerRpc(RequireOwnership = false)]
    public void KeyUseRequestServerRpc(ServerRpcParams serverRpcParams = default)
    {
        this.OpenPermit.Value = true;

        // リクエストを送ったクライアントIDを取得
        ulong clientId = serverRpcParams.Receive.SenderClientId;

        // そのクライアントにだけ通知を送る
        NotifyKeyOpenedClientRpc(new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        });
    }
    [ClientRpc]
    private void NotifyKeyOpenedClientRpc(ClientRpcParams clientRpcParams = default)
    {
        // 鍵を使った人だけにUIを表示
        if (sub_noticed_ui_controller != null)
        {
            sub_noticed_ui_controller.Notice("鍵が開いた");
        }

        if (KeyAudioSource != null && key_audio_datas != null)
        {
            KeyAudioSource.PlayOneShot(key_audio_datas.audioClipDatasWithKey["OpenedKey"]);
        }
    }
    [ClientRpc]
    public void NeedKeyForPlayerClientRpc(ClientRpcParams clientRpcParams = default)
    {
        //TheDoorControllerから実行される。
        sub_noticed_ui_controller.Notice("鍵がかかっている");
        KeyAudioSource.PlayOneShot(key_audio_datas.audioClipDatasWithKey["LockedKey"]);
    }
}
