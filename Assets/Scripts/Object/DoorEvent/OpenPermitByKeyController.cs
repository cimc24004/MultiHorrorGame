using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OpenPermitByKeyController : NetworkBehaviour
{
    //鍵によって開けるコントローラー 鍵つきのドアにしたい場合だけこのスクリプトをアタッチ。
    public bool OpenPermit = false;
    private PlayerStatus player_status;
    private SubNoticedUIController sub_noticed_ui_controller;
    private CrossHairEvent cross_hair_event;
    private AudioDatas key_audio_datas;
    private AudioSource KeyAudioSource;
    private void Start()
    {
        // player_status = PlayerStatus.LocalInstance;
        // cross_hair_event = player_status.cross_hair_event;
        // sub_noticed_ui_controller = player_status.sub_noticed_ui_controller;
        // key_audio_datas = cross_hair_event.KeyAudioDatas;
        // KeyAudioSource = key_audio_datas.audioSource;
    }
    public override void OnNetworkSpawn(){
        player_status = PlayerStatus.LocalInstance;
        cross_hair_event = player_status.cross_hair_event;
        sub_noticed_ui_controller = player_status.sub_noticed_ui_controller;
        key_audio_datas = cross_hair_event.KeyAudioDatas;
        KeyAudioSource = key_audio_datas.audioSource;
    }
    public void KeyUse()
    {
        sub_noticed_ui_controller.Notice("鍵が開いた");
        this.OpenPermit = true;
        KeyAudioSource.PlayOneShot(key_audio_datas.audioClipDatasWithKey["OpenedKey"]);
    }

    public void NeedKeyForPlayer()
    {
        sub_noticed_ui_controller.Notice("鍵がかかっている");
        KeyAudioSource.PlayOneShot(key_audio_datas.audioClipDatasWithKey["LockedKey"]);
    }
}
