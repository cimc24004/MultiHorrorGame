using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
public class PrayToDeliverItemAppearPlace : Singleton<PrayToDeliverItemAppearPlace>
{
    public Transform DeliverItemApperancePlace;

    [Header("出現させる敵")]
    public GameObject ApperanceEnemy;
    public Transform ApperanceEnemyPos;

    [Header("アイテムを取得したときのイベント")]
    public UnityEvent2 ItemGetedEvent;

    [Header("どちらでもいいのでドアをセット")]
    public DoorOpenCloseController door_open_close_controller;
    private ItemDatabases item_databases;
    private AudioSource audio_source;


    [Header("必要な仏壇に祈る回数")]
    public int NeedPrayCount = 1;

    [Header("現在の祈り回数")]
    public int PrayCountNow = 0;

    // Start is called before the first frame update
    void Start()
    {
        item_databases = ItemDatabases.instance;
        audio_source = this.gameObject.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [Button]
    public void OnPrayCompleted()
    {
        //どこかの仏壇で祈りがコンプリートされたとき。
        PrayCountNow++;
        if(NeedPrayCount <= PrayCountNow)
        {
            //必要な祈り回数が必要になったら最終イベントアイテムを出現させる。
            DeliverItemAppear();
        }
    }
    public void DeliverItemAppear()
    {

        //int r = Random.Range(0, item_databases.ForDeliverItem.Count);

        GameObject item = Instantiate(item_databases.ForDeliverItem.ItemPutObjectPrefab, DeliverItemApperancePlace.transform.position, Quaternion.identity);
        ItemObject item_object_script = item.GetComponent<ItemObject>();
        GameObject enemy = null;
        EnemySoundSensor enemy_sound_sensor = null;
        DiscoveryPlayerController discovery_player_controller = null;
        //アイテムを取得したときのイベント。
        item_object_script.ItemGetAddEvent.AddListener(() => {
            enemy = Instantiate(ApperanceEnemy, ApperanceEnemyPos.position, Quaternion.LookRotation(-transform.right));
            enemy_sound_sensor = enemy.GetComponent<EnemySoundSensor>();
            discovery_player_controller = enemy.GetComponent<DiscoveryPlayerController>();
            enemy_sound_sensor.enabled = false;
            discovery_player_controller.enabled = false;
            ItemGetedEvent.Invoke();
        });
        door_open_close_controller.OpenCompleted.AddListener(() => { 
            enemy_sound_sensor.enabled = true;
            discovery_player_controller.enabled = true;
        });
        audio_source.Play();
    }
}
