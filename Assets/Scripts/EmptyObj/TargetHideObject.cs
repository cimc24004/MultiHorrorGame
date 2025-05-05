using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TargetHideObject : MonoBehaviour
{
    //どこに隠すか場所を指定！
    public Transform HideBeyondPos;
    //隠れた後その場所から出るときにどこに出るか。
    public Transform UnHideBeyondPos;

    //誰かが隠れているかどうか。
    public bool SomeoneHided;

    //隠れるタイプは何なのか。
    public enum HideType{
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
    void Start()
    {
        player_status = PlayerStatus.instance;
        cross_hair_event = player_status.cross_hair_event;
        HideActionAudioDatas = player_status.HideActionAudioDatas;
        HideActionAudioSource = HideActionAudioDatas.audioSource;
    }
    public void HideTarget(GameObject Target)
    {
        Target.transform.position = HideBeyondPos.transform.position;
        // Target.transform.rotation = 
        // player_camera_controller.enabled = false;
        SomeoneHided = true;
        HideAddEvent.Invoke();
        cross_hair_event.CrossHairEventRaycastPermit = false;
        HideActionAudioSource.PlayOneShot(HideActionAudioDatas.audioClipDatasWithKey[AudioClipKey]);
    }
    public void UnHideTarget(GameObject Target)
    {
        UnHideAddEvent.Invoke();
        HideActionAudioSource.PlayOneShot(HideActionAudioDatas.audioClipDatasWithKey[AudioClipKey]);
        cross_hair_event.CrossHairEventRaycastPermit = true;
        Target.transform.position = UnHideBeyondPos.transform.position;
        SomeoneHided = false;
        // player_camera_controller.enabled = true;
    }
}
