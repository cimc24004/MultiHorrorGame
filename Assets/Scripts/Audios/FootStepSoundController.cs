using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public class FootStepSoundController : SerializedMonoBehaviour
{
    //足音を鳴らすためだけのスクリプト

    public enum MoveStatus
    {
        Walk,
        Run,
        Crouch,
        Stop
    }
    // 1 = RUn 0.5 = Walk  Crouch = 0 Stop = 0
    public float FootStepSoundLevel = 0;
    public bool AutoFootStepSoundLevelChange = true;

    public AudioSource audio_source;

    [Header("Update関数から足音関数を実行するかどうか(Animation Eventから実行するなら不要)")]
    public bool FootStepPlayFromUpdate;

    [Header("足音は一定で一個しかない場合")]
    public bool OnlyOneFootStepSound;
    [Header("AnimationEventで足音を慣らすかどうか")]
    [SerializeField] private bool PlayFootStepFromAnimationEvent = true;
    [HideIf("OnlyOneFootStepSound")]
    [Header("Groundの名前によって再生するクリップを変える、")]
    public FootStepControllerStatus foot_step_controller_status;
    public GroundCheck ground_check;

    public MoveStatus move_status;
    [Header("Run,Walkの再生する間の時間")]
    public float RunPlayDelay = 0.7f;
    public float WalkPlayDelay = 1;

    public float PlaySoundDelayTimeNow;

    private string GroundedName;
    private float FootStepCantTimeNow;
    private float FootStepCantTime;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (AutoFootStepSoundLevelChange)
        {
            if (move_status == MoveStatus.Run)
            {
                FootStepSoundLevel = 1;
            }
            else if (move_status == MoveStatus.Walk)
            {
                FootStepSoundLevel = 0.5f;
            }
            else if (move_status == MoveStatus.Stop || move_status == MoveStatus.Crouch)
            {
                FootStepSoundLevel = 0;
            }
        }
        else
        {
            FootStepCantTimeNow += Time.deltaTime;
            if (FootStepCantTime < FootStepCantTimeNow)
            {
                AutoFootStepSoundLevelChange = true;
                FootStepCantTimeNow = 0;
            }
        }

        if (!FootStepPlayFromUpdate) return;

        if (OnlyOneFootStepSound)
        {
            if(move_status == MoveStatus.Walk || move_status == MoveStatus.Run)
            {
                if (!audio_source.isPlaying)
                {
                    Debug.Log("再生しました！");
                    audio_source.Play();
                }
            }
            else
            {
                if (audio_source.isPlaying)
                {
                    audio_source.Stop();

                }
            }

            return;
        }
        if (FootStepSoundLevel == 0) return;
        if (ground_check.OnGround)
        {
            if(ground_check.GroundedTagName != GroundedName)
            {
                if (move_status == MoveStatus.Run)
                {
                    FootStepSoundPlayFromUpdate("Run");
                }
                else if (move_status == MoveStatus.Walk)
                {

                    FootStepSoundPlayFromUpdate("Walk");
                }
            }
            if (move_status == MoveStatus.Run)
            {
                PlaySoundDelayTimeNow += Time.deltaTime;
                if (PlaySoundDelayTimeNow > RunPlayDelay)
                {
                    FootStepSoundPlayFromUpdate("Run");
                    PlaySoundDelayTimeNow = 0;
                }
            }
            else if (move_status == MoveStatus.Walk)
            {
                PlaySoundDelayTimeNow += Time.deltaTime;
                if (PlaySoundDelayTimeNow > WalkPlayDelay)
                {
                    FootStepSoundPlayFromUpdate("Walk");
                    PlaySoundDelayTimeNow = 0;
                }
            }
            else
            {
                PlaySoundDelayTimeNow = 0;
            }
        }
        else
        {
            PlaySoundDelayTimeNow = 0;
        }
    }
    [Button]
    public void FootStepSoundPlay(string _MoveMode)
    {
        Debug.Log("aaaaaaaaaaaaaananananananananFootStepSoundPLay");
        if (FootStepSoundLevel == 0) return;
        if (FootStepPlayFromUpdate || OnlyOneFootStepSound || !PlayFootStepFromAnimationEvent) return;
        //GroundedNameによって音を変える
        GroundedName = ground_check.GroundedTagName;
        if (!ground_check.OnGround) return;
        if (foot_step_controller_status.audio_random_with_grounded_name.ContainsKey(GroundedName))
        {
            int Count = foot_step_controller_status.audio_random_with_grounded_name[GroundedName][_MoveMode].Count;
            int r = Random.Range(0, Count);
            audio_source.PlayOneShot(foot_step_controller_status.audio_random_with_grounded_name[GroundedName][_MoveMode][r]);
        }
    }

    public void FootStepSoundPlayFromUpdate(string _MoveMode)
    {
        GroundedName = ground_check.GroundedTagName;
        if (FootStepSoundLevel == 0) return;
        if (!ground_check.OnGround) return;
        if (foot_step_controller_status.audio_random_with_grounded_name.ContainsKey(GroundedName))
        {
            int Count = foot_step_controller_status.audio_random_with_grounded_name[GroundedName][_MoveMode].Count;
            int r = Random.Range(0, Count);
            audio_source.PlayOneShot(foot_step_controller_status.audio_random_with_grounded_name[GroundedName][_MoveMode][r]);
        }
    }

    public void FootStepCant(float _time)
    {
        this.FootStepSoundLevel = 0;
        this.AutoFootStepSoundLevelChange = false;
        this.FootStepCantTime = _time;
        this.FootStepCantTimeNow = 0;
    }
}
