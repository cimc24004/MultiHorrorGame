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
    private EnemyStatus enemy_status;
    public float ArriveSoundPlaceWaitTimeToReturnNow;
    private Vector3 DefaultPosition;
    //プレイヤーとの距離
    private float ToPlayerDistance;
    private Animator enemy_anim;
    private float ToSoundPlaceDistance;
    //音が鳴った先の場所
    private Vector3 SoundPlacePosition;
    //音が鳴った場所に移動しているNow
    public bool MoveToSoundPlaceNow;
    [Header("音がなった場所についた後戻るまで判断する時間。")]
    public float ArriveSoundPlaceWaitTimeToReturn;
    [Header("プレイヤーの状態によって音に気づく距離を変える。")]
    [Space]
    [Header("プレイヤーが走っている時")]
    public float RealizeSoundPlayerRunDistance;
    [Header("プレイヤーが歩いている時")]
    public float RealizeSoundPlayerWalkDistance;
    [Header("プレイヤーがしゃがみ歩きしてるとき")]
    public float RealizeSoundPlayerCrouchWalkDistance;
    [Space]
    [Header("音が鳴ったさきのポイント歩き移動する速さ")]
    public float SpeedWalkWhenRealizeSound = 1;
    [Header("音が鳴ったさきのポイント走り移動する速さ")]
    public float SpeedRunWhenRealizeSound = 4;
    [Header("探すのをあきらめて戻るときの歩く速さ")]
    public float SpeedWhenReturn = 1;
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
    }

    // Update is called once per frame
    void Update()
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
                ArriveSoundPlaceWaitTimeToReturnNow += Time.deltaTime;
                if(ArriveSoundPlaceWaitTimeToReturnNow >= ArriveSoundPlaceWaitTimeToReturn)
                {
                    //ある程度時間がたったので

                    //元の位置に戻るようにしている。 これを最短の探索経路のPosを入れたりすればいいと思う。
                    follow_object_nav.ChaseToTargetSet(true, DefaultPosition, SpeedWhenReturn);
                    enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Walk;
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
            ArriveSoundPlaceWaitTimeToReturnNow = 0;
        }

        //ここからはプレイヤーの足音がなったときの話。
        if (player_status.MoveSpeed != 0)
        {
            if (player_status.player_move_state == PlayerStatus.PlayerMoveState.Run)
            {
                //プレイヤーが走っている場合
                if (ToPlayerDistance <= RealizeSoundPlayerRunDistance)
                {
                    Debug.Log("走っているので音が鳴って酔ってくるよ！11111111");
                    if (!MoveToSoundPlaceNow)
                    {
                        SoundSeonsor(Player.transform.position, SpeedWalkWhenRealizeSound);
                        enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Walk;
                        MoveToSoundPlaceNow = true;
                    }
                }
            }
            else if (player_status.player_move_state == PlayerStatus.PlayerMoveState.Walk)
            {
                //プレイヤーが歩いている場合
                if (ToPlayerDistance <= RealizeSoundPlayerWalkDistance)
                {
                    //プレイヤーとの距離が気づく範囲にいる場合
                    Debug.Log("歩いているので音が鳴って酔ってくるよ！2222222");
                    if (!MoveToSoundPlaceNow)
                    {
                        SoundSeonsor(Player.transform.position, SpeedWalkWhenRealizeSound);
                        enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Walk;

                        MoveToSoundPlaceNow = true;
                    }
                }
            }
            else if (player_status.player_move_state == PlayerStatus.PlayerMoveState.Crouch)
            {
                //プレイヤーがしゃがんでいる場合
                if (ToPlayerDistance <= RealizeSoundPlayerCrouchWalkDistance)
                {
                    //プレイヤーとの距離が気づく範囲にいる場合
                    Debug.Log("歩いているので音が鳴って酔ってくるよ！33333");
                    if (!MoveToSoundPlaceNow)
                    {
                        SoundSeonsor(Player.transform.position, SpeedWalkWhenRealizeSound);
                        enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Walk;

                        MoveToSoundPlaceNow = true;
                    }
                }
            }
        }
    }
    //TargetPos: 音が鳴った目標先の場所 ReturnPos: 音が鳴って目標先に到着した後帰る先の場所

    public void SoundSeonsor(Vector3 TargetPos, float _speed)
    {
        //どこからか音鳴らすときこの関数を呼ぶ。 例(プレイヤーの足音や非常ベルの音をならす関数などで)
        if (!discovery_player_controller.DiscoveryPlayerNow)
        {
            //敵がプレイヤーを追いかけている最中じゃない時に実行するようにします。
            follow_object_nav.ChaseToTargetSet(true, TargetPos, _speed);
            SoundPlacePosition = TargetPos;

            MoveToSoundPlaceNow = true;

        }
        else
        {
            MoveToSoundPlaceNow = false;
        }
    }

}
