using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;
using UnityEngine.InputSystem;
using System.Linq;
public class DarumasanKorondaGameController : SerializedMonoBehaviour
{
    [SerializeField]
    private Collidered GameRangeCollidered;

    [SerializeField]
    private bool GamePlayNow;

    [SerializeField]
    private Dictionary<AudioClip,float> darumasangakoronda_clips_with_play_time;
    [SerializeField]
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
    private PlayerStatus player_status;
    private CharacterController player_character_controller;
    [SerializeField]
    private CharacterMovePermit character_move_permit;
    private Tween DefaultLookTween;
    private Tween LookAtPlayerTween;
    private PlayerInput player_input;
    private GameControllerOnAllScene game_controller_on_all_scene;
    private DiscoveryPlayerController discovery_player_controller;
    private InputAction look_action;
    // Start is called before the first frame update

    void Start()
    {
        audio_source = this.gameObject.GetComponent<AudioSource>();
        discovery_player_controller = DarumasanParent.GetComponent<DiscoveryPlayerController>();
        player_status = PlayerStatus.instance;
        game_controller_on_all_scene = GameControllerOnAllScene.instance;
        player_input = game_controller_on_all_scene.player_input;
        player_character_controller = player_status.gameObject.GetComponent<CharacterController>();
        look_action = player_input.currentActionMap["Look"];
    }

    // Update is called once per frame
    void Update()
    {
        if (GameRangeCollidered.Collider)
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
                    //こっちを振り返っている間。
                    //プレイヤーが動いたらこっちも動作し始める。
                    Debug.Log("このときの視点のMagnitude!?!?:  : " + look_action.ReadValue<Vector2>().magnitude);
                    Debug.Log("player_character_controllerのMagnitude!?!?:  : " + player_character_controller.velocity.magnitude);
                    if (player_character_controller.velocity.magnitude > 0 || look_action.ReadValue<Vector2>().magnitude > 1f)
                    {
                        Debug.Log("ボイス あなたは動きましたね？？");
                        DarumasanParent.transform.SetParent(null);
                        character_move_permit.Move();
                        discovery_player_controller.DiscoveryPlayerNow = true;
                        this.gameObject.SetActive(false);
                        KorondaVoiceReset();
                    }
                }
            }
            if (VoiceStart)
            {
                TimePassedNowFromVoiceStart += Time.deltaTime;
                if(TimePassedNowFromVoiceStart > darumasangakoronda_clips_with_play_time[audio_source.clip])
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
        int i = Random.Range(0, darumasangakoronda_clips_with_play_time.Count);
        audio_source.clip = darumasangakoronda_clips_with_play_time.Keys.ToList()[i];

        audio_source.Play();
        VoiceStart = true;
        VoiceEnd = false;

    }
    private void KorondaVoiceEnd()
    {
        Debug.Log("ボイス終了した");
        LookAtPlayerTween = DarumasanParent.transform.DODynamicLookAt(player_status.CameraRoot.transform.position, 0.3f).SetEase(Ease.Linear).SetLink(this.gameObject).OnComplete(() => {
            VoiceStart = false;
            VoiceEnd = true;
        });



    }
}
