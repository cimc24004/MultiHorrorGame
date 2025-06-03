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
    private void InitOnPlayerSpawn(PlayerStatus _playerStatus){
        player_status = _playerStatus;
        cross_hair_event = player_status.cross_hair_event;
        sub_noticed_ui_controller = player_status.sub_noticed_ui_controller;
        key_audio_datas = cross_hair_event.KeyAudioDatas;
        KeyAudioSource = key_audio_datas.audioSource;
    }
    public override void OnNetworkSpawn(){


    }
    [ServerRpc(RequireOwnership = false)]
    public void KeyUseRequestServerRpc()
    {
        sub_noticed_ui_controller.Notice("鍵が開いた");
        this.OpenPermit.Value = true;
        KeyAudioSource.PlayOneShot(key_audio_datas.audioClipDatasWithKey["OpenedKey"]);
    }

    public void NeedKeyForPlayer()
    {
        sub_noticed_ui_controller.Notice("鍵がかかっている");
        KeyAudioSource.PlayOneShot(key_audio_datas.audioClipDatasWithKey["LockedKey"]);
    }
}
