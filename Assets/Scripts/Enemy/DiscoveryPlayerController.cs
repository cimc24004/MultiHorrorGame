using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscoveryPlayerController : MonoBehaviour
{
    public bool DiscoveryPlayerNow;

    public Transform EyePosition;
    public DiscoveryPlayerStatus discovery_player_status;
    //プレイヤーを視認しているかどうか
    public bool SightningPlayer;
    //プレイヤーが隠れた時に敵はその隠れた所を見ているかどうか。
    public bool SightningPlayerWhenHidePlayer;

    [SerializeField] private Material EnemyDiscoveryPlayerSlihouetteMaterial;
    
    private GameObject Player;
    private Transform PlayersHeadTransform;
    private FollowObjectByNav follow_object_nav;
    private PlayerStatus player_status;
    private Animator anim;
    private RaycastHit raycast_hit;
    //プレイヤーが見つけられる範囲以内にいるかどうか。
    public bool InsidePlayer;
    [HideInInspector]
    public Vector3 direction;
    private Vector3 DefaultPos;
    private float ToPlayerDistance;
    private EnemyStatus enemy_status;
    private CameraEnemyFaceExpanding camera_enemy_face_expanding;
    private EnemyRoutingController enemy_routing_controller;
    private TimeManager time_manager;
    private MaterialChangeController material_change_controller;
    //private VisibleThroughTheWallController visible_through_the_wall_controller;
    [SerializeField]private GameObject raycasted_obj;
    //プレイヤーが自分の視界から外れている間の時間Now
    public float SightOutPlayerTimeNow;

    private HorrorDirector horror_director;

    private bool DiscoveriedEventOnce;
    private bool NonDiscoveriedEventOnce;

    // Start is called before the first frame update
    void Start()
    {
        player_status = PlayerStatus.instance;
        //visible_through_the_wall_controller = this.gameObject.GetComponent<VisibleThroughTheWallController>();
        material_change_controller = this.gameObject.GetComponent<MaterialChangeController>();
        anim = this.gameObject.GetComponent<Animator>();
        Player = player_status.gameObject;
        PlayersHeadTransform = player_status.HeadPos;
        DefaultPos = this.transform.position;
        follow_object_nav = this.gameObject.GetComponent<FollowObjectByNav>();
        camera_enemy_face_expanding = this.gameObject.GetComponent<CameraEnemyFaceExpanding>();
        enemy_status = this.gameObject.GetComponent<EnemyStatus>();
        time_manager = TimeManager.instance;
        enemy_routing_controller = this.gameObject.GetComponent<EnemyRoutingController>();
        horror_director = HorrorDirector.instance;

    }

    // Update is called once per frame
    void Update()
    {
        direction = (PlayersHeadTransform.transform.position - EyePosition.transform.position).normalized;
        ToPlayerDistance = Vector3.Distance(Player.transform.position, this.transform.position);
        if (player_status.PlayerDeaded) {
            enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Walk;
            follow_object_nav.ArrivedTarget = false;
            enemy_routing_controller.NearRouteSet();
            DiscoveryPlayer(false);
        } 
        if (ToPlayerDistance <= discovery_player_status.PlayerDiscoveryRange)
            InsidePlayer = true;
        else
            InsidePlayer = false;
        if (DiscoveryPlayerNow)
        {
            horror_director.DiscoveryNow = true;
            if (!DiscoveriedEventOnce)
            {
                //visible_through_the_wall_controller.VisibleOnce = true;
                horror_director.DiscoveriedHorror();
                DiscoveriedEventOnce = true;
            }
            if (follow_object_nav.ArrivedTarget)
            {
                enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Stop;
            }
            DiscoveryPlayer(true);
            //見つかっている状態でプレイヤーの位置に到着したら
            enemy_routing_controller.PermitFunction(false);
            NonDiscoveriedEventOnce = false;
        }
        else
        {
            DiscoveriedEventOnce = false;
            if (!NonDiscoveriedEventOnce)
            {
                horror_director.DiscoveryNow = false;
                horror_director.ResetSetting();
                NonDiscoveriedEventOnce = true;
            }
        }
        anim.SetBool("PlayerHidingNow", player_status.HidingNow);
        if (DiscoveryPlayerNow && !SightningPlayer)
        {
            //敵に見つかっているかつプレイヤーが見えてなかったら


            //時間を計り、ある程度の時間がたったら追いかけるのをやめる
            SightOutPlayerTimeNow += Time.deltaTime;
            if (SightOutPlayerTimeNow > discovery_player_status.SightOutPlayerTime)
            {
                Debug.Log("時間経過しましたよ");
                //敵が追いかけてる最中もしプレイヤーを見失った場合は元の位置に戻る
                //今はデフォルト位置に戻る設定にしているが近くの徘徊ルートに行くという設定する予定。
                //一番近くの徘徊ルートにいく。　つまりは固定の位置なのでSetDestinationOnceModeはtrue
                enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Walk;
                follow_object_nav.ArrivedTarget = false;
                enemy_routing_controller.NearRouteSet();

                DiscoveryPlayer(false);


                //follow_object_nav.ChaseToTargetSet(true, DefaultPos, SpeedWhenDiscoveried);
                SightOutPlayerTimeNow = 0;
            }

        }
        else
        {
            SightOutPlayerTimeNow = 0;
        }


        if (SightningPlayer)
        {
            if (player_status.HidingNow)
            {
                //視認しているかつ隠れてる場合は隠れてること知ってるよー　変数をTrueに。
                SightningPlayerWhenHidePlayer = true;
            }
        }
        if (SightningPlayerWhenHidePlayer)
        {
            if (ToPlayerDistance <= 2)
            {
                //隠れてることがバレていてかつ近くにいたら場合はこの関数を実行。
                player_status.player_hide_object_action.GetCoughtWhenHidingByEnemy(this.gameObject);
                this.SightningPlayerWhenHidePlayer = false;
            }
        }
        if (Vector3.Angle(new Vector3(direction.x, 0, direction.z), new Vector3(EyePosition.forward.x, 0, EyePosition.forward.z)) <= discovery_player_status.VisionFieldAngle && InsidePlayer)
        {
            //プレイヤーが敵の視界以内にいるかどうか && プレイヤーが範囲以内にいるかどうか。
            Debug.Log("プレイヤーが敵の視界以内にいるかどうか && プレイヤーが範囲以内にいるかどうか");
            if(Physics.Raycast(EyePosition.transform.position, direction, out raycast_hit,Mathf.Infinity, discovery_player_status.layerMask))
            {
                raycasted_obj = raycast_hit.collider.gameObject;
                //Raycastをプレイヤーにとりあえず飛ばす。
                //プレイヤーにRaycastが当たっているが、まずその前に障害物が存在する場合は見つからなかった判定にしておく。
                if (raycast_hit.collider.gameObject.tag == "Player")
                {
                    Debug.Log("Playerが敵に見つかりました！");
                    if (!player_status.HidingNow)
                    {
                        //プレイヤーを視認しているナウ。
                        SightningPlayer = true;
                        //即見つかる判定ではある。
                        DiscoveryPlayerNow = true;
                        //renderer.material = EnemyDiscoveryPlayerSlihouetteMaterial;
                    }
                }
                else
                {
                    SightningPlayer = false;
                }
            }
            else {
                SightningPlayer = false;
            }
        }
        else
        {
            SightningPlayer = false;
        }
        Debug.DrawRay(EyePosition.transform.position, direction * 1000, Color.green);

        if (DiscoveryPlayerNow && !SightningPlayerWhenHidePlayer)
        {
            if(!SightningPlayer)
            {
                if (player_status.HidingNow)
                {
                    //プレイヤーが隠れていて敵にばれていなかったらストップ距離は大きめに。。
                    follow_object_nav.StoppingDistance = 4;
                }
                else
                {
                    //それ以外ならデフォルトのストップ距離にしておく。
                    follow_object_nav.StoppingDistance = follow_object_nav.DefaultStoppingDistance;
                }
            }
            else
            {
                follow_object_nav.StoppingDistance = follow_object_nav.DefaultStoppingDistance;
            }
        }
        else
        {
            follow_object_nav.StoppingDistance = follow_object_nav.DefaultStoppingDistance;
        }

    }
    private void DiscoveryPlayer(bool _discovery)
    {
        //敵に見つかったとき、または見つかってる状態をリセットしたい時に使う。
        //Trueならば見つかったときのイベント、Falseならば見つからなくなったときにリセットするための関数
        DiscoveryPlayerNow = _discovery;
        if (_discovery)
        {
            if (!follow_object_nav.ArrivedTarget)
            {
                enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Run;
            }

            follow_object_nav.ChaseToTargetSet(true, Player.transform.position, discovery_player_status.SpeedWhenDiscoveried);
        }
        else
        {

        }
    }
    

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(EyePosition.transform.position, (Quaternion.Euler(0, -discovery_player_status.VisionFieldAngle, 0) * EyePosition.forward) * 10);
        Gizmos.DrawRay(EyePosition.transform.position, (Quaternion.Euler(0, discovery_player_status.VisionFieldAngle, 0) * EyePosition.forward) * 10);
    }


}
