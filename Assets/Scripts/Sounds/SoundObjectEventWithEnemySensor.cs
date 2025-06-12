using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class SoundObjectEventWithEnemySensor : NetworkBehaviour
{
    //SoundObjectにつける　敵がこのオブジェクトの音を感知する為のスクリプト。


    //敵が音に反応した。 このenemy_sound_sensor_modeがTrueかつ、
    //EnemyについているSoundEnemyObjectColliderForEventスクリプトのInSoundRangeがTrueならば
    //このオブジェクトについてくる。
    public NetworkVariable<bool> enemy_sound_sensor_mode = new NetworkVariable<bool>();


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
        if (IsServer)
        {
            if (enemy_sound_sensor_mode.Value)
            {
                //ある程度時間がたったら音声停止。
                SoundPlayingTimeNow += Time.deltaTime;

                if (SoundPlayingTimeNow > SoundPlayingTime)
                {
                    SensorSoundPlayForRequestClientRpc();
                    SoundPlayingTimeNow = 0;
                }
            }
        }

    }

    void OnDestroy()
    {
        SensorSoundStop();
    }
    [ClientRpc]
    public void SensorSoundPlayForRequestClientRpc()
    {
        //ClientRpcから実行される予定。
        //音を鳴らすと同時にenemy_sound_sensor_modeもtrueにする。
        Debug.Log("SensorSoundPlay!!!!!!");
        EnemySoundSensorModeRequestServerRpc(true);
        audio_source.Play();
    }
    public void SensorSoundStop()
    {
        EnemySoundSensorModeRequestServerRpc(false);
        SoundStopEvent.Invoke();
        audio_source.Stop();
    }

    [ServerRpc(RequireOwnership = false)]
    private void EnemySoundSensorModeRequestServerRpc(bool _Value)
    {
        enemy_sound_sensor_mode.Value = _Value;
    }
}
