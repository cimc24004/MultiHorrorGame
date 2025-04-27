using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


public class CharacterAudioController : SerializedMonoBehaviour
{
    //キャラクターの行動(追いかける、通常モード)で再生する声とかを変える。
    [System.Serializable]
    private class AudioData
    {
        public List<AudioClip> clips;
        public bool loop;
        [SerializeField] public float AudioPlayInterval;
        [SerializeField] public bool AutoAudioPlayOnUpdate;
    }
    public enum CharacterBehaviorState
    {
        Normal,
        Chase
    }

    [SerializeField] private AudioSource characterAudioSource;
    [SerializeField] private Dictionary<CharacterBehaviorState, AudioData> characterAudioDatas
        = new Dictionary<CharacterBehaviorState, AudioData>();

    public CharacterBehaviorState current_character_behavior_state;


    private float AudioPlayIntervalTimeNow;

    private AudioData currentAudioData;
    void Start()
    {
        this.ChangeCharacterBehaviorState(CharacterAudioController.CharacterBehaviorState.Normal);
    }
    /// <summary>
    /// 指定したステータスの音を再生
    /// </summary>
    void Update(){
        if(currentAudioData != null){
            if(currentAudioData.AutoAudioPlayOnUpdate){
                AudioPlayIntervalTimeNow += Time.deltaTime;
                if(AudioPlayIntervalTimeNow > currentAudioData.AudioPlayInterval){
                    PlayAudio(current_character_behavior_state);

                    AudioPlayIntervalTimeNow = 0;
                }
            }
        }
    }

    public void ChangeCharacterBehaviorState(CharacterBehaviorState state){
        current_character_behavior_state = state;
        if(characterAudioDatas.ContainsKey(current_character_behavior_state)){
            currentAudioData = characterAudioDatas[current_character_behavior_state];
        }
        AudioPlayIntervalTimeNow = 0;
    }
    public void PlayAudio(CharacterBehaviorState state)
    {
        if (!characterAudioDatas.TryGetValue(state, out var data) || characterAudioDatas.Count == 0 || data.clips.Count == 0)
        {
            Debug.LogWarning($"[CharacterAudio] AudioData が見つかんないよ: {state}");
            return;
        }

        characterAudioSource.loop = data.loop;
        int r = Random.Range(0, data.clips.Count);
        characterAudioSource.clip = data.clips[r];
        characterAudioSource.Play();
    }

    /// <summary>
    /// 再生中の音を止める
    /// </summary>
    public void StopAudio()
    {
        characterAudioSource.Stop();
    }

    // もし「Normalだけ」「Chaseだけ」を呼びたいなら、こんなショートカットも追加できる
    public void PlayNormal() => PlayAudio(CharacterBehaviorState.Normal);
    public void PlayChase()  => PlayAudio(CharacterBehaviorState.Chase);
}
