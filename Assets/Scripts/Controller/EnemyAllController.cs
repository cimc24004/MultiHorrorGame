using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public class EnemyAllController : Singleton<EnemyAllController>
{
    [SerializeField]
    private List<EnemySoundSensor> enemy_sound_sensors;
    [SerializeField]
    private List<DiscoveryPlayerController> discovery_player_controllers;
    private void Start()
    {
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
        foreach(EnemySoundSensor enemy_sound_sensor in enemy_sound_sensors)
        {
            enemy_sound_sensor.SoundSensorStatusChange(_sound_sensor_status, _originalMode);
        }
    }
    public void AllDiscoveryPlayerSoundSensorChange(DiscoveryPlayerStatus _discovery_player_status, bool _originalMode)
    {
        foreach (DiscoveryPlayerController discovery_player_controller in discovery_player_controllers)
        {
            discovery_player_controller.DiscoveryPlayerStatusChange(_discovery_player_status, _originalMode);
        }
    }

}
