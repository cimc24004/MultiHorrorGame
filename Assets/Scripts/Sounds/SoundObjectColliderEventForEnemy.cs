using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SoundEnemyObjectColliderForEvent : NetworkBehaviour
{
    //敵につける音センサーイベントスクリプト(プレイヤーの足音以外のアイテムから出る音など)

    public bool NoticedSound;
    [SerializeField] private bool InSoundRange;
    [SerializeField] private SoundObjectEventWithEnemySensor sound_object_event_with_sensor;
    [SerializeField] private SoundObjectEventWithEnemySensor before_sound_object_event_with_sensor;

    private EnemySoundSensor enemy_sound_sensor;
    private EnemyStatus enemy_status;
    private FollowObjectByNav follow_object_by_nav;
    private bool once;

    private bool before_enemy_sound_sensor_mode = false;
    [SerializeField] private GameObject hited_collider;

    // ───── 追加した設定 ───────────────────────
    [Header("BoxCast 設定")]
    [SerializeField] private Vector3 boxHalfSize = new Vector3(2f, 1f, 2f);
    [Tooltip("SoundObjectRange 用レイヤーマスク")]
    [SerializeField] private LayerMask soundObjectLayerMask;
    private MoveSpeedController move_speed_controller;
    // private List<GameObject> triggeries;
    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        enemy_status       = GetComponent<EnemyStatus>();
        follow_object_by_nav = GetComponent<FollowObjectByNav>();
        enemy_sound_sensor = GetComponent<EnemySoundSensor>();
        move_speed_controller = this.GetComponent<MoveSpeedController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;
        // ── ここで BoxCast（実質 OverlapBox） ──────────
        Collider[] hits = Physics.OverlapBox(
            transform.position,
            boxHalfSize,
            Quaternion.identity,
            soundObjectLayerMask);
        
        if (hits.Length > 0)
        {
            // いずれかの SoundObjectRange が範囲内
            hited_collider = hits[0].gameObject;
            if(hited_collider.tag == "SoundObjectRange"){
                if((sound_object_event_with_sensor != before_sound_object_event_with_sensor) || sound_object_event_with_sensor == null){
                    sound_object_event_with_sensor = hits[0].transform.parent
                                                    .GetComponent<SoundObjectEventWithEnemySensor>();
                }


                before_sound_object_event_with_sensor = sound_object_event_with_sensor;
                InSoundRange = true;
            }
        }
        else
        {
            InSoundRange = false;
        }

        // ── 以降は元のロジックそのまま ────────────────
        if (InSoundRange)
        {
            if (sound_object_event_with_sensor.enemy_sound_sensor_mode.Value)
            {
                enemy_sound_sensor.SoundSeonsor(
                    sound_object_event_with_sensor.transform.position,
                    move_speed_controller.RunSpeed,
                    true
                    ,FollowObjectByNav.MoveStyle.Run);

                enemy_sound_sensor.sound_by_obj = true;
            }
            before_enemy_sound_sensor_mode = sound_object_event_with_sensor.enemy_sound_sensor_mode.Value;
        }
    }

    // OnTrigger 系は不要になったのでコメントアウト
    /*
    private void OnTriggerEnter(Collider other) { }
    
    private void OnTriggerExit(Collider other) { }
    */
    // private void OnTriggerStay(Collider other)
    // {
    //     if(!triggeries.Contains(other.gameObject)){
    //         triggeries.Add(other.gameObject);
    //     }
    // }

    // デバッグ用: シーンビューで検知範囲を表示
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.identity, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxHalfSize * 2f);
    }
}
