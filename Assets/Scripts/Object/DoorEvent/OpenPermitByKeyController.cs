using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenPermitByKeyController : MonoBehaviour
{
    //鍵によって開けるコントローラー 鍵つきのドアにしたい場合だけこのスクリプトをアタッチ。
    public bool OpenPermit = false;
    public AudioSource KeyAudioSource;
    [Header("キーを使った時の音")]
    public AudioClip KeyUsedAudioClip;
    [Header("鍵がかかっている時の音")]
    public AudioClip NeedKeyAudioClip;

    private PlayerStatus player_status;
    private SubNoticedUIController sub_noticed_ui_controller;
    private void Start()
    {
        player_status = PlayerStatus.instance;
        sub_noticed_ui_controller = player_status.sub_noticed_ui_controller;
    }
    public void KeyUse()
    {
        sub_noticed_ui_controller.Notice("鍵が開いた");
        this.OpenPermit = true;

        KeyAudioSource.PlayOneShot(KeyUsedAudioClip);
    }

    public void NeedKeyForPlayer()
    {
        sub_noticed_ui_controller.Notice("鍵がかかっている");

        KeyAudioSource.PlayOneShot(NeedKeyAudioClip);
    }
}
