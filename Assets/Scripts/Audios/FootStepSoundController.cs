using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public class FootStepSoundController : SerializedMonoBehaviour
{
    //足音を鳴らすためだけのスクリプト
    public AudioSource audio_source;

    [Header("Groundの名前によって再生するクリップを変える、")]
    public Dictionary<string, Dictionary<string,List<AudioClip>>> audio_random_with_grounded_name;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [Button]
    public void FootStepSoundPlay(string _GroundedName, string _MoveMode)
    {
        //GroundedNameによって音を変える
        if (audio_random_with_grounded_name.ContainsKey(_GroundedName))
        {
            int Count = audio_random_with_grounded_name[_GroundedName][_MoveMode].Count;
            int r = Random.Range(0, Count);
            Debug.Log("Random R: " + r);
            audio_source.PlayOneShot(audio_random_with_grounded_name[_GroundedName][_MoveMode][r]);

        }
    }
}
