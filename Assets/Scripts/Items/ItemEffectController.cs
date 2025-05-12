using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemEffectController : Singleton<ItemEffectController>
{
    private class SenkouItemUse
    {
        public float SenkouUsingTimeNow;
        public float SenkouUsingTime;
        public bool usingNow;
        public DiscoveryPlayerStatus discovery_player_status;
        public SoundSensorStatus sound_sensor_status;
    }
    [SerializeField]
    private SenkouItemUse senkou_item_use;
    private EnemyAllController enemy_all_controller;
    // Start is called before the first frame update
    void Start()
    {
        enemy_all_controller = EnemyAllController.Instance;
        Debug.Log("ItemEffectControllerStart: " + enemy_all_controller);
    }

    // Update is called once per frame
    void Update()
    {
        if (senkou_item_use.usingNow)
        {
            senkou_item_use.SenkouUsingTimeNow += Time.deltaTime;
            if(senkou_item_use.SenkouUsingTimeNow > senkou_item_use.SenkouUsingTime)
            {
                //ü‚ÌŒø‰ÊŠÔ‚ªI‚í‚è‚Ü‚µ‚½
                enemy_all_controller.AllEnemySoundSensorChange(null, true);
                enemy_all_controller.AllDiscoveryPlayerSoundSensorChange(null, true);
                senkou_item_use.SenkouUsingTimeNow = 0;
            }
        }
    }
    public void SenkouUse(float _time)
    {
        senkou_item_use.SenkouUsingTime = _time;
        senkou_item_use.usingNow = true;
        Debug.Log("enemy_all_controller: " + enemy_all_controller);
        Debug.Log("senkou_item_use: " + senkou_item_use);
        Debug.Log("sound_sensor_status: " + senkou_item_use.sound_sensor_status);
        enemy_all_controller.AllEnemySoundSensorChange(senkou_item_use.sound_sensor_status, false);
        enemy_all_controller.AllDiscoveryPlayerSoundSensorChange(senkou_item_use.discovery_player_status, false);
        senkou_item_use.SenkouUsingTimeNow = 0;
    }
}
