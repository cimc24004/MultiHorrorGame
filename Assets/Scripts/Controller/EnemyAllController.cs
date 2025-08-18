using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Unity.Netcode;
public class EnemyAllController : Singleton<EnemyAllController>
{
    [SerializeField]
    public List<EnemySoundSensor> enemy_sound_sensors;
    [SerializeField]
    public List<DiscoveryPlayerController> discovery_player_controllers;
    public List<MoveSpeedController> enemy_move_speed_controllers;

    private void Start()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        AllSetStatusList();
    }
    [Button]
    public void AllSetStatusList()
    {
        enemy_sound_sensors = new List<EnemySoundSensor>(FindObjectsOfType<EnemySoundSensor>());
        discovery_player_controllers =  new List<DiscoveryPlayerController>(FindObjectsOfType<DiscoveryPlayerController>());
    }
    public void AllEnemySoundSensorChange(SoundSensorStatus _sound_sensor_status, bool _originalMode)
    {
        //サーバーから実行される。
        foreach (EnemySoundSensor enemy_sound_sensor in enemy_sound_sensors)
        {
            enemy_sound_sensor.SoundSensorStatusChange(_sound_sensor_status, _originalMode);
        }
    }
    public void AllDiscoveryPlayerSoundSensorChange(DiscoveryPlayerStatus _discovery_player_status, bool _originalMode)
    {
        //サーバーから実行される。
        foreach (DiscoveryPlayerController discovery_player_controller in discovery_player_controllers)
        {
            discovery_player_controller.DiscoveryPlayerStatusChange(_discovery_player_status, _originalMode);
        }
    }
    public void IncreaseEnemiesMoveSpeed()
    {
        //サーバーから実行される
        //すべての敵のスピードを上げる関数
        foreach (MoveSpeedController move_speed_controller in enemy_move_speed_controllers)
        {
            move_speed_controller.MoveSpeedUpAndAnimSpeedUp(0f, 0f, 0.1f);
        }
    }
}
