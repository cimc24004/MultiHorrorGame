using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySoundSensor : MonoBehaviour
{
    //足音によって敵が寄ってくる機能。
    private DiscoveryPlayerController discovery_player_controller;
    private FollowObjectByNav follow_object_nav;
    private GameObject Player;
    private PlayerStatus player_status;
    private EnemyRoutingController enemy_routing_controller;
    private EnemyStatus enemy_status;
    public float ArriveSoundPlaceWaitTimeToReturnNow;
    private Vector3 DefaultPosition;
    //プレイヤーとの距離
    private float ToPlayerDistance;
    private Animator enemy_anim;
    private TimeManager time_manager;
    private double ToSoundPlaceDistance;
    //音が鳴った先の場所
    private Vector3 SoundPlacePosition;
    //音が鳴った場所に移動しているNow
    public bool MoveToSoundPlaceNow;

    //オブジェクトが鳴らしている音に向かっているのかどうか
    public bool MoveToSoundObjectPlace;

    public SoundSensorStatus sound_sensor_status;
    // Start is called before the first frame update
    void Start()
    {
        discovery_player_controller = this.gameObject.GetComponent<DiscoveryPlayerController>();
        follow_object_nav = this.gameObject.GetComponent<FollowObjectByNav>();
        player_status = PlayerStatus.instance;
        Player = player_status.gameObject;
        enemy_status = this.gameObject.GetComponent<EnemyStatus>();
        enemy_anim = this.gameObject.GetComponent<Animator>();
        DefaultPosition = this.transform.position;
        time_manager = TimeManager.instance;
        enemy_routing_controller = this.gameObject.GetComponent<EnemyRoutingController>();
    }
    private void Update()
    {
        if (follow_object_nav.ArrivedTarget && MoveToSoundPlaceNow)
        {
            ArriveSoundPlaceWaitTimeToReturnNow += Time.deltaTime;
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {

        //プレイヤーの場合はこちらで制御します。 音を慣らすアイテムによって敵が酔ってくるシステムはアイテムそれぞれに作ります。
        ToPlayerDistance = Vector3.Distance(this.transform.position, Player.transform.position);
        ToSoundPlaceDistance = follow_object_nav.ToTargetPosDistance;
        if (discovery_player_controller.DiscoveryPlayerNow)
        {
            MoveToSoundPlaceNow = false;
            return;
        }
        if (MoveToSoundPlaceNow)
        {
            //今音がなっている場所に向かっている場合


            if(follow_object_nav.ArrivedTarget)
            {

                //音が鳴った位置に到着し終えた場合 少し時間を待ってから元の位置に戻るようにする。
                if (ArriveSoundPlaceWaitTimeToReturnNow >= sound_sensor_status.ArriveSoundPlaceWaitTimeToReturn)
                {
                    //ある程度時間がたったので

                    //元の位置に戻るようにしている。 これを最短の探索経路のPosを入れたりすればいいと思う。
                    //enemy_routing_controller.RoutingMovePermit = true;
                    enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Walk;
                    enemy_routing_controller.NearRouteSet();
                    //follow_object_nav.ChaseToTargetSet(true, DefaultPosition, SpeedWhenReturn);
                    Debug.Log("NearRouteSet 帰るところです！");
                    MoveToSoundPlaceNow = false;
                    ArriveSoundPlaceWaitTimeToReturnNow = 0;
                }
            }
            else
            {
            }
        }
        else
        {
            follow_object_nav.ArrivedTarget = false;
            ArriveSoundPlaceWaitTimeToReturnNow = 0;
        }

        //ここからはプレイヤーの足音がなったときの話。
        if (player_status.MoveSpeed != 0)
        {
            if (player_status.player_move_state == PlayerStatus.PlayerMoveState.Run)
            {
                //プレイヤーが走っている場合
                if (ToPlayerDistance <= sound_sensor_status.RealizeSoundPlayerRunDistance)
                {
                    Debug.Log("走っているので音が鳴って酔ってくるよ！11111111");
                    SoundSeonsor(Player.transform.position, sound_sensor_status.SpeedWalkWhenRealizeSound, false);
                    enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Walk;

                    if (!MoveToSoundPlaceNow)
                    {
                        MoveToSoundPlaceNow = true;
                    }
                }
            }
            else if (player_status.player_move_state == PlayerStatus.PlayerMoveState.Walk)
            {
                //プレイヤーが歩いている場合
                if (ToPlayerDistance <= sound_sensor_status.RealizeSoundPlayerWalkDistance)
                {
                    //プレイヤーとの距離が気づく範囲にいる場合
                    Debug.Log("歩いているので音が鳴って酔ってくるよ！2222222");
                    SoundSeonsor(Player.transform.position, sound_sensor_status.SpeedWalkWhenRealizeSound, false);
                    enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Walk;

                    if (!MoveToSoundPlaceNow)
                    {

                        MoveToSoundPlaceNow = true;
                    }
                }
            }
            else if (player_status.player_move_state == PlayerStatus.PlayerMoveState.Crouch)
            {
                //プレイヤーがしゃがんでいる場合
                if (ToPlayerDistance <= sound_sensor_status.RealizeSoundPlayerCrouchWalkDistance)
                {
                    //プレイヤーとの距離が気づく範囲にいる場合
                    Debug.Log("歩いているので音が鳴って酔ってくるよ！33333");
                    SoundSeonsor(Player.transform.position, sound_sensor_status.SpeedWalkWhenRealizeSound, false);
                    enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Walk;

                    if (!MoveToSoundPlaceNow)
                    {

                        MoveToSoundPlaceNow = true;
                    }
                }
            }
        }
    }
    //TargetPos: 音が鳴った目標先の場所 ReturnPos: 音が鳴って目標先に到着した後帰る先の場所

    public void SoundSeonsor(Vector3 TargetPos, float _speed, bool _SoundObject)
    {
        enemy_routing_controller.PermitFunction(false);
        //どこからか音鳴らすときこの関数を呼ぶ。 例(プレイヤーの足音や非常ベルの音をならす関数などで)
        if (!discovery_player_controller.DiscoveryPlayerNow)
        {
            //敵がプレイヤーを追いかけている最中じゃない時に実行するようにします。
            follow_object_nav.ChaseToTargetSet(true, TargetPos, _speed);
            SoundPlacePosition = TargetPos;
            MoveToSoundPlaceNow = true;
            MoveToSoundObjectPlace = _SoundObject;
        }
        else
        {
            MoveToSoundPlaceNow = false;
        }
    }

}
