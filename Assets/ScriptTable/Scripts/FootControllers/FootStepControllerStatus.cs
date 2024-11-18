using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
[CreateAssetMenu(fileName = "FootStepControllerStatus", menuName = "ScriptableObjects/FootStepControllerStatus")]

public class FootStepControllerStatus : SerializedScriptableObject
{

    public enum MoveStatus
    {
        Walk,
        Run,
        Crouch,
        Stop
    }
    [Header("Groundの名前によって再生するクリップを変える、")]
    public Dictionary<string, Dictionary<string, List<AudioClip>>> audio_random_with_grounded_name;

    public MoveStatus move_status;
    [Header("Run,Walkの再生する間の時間")]
    public float RunPlayDelay = 0.7f;
    public float WalkPlayDelay = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
