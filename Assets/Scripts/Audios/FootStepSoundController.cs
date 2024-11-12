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
    public AudioSource audio_source;

    [Header("Groundの名前によって再生するクリップを変える、")]

    public FootStepControllerStatus foot_step_controller_status;
    public GroundCheck ground_check;

    public MoveStatus move_status;
    [Header("Run,Walkの再生する間の時間")]
    public float RunPlayDelay = 0.7f;
    public float WalkPlayDelay = 1;

    public float PlaySoundDelayTimeNow;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (ground_check.OnGround)
        {
            if(move_status == MoveStatus.Run)
            {
                PlaySoundDelayTimeNow += Time.deltaTime;
                if(PlaySoundDelayTimeNow > RunPlayDelay)
                {
                    FootStepSoundPlay(ground_check.GroundedTagName, "Run");
                    PlaySoundDelayTimeNow = 0;
                }
            }
            else if(move_status == MoveStatus.Walk)
            {
                PlaySoundDelayTimeNow += Time.deltaTime;
                if (PlaySoundDelayTimeNow > WalkPlayDelay)
                {
                    FootStepSoundPlay(ground_check.GroundedTagName, "Walk");
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
    public void FootStepSoundPlay(string _GroundedName, string _MoveMode)
    {
        //GroundedNameによって音を変える
        if (this.gameObject.name == "泣き霊 (1)")
            Debug.Log("GroundedName: ? " + _GroundedName);
        if (foot_step_controller_status.audio_random_with_grounded_name.ContainsKey(_GroundedName))
        {
            int Count = foot_step_controller_status.audio_random_with_grounded_name[_GroundedName][_MoveMode].Count;
            int r = Random.Range(0, Count);
            audio_source.PlayOneShot(foot_step_controller_status.audio_random_with_grounded_name[_GroundedName][_MoveMode][r]);
            if(this.gameObject.name == "泣き霊 (1)")
            {
                Debug.Log(foot_step_controller_status.audio_random_with_grounded_name[_GroundedName][_MoveMode][r] + "を再生するよおお");
            }
        }
    }
}
