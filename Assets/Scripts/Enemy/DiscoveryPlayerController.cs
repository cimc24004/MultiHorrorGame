using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

public class DiscoveryPlayerController : NetworkBehaviour
{
    [Header("視界から見つけるかどうか")]
    public bool DiscoveryPlayerBySightning;
    public Transform EyePosition;

    [SerializeField]
    private DiscoveryPlayerStatus original_discovery_player_status;
    private MoveSpeedController move_speed_controller;
    [SerializeField]
    private DiscoveryPlayerStatus discovery_player_status;

    [Header("追いかけてるときに音声を流すかどうか PlayOnAwake限定")]
    [SerializeField]
    private bool IsAudioPlayOnChase;
    [ShowIf("IsAudioPlayOnChase")]
    private CharacterAudioController character_audio_controller;

    //プレイヤーを視認しているかどうか
    // public bool SightningPlayer;
    //プレイヤーが隠れた時に敵はその隠れた所を見ているかどうか。
    public bool SightningPlayerWhenHidePlayer;
    private FollowObjectByNav follow_object_nav;
    private Animator anim;
    private RaycastHit raycast_hit;
    //プレイヤーが見つけられる範囲以内にいるかどうか。
    [HideInInspector]
    public Vector3 direction;
    private Vector3 DefaultPos;
    private EnemyStatus enemy_status;
    private CameraEnemyFaceExpanding camera_enemy_face_expanding;
    private EnemyRoutingController enemy_routing_controller;
    private TimeManager time_manager;
    private MaterialChangeController material_change_controller;
    //private VisibleThroughTheWallController visible_through_the_wall_controller;
    [SerializeField] private GameObject raycasted_obj;
    //プレイヤーが自分の視界から外れている間の時間Now

    private HorrorDirector horror_director;

    private bool DiscoveriedEventOnce;
    private bool NonDiscoveriedEventOnce;
    private SilhouetteChangeController silhouette_change_controller;
    //発見したプレイヤーたち
    [SerializeField] public List<GameObject> DiscoveriedPlayers = new List<GameObject>();
    [SerializeField]public GameObject FirstKeyOfDiscoveriedPlayers;
    private ToPlayersDistanceTracker to_players_distance_tracker;
    private GameNetworkController game_network_controller;
    [SerializeField, ShowInInspector] public Dictionary<GameObject, PlayerVisibilityMapStatus> PlayersVisibilityMapStatuesWithKey = new Dictionary<GameObject, PlayerVisibilityMapStatus>();
    public class PlayerVisibilityMapStatus
    {
        public Vector3 ToPlayerDirection;
        public bool InsidePlayer;
        public bool InAngle;
        public PlayerStatus player_status;
        public bool SightningPlayer;
        public bool DiscoveryPlayerNow;
        public float SightOutPlayerTimeNow;
    }
    // Start is called before the first frame update
    // void OnEnable()
    // {
    //     PlayerStatus.OnLocalPlayerSpawned += InitOnPlayerSpawn;
    // }
    // void OnDisable()
    // {
    //     PlayerStatus.OnLocalPlayerSpawned -= InitOnPlayerSpawn;
    // }
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        to_players_distance_tracker = this.gameObject.GetComponent<ToPlayersDistanceTracker>();
        //visible_through_the_wall_controller = this.gameObject.GetComponent<VisibleThroughTheWallController>();
        material_change_controller = this.gameObject.GetComponent<MaterialChangeController>();
        anim = this.gameObject.GetComponent<Animator>();
        silhouette_change_controller = this.gameObject.GetComponent<SilhouetteChangeController>();
        move_speed_controller = this.gameObject.GetComponent<MoveSpeedController>();
        horror_director = HorrorDirector.instance;
        DefaultPos = this.transform.position;
        follow_object_nav = this.gameObject.GetComponent<FollowObjectByNav>();
        camera_enemy_face_expanding = this.gameObject.GetComponent<CameraEnemyFaceExpanding>();
        enemy_status = this.gameObject.GetComponent<EnemyStatus>();
        character_audio_controller = enemy_status.character_audio_controller;
        time_manager = TimeManager.instance;
        this.gameObject.TryGetComponent<EnemyRoutingController>(out enemy_routing_controller);
        discovery_player_status = original_discovery_player_status;
        game_network_controller = GameNetworkController.instance;
        StartCoroutine(DisctionarySet());
    }
    IEnumerator DisctionarySet()
    {
        yield return null;
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            Debug.Log("NetworkManager.Singleton.SpawnManager.PlayerObjects.Count: " + NetworkManager.Singleton.SpawnManager.PlayerObjects.Count);
            NetworkObject PlayerNetworkObject = client.PlayerObject;
            
            PlayerStatus playerStatus = PlayerNetworkObject.gameObject.GetComponent<PlayerStatus>();
            PlayerVisibilityMapStatus player_visibility_map_status = new PlayerVisibilityMapStatus() { ToPlayerDirection = Vector3.zero, InAngle = false, InsidePlayer = false, player_status = playerStatus };
            PlayersVisibilityMapStatuesWithKey.Add(PlayerNetworkObject.gameObject, player_visibility_map_status);
        }
    }
    void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += OnLocalPlayerSpawned;
    }
    void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned += OnLocalPlayerSpawned;
        
    }
    private void OnLocalPlayerSpawned(PlayerStatus _) {

    }
    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;
        if (IsServer)
        {
            foreach (var kvp in PlayersVisibilityMapStatuesWithKey)
            {
                var go = kvp.Key;
                Debug.DrawLine(
                    EyePosition.position,
                    go.transform.position,
                    Color.red);      // これがプレイヤーRoot

                Debug.DrawLine(
                    EyePosition.position,
                    kvp.Value.player_status.HeadPos.position,
                    Color.blue);     // こっちは HeadPos
            }
        }
        if (DiscoveriedPlayers.Count != 0)
        {
            //ここは後々必要になる。 一時的にオフ
            // if (player_status.PlayerDeaded)
            // {
            //     enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Walk;
            //     follow_object_nav.ArrivedTarget = false;
            //     enemy_routing_controller?.NearRouteSet();
            //     DiscoveryPlayer(false);
            // }
        }

        if (this.DiscoveryPlayerNowCheck())
        {
            //horror_director.DiscoveryNow = true;
            if (!DiscoveriedEventOnce)
            {
                character_audio_controller.ChangeCharacterBehaviorState(CharacterAudioController.CharacterBehaviorState.Chase);
                silhouette_change_controller?.SilhouetteChangeWithAutoReset(3);
                horror_director.EnemyDiscoveredPlayer();
                DiscoveriedEventOnce = true;
            }
            // if (follow_object_nav.ArrivedTarget)
            // {
            // }
            DiscoveryPlayer(true);
            //見つかっている状態でプレイヤーの位置に到着したら
            // enemy_routing_controller?.PermitFunction(false);
            NonDiscoveriedEventOnce = false;
        }
        else
        {
            DiscoveriedEventOnce = false;
            if (!NonDiscoveriedEventOnce)
            {
                horror_director.EnemyLostPlayer();
                character_audio_controller.ChangeCharacterBehaviorState(CharacterAudioController.CharacterBehaviorState.Normal);
                NonDiscoveriedEventOnce = true;
            }
        }




        for (int i = 0; i < game_network_controller.SpawededPlayers.Count; i++)
        {
            if ( to_players_distance_tracker.ToPlayerDistanceLists.Count == 0) return;
            GameObject TargetPlayer = game_network_controller.SpawededPlayers[i];
            bool InsidePlayer = to_players_distance_tracker.ToPlayerDistanceLists[TargetPlayer] <= discovery_player_status.PlayerDiscoveryRange;
            if (InsidePlayer)
            {
                Transform PlayersHeadTransform = PlayersVisibilityMapStatuesWithKey[TargetPlayer].player_status.HeadPos;
                direction = (PlayersHeadTransform.transform.position - EyePosition.transform.position).normalized;
                PlayersVisibilityMapStatuesWithKey[TargetPlayer].InsidePlayer = InsidePlayer;
                PlayersVisibilityMapStatuesWithKey[TargetPlayer].InAngle = Vector3.Angle(new Vector3(direction.x, 0, direction.z), new Vector3(EyePosition.forward.x, 0, EyePosition.forward.z)) <= discovery_player_status.VisionFieldAngle;
                if (PlayersVisibilityMapStatuesWithKey[TargetPlayer].InAngle && InsidePlayer)
                {
                    //プレイヤーが敵の視界以内にいるかどうか && プレイヤーが範囲以内にいるかどうか。
                    //壁は関係ない。
                    if (Physics.Raycast(EyePosition.transform.position, direction, out raycast_hit, Mathf.Infinity, discovery_player_status.layerMask))
                    {
                        raycasted_obj = raycast_hit.collider.gameObject;
                        //Raycastをプレイヤーにとりあえず飛ばす。
                        //プレイヤーにRaycastが当たっているが、まずその前に障害物が存在する場合は見つからなかった判定にしておく。
                        if (raycast_hit.collider.gameObject.tag == "Player")
                        {
                            AddPlayerToDiscoveryPlayerLists(TargetPlayer);
                            Debug.Log("Playerが敵に見つかりました！");
                            if (!PlayersVisibilityMapStatuesWithKey[TargetPlayer].player_status.HidingNow.Value)
                            {
                                //プレイヤーを視認しているナウ。
                                PlayersVisibilityMapStatuesWithKey[TargetPlayer].SightningPlayer = true;
                                // SightningPlayer = true;
                                //即見つかる判定ではある。
                                if (DiscoveryPlayerBySightning)
                                {
                                    PlayersVisibilityMapStatuesWithKey[TargetPlayer].DiscoveryPlayerNow = true;
                                }
                            }
                        }
                        else
                        {
                            // SightningPlayer = false;
                            PlayersVisibilityMapStatuesWithKey[TargetPlayer].SightningPlayer = false;
                        }
                    }
                    else
                    {
                        // SightningPlayer = false;
                        PlayersVisibilityMapStatuesWithKey[TargetPlayer].SightningPlayer = false;
                    }
                }
                else
                {
                    // SightningPlayer = false;
                    PlayersVisibilityMapStatuesWithKey[TargetPlayer].SightningPlayer = false;
                }



            }
            if (DiscoveryPlayerNowCheck())
            {
                anim.SetBool("PlayerHidingNow", PlayersVisibilityMapStatuesWithKey[FirstKeyOfDiscoveriedPlayers].player_status.HidingNow.Value);
                if (PlayersVisibilityMapStatuesWithKey[FirstKeyOfDiscoveriedPlayers].SightningPlayer)
                {
                    if (PlayersVisibilityMapStatuesWithKey[FirstKeyOfDiscoveriedPlayers].player_status.HidingNow.Value)
                    {
                        //視認しているかつ隠れてる場合は隠れてること知ってるよー　変数をTrueに。
                        SightningPlayerWhenHidePlayer = true;
                    }
                }
                if (SightningPlayerWhenHidePlayer)
                {
                    if (to_players_distance_tracker.ToPlayerDistanceLists[FirstKeyOfDiscoveriedPlayers] <= 2)
                    {
                        //隠れてることがバレていてかつ近くにいたら場合はこの関数を実行。
                        PlayersVisibilityMapStatuesWithKey[FirstKeyOfDiscoveriedPlayers].player_status.player_hide_object_action.GetCoughtWhenHidingByEnemy(this.gameObject);
                        this.SightningPlayerWhenHidePlayer = false;
                    }
                }
            }



            Debug.DrawRay(EyePosition.transform.position, direction * 1000, Color.green);
            if (PlayersVisibilityMapStatuesWithKey[TargetPlayer].DiscoveryPlayerNow && !PlayersVisibilityMapStatuesWithKey[TargetPlayer].SightningPlayer)
            {
                PlayersVisibilityMapStatuesWithKey[TargetPlayer].SightOutPlayerTimeNow += Time.deltaTime;
                if (PlayersVisibilityMapStatuesWithKey[TargetPlayer].SightOutPlayerTimeNow > discovery_player_status.SightOutPlayerTime)
                {
                    if (TargetPlayer == FirstKeyOfDiscoveriedPlayers)
                    {
                        //敵に見つかっているかつプレイヤーが見えてなかったら
                        //時間を計り、ある程度の時間がたったら追いかけるのをやめる
                        Debug.Log("時間経過しましたよ");
                        //敵が追いかけてる最中もしプレイヤーを見失った場合は元の位置に戻る
                        //今はデフォルト位置に戻る設定にしているが近くの徘徊ルートに行くという設定する予定。
                        //一番近くの徘徊ルートにいく。　つまりは固定の位置なのでSetDestinationOnceModeはtrue
                        enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Walk;
                        follow_object_nav.ArrivedTarget = false;
                        enemy_routing_controller?.NearRouteSet();
                        DiscoveryPlayer(false);
                        RemovePlayerOfDiscoveryPlayerLists(FirstKeyOfDiscoveriedPlayers);
     
                    }
                    PlayersVisibilityMapStatuesWithKey[FirstKeyOfDiscoveriedPlayers].SightOutPlayerTimeNow = 0;
                    //follow_object_nav.ChaseToTargetSet(true, DefaultPos, SpeedWhenDiscoveried);
                

                }
            }
            else
            {
                PlayersVisibilityMapStatuesWithKey[TargetPlayer].SightOutPlayerTimeNow = 0;
            }
            if (!DiscoveryPlayerNowCheck()) return;
            if (PlayersVisibilityMapStatuesWithKey[FirstKeyOfDiscoveriedPlayers].DiscoveryPlayerNow && !SightningPlayerWhenHidePlayer)
            {
                if (!PlayersVisibilityMapStatuesWithKey[FirstKeyOfDiscoveriedPlayers].SightningPlayer)
                {
                    if (PlayersVisibilityMapStatuesWithKey[FirstKeyOfDiscoveriedPlayers].player_status.HidingNow.Value)
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

        


    }
    private void DiscoveryPlayer(bool _discovery)
    {
        PlayersVisibilityMapStatuesWithKey[FirstKeyOfDiscoveriedPlayers].DiscoveryPlayerNow = _discovery;
        if (!_discovery) return;
        //敵に見つかったとき、または見つかってる状態をリセットしたい時に使う。
        //Trueならば見つかったときのイベント、Falseならば見つからなくなったときにリセットするための関数
        if (_discovery)
        {
            if (!follow_object_nav.ArrivedTarget)
            {
                enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Run;
            }

            follow_object_nav.ChaseToTargetSet(true, FirstKeyOfDiscoveriedPlayers.transform.position, move_speed_controller.RunSpeed);
        }
        else
        {

        }
    }


    private void OnDrawGizmos()
    {
        if (!DiscoveryPlayerBySightning) return;
        if (discovery_player_status == null)
            discovery_player_status = original_discovery_player_status;
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(EyePosition.transform.position, (Quaternion.Euler(0, -discovery_player_status.VisionFieldAngle, 0) * EyePosition.forward) * 10);
        Gizmos.DrawRay(EyePosition.transform.position, (Quaternion.Euler(0, discovery_player_status.VisionFieldAngle, 0) * EyePosition.forward) * 10);
    }

    public void DiscoveryPlayerStatusChange(DiscoveryPlayerStatus _discovery_player_status = null, bool original = false)
    {
        if (original == true)
        {
            discovery_player_status = original_discovery_player_status;
        }
        else
        {
            Debug.Log("DiscoveryPlayerStatusChange: " + _discovery_player_status);
            discovery_player_status = _discovery_player_status;
        }

    }
    public void RemovePlayerOfDiscoveryPlayerLists(GameObject _Player)
    {
        if (DiscoveriedPlayers.Count != 0)
        {
            if (DiscoveriedPlayers.Contains(_Player))
            {
                DiscoveriedPlayers.Remove(_Player);
                if (DiscoveriedPlayers.Count != 0)
                {
                    FirstKeyOfDiscoveriedPlayers = DiscoveriedPlayers[0];
                }
                else
                {
                    
                }
            }
        }

    }
    public void AddPlayerToDiscoveryPlayerLists(GameObject _Player)
    {
        //見つけたプレイヤーたちをすべて格納するための情報
        if (DiscoveriedPlayers.Count != 0)
        {
            //すでにプレイヤー追加されていないなら
            if (!DiscoveriedPlayers.Contains(_Player))
            {
                DiscoveriedPlayers.Add(_Player);
            }
        }
        else
        {
            //0個なら確認なしに挿入する。
            DiscoveriedPlayers.Add(_Player);
        }

        FirstKeyOfDiscoveriedPlayers = DiscoveriedPlayers[0];
    }
    public bool DiscoveryPlayerNowCheck()
    {
        if (FirstKeyOfDiscoveriedPlayers != null)
        {
            if (this.PlayersVisibilityMapStatuesWithKey[this.FirstKeyOfDiscoveriedPlayers].DiscoveryPlayerNow)
            {
                return true;
            }
        }
        return false;
    }
}
