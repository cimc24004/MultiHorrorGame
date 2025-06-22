using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class SoundObjectEventWithEnemySensor : NetworkBehaviour
{
    public NetworkVariable<bool> enemy_sound_sensor_mode = new NetworkVariable<bool>();

    [Header("音を鳴らし続ける時間(s)")]
    public float SoundPlayingTime = 5f;
    [Header("音が鳴り終わったあとに実行する関数")]
    public UnityEvent SoundStopEvent;
    [Header("サーバー側で実行する関数")]
    public UnityEvent SoundStopEventForServer;

    private float soundTimer = 0f;
    private AudioSource audio_source;

    public override void OnNetworkSpawn()
    {
        audio_source = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!IsServer) return;

        if (enemy_sound_sensor_mode.Value)
        {
            soundTimer += Time.deltaTime;
            if (soundTimer >= SoundPlayingTime)
            {
                // サーバー側イベント
                SoundStopEventForServer.Invoke();

                // クライアント全員に止めろって通知
                SensorSoundStopClientRpc();

                soundTimer = 0f;
            }
        }
    }

    // 外部から音を鳴らしたいときはコレ呼ぶ
    public void SensorSoundPlay()
    {
        // サーバーに「鳴らして！」要求して
        SensorSoundPlayRequestServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SensorSoundPlayRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        // フラグ立て
        enemy_sound_sensor_mode.Value = true;
        // クライアント全員に音再生命令
        SensorSoundPlayClientRpc();
    }

    [ClientRpc]
    private void SensorSoundPlayClientRpc(ClientRpcParams rpcParams = default)
    {
        // クライアントで音鳴らす
        audio_source.Play();
    }

    [ClientRpc]
    private void SensorSoundStopClientRpc(ClientRpcParams rpcParams = default)
    {
        // クライアントで音止める & イベント
        audio_source.Stop();
        SoundStopEvent.Invoke();
        // サーバー側フラグクリア
        if (IsOwner || IsServer)
            EnemySoundSensorModeRequestServerRpc(false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void EnemySoundSensorModeRequestServerRpc(bool value, ServerRpcParams rpcParams = default)
    {
        enemy_sound_sensor_mode.Value = value;
    }
}
