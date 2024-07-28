using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SoundObjectEventWithEnemySensor : MonoBehaviour
{
    //SoundObjectにつける　敵がこのオブジェクトの音を感知する為のスクリプト。


    //敵が音に反応した。 このenemy_sound_sensor_modeがTrueかつ、
    //EnemyについているSoundEnemyObjectColliderForEventスクリプトのInSoundRangeがTrueならば
    //このオブジェクトについてくる。
    public bool enemy_sound_sensor_mode;


    [Header("音を慣らしておく時間 ")]
    public float SoundPlayingTime = 5;
    [Header("音がなり終えたあとに実行する関数")]
    public UnityEvent SoundStopEvent;
    private float SoundPlayingTimeNow = 0;
    private AudioSource audio_source;

    // Start is called before the first frame update
    void Start()
    {
        audio_source = this.gameObject.GetComponent<AudioSource>();
    }
    private void Update()
    {
        if (enemy_sound_sensor_mode)
        {
            //ある程度時間がたったら音声停止。
            SoundPlayingTimeNow += TimeManager.instance.deltaTime;

            if(SoundPlayingTimeNow > SoundPlayingTime)
            {
                SensorSoundStop();
                SoundStopEvent.Invoke();
                SoundPlayingTimeNow = 0;
            }
        }
    }


    public void SensorSoundPlay()
    {
        //音を鳴らすと同時にenemy_sound_sensor_modeもtrueにする。
        enemy_sound_sensor_mode = true;
        audio_source.Play();
    }
    public void SensorSoundStop()
    {
        enemy_sound_sensor_mode = false;
        audio_source.Stop();
    }

}
