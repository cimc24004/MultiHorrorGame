using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEnemyObjectColliderForEvent : MonoBehaviour
{
    //敵につける音センサーイベントスクリプト(プレイヤーの足音以外のアイテムから出る音など)

    //音が鳴るアイテムに音センサーコライダーをつけ、Enemyにもセンサーコライダーをつける。
    //こうすることで高さにも対応したセンサー機能ようなのができる。
    public bool NoticedSound;
    private bool InSoundRange;
    private SoundObjectEventWithEnemySensor sound_object_event_with_sensor;
    private EnemySoundSensor enemy_sound_sensor;
    private EnemyStatus enemy_status;
    private FollowObjectByNav follow_object_by_nav;
    private bool once;

    private bool before_enemy_sound_sensor_mode = false;
    // Start is called before the first frame update
    void Start()
    {
        enemy_sound_sensor = this.gameObject.GetComponent<EnemySoundSensor>();
        enemy_status = this.gameObject.GetComponent<EnemyStatus>();
        follow_object_by_nav = this.gameObject.GetComponent<FollowObjectByNav>();

    }

    // Update is called once per frame
    void Update()
    {
        
        //(敵のセンサー範囲内かつ音範囲内)
        if (InSoundRange)
        {
            //敵が音に気づいた場合。
            if (sound_object_event_with_sensor.enemy_sound_sensor_mode)
            {
                //SoundObject(非常ベルや投擲アイテム)の音がなっている最中ならば
                //enemy_sound_sensor.ArriveSoundPlaceWaitTimeToReturnNow = 0;
                enemy_sound_sensor.SoundSeonsor(sound_object_event_with_sensor.gameObject.transform.position, enemy_sound_sensor.sound_sensor_status.SpeedRunWhenRealizeSound, true);
                enemy_sound_sensor.sound_by_obj = true;


            }
            before_enemy_sound_sensor_mode = sound_object_event_with_sensor.enemy_sound_sensor_mode;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "SoundObjectRange")
        {
            sound_object_event_with_sensor = other.transform.parent.gameObject.GetComponent<SoundObjectEventWithEnemySensor>();


            //sound_object_event_with_sensor.SoundStopEvent.AddListener(() =>
            //{
            //    //音が終了したらsound_by_objをfalseにします。
            //    Debug.Log("soundobject_event_with_sensorのSoundStopEventが実行したああああ");
            //    enemy_sound_sensor.sound_by_obj = false;
            //}); 
            
            InSoundRange = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "SoundObjectRange")
        {
            InSoundRange = false;
        }
    }
}
