using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
public class EnemyRoutingController : SerializedMonoBehaviour
{
    private StageRoutings stage_routings;
    private FollowObjectByNav follow_object_by_nav;
    private EnemyStatus enemy_status;
    private float WaitToMoveNextPointTimeNow;
    private TimeManager time_manager;

    public float ToNextRouteDistance;
    [Header("次のポイントまで移動するまで少し待つその時間")]
    public float WaitToMoveNextPointTime;
    [Header("徘徊の歩くスピード")]
    public float RoutingWalkSpeed = 1;
    public GameObject NextRouteObject;
    //最後のルートまで言ったら0まで減らす。 デフォルトは++で増やしていく。
    public bool ReverseIndexChange;
    //RoutingNowObjectに到着したかどうか
    public bool ArrivedNextRouteMode = false;
    public bool RoutingMovePermit = true;

    public int RouteIndexNow = 0;

    //いちばん近いRoute
    public GameObject NearRouteObject;

    public bool ArrivedTarget;

    [Header("ルートを独自に指定するかどうか")]
    public bool OriginalRouting;

    [ShowIf("OriginalRouting")]
    public List<GameObject> Routings;
    // Start is called before the first frame update
    void Start()
    {
        //StageRoutingsからルーティングたちを取得。
        stage_routings = StageController.instance.gameObject.GetComponent<StageRoutings>();
        follow_object_by_nav = this.gameObject.GetComponent<FollowObjectByNav>();
        enemy_status = this.gameObject.GetComponent<EnemyStatus>();
        time_manager = TimeManager.instance;

        if (!OriginalRouting)
        {
            Routings = stage_routings.Routings;
        }
        //最初にルートを設定。
        NextRouteSet();
    }

    // Update is called once per frame
    void Update()
    {
        if (!RoutingMovePermit) return;
        if (ArrivedNextRouteMode)
        {
            //すでに進んでいるルートに到着していたら
            if (Routings.Count > RouteIndexNow + 1)
            {
                //次のルートにいく。
                if (!ReverseIndexChange)
                {
                    RouteIndexNow += 1;
                }
                else
                {
                    if(RouteIndexNow == 0)
                    {
                        //逆順にルートを徘徊していて、もし最後(最初のルート)に到着したらReverseを元に戻して+=1していく動作を作る。
                        RouteIndexNow += 1;
                        ReverseIndexChange = false;
                    }
                    else
                    {
                        RouteIndexNow -= 1;
                    }
                }
                NextRouteSet();
            }
            else
            {
                //最後のルートに到着したらReverseIndexChangeにTrueを指定。
                //逆順にルートをたどるようにする。
                ReverseIndexChange = true;
                RouteIndexNow -= 1;
                NextRouteSet();
            }

            ArrivedNextRouteMode = false;
        }
        follow_object_by_nav.ChaseToTargetSet(true, NextRouteObject.transform.position, RoutingWalkSpeed);

        if (!ArrivedNextRouteMode)
        {
            //まだ到着していない最中でしたら
            //follow_object_by_nav.StoppingDistance = 0.8f;
            ToNextRouteDistance = (float)Math.Round(Vector3.Distance(NextRouteObject.transform.position, this.transform.position), 1);
            //if (follow_object_by_nav.ArrivedTarget) ArrivedTarget = true;
            if (ToNextRouteDistance <= follow_object_by_nav.StoppingDistance + 0.1f)
            {
                ArrivedTarget = true;
            }
            //FollowObjectByNavがArrivedtargetになったら　これは一フレームだけしかTrueにならないので、継続させるために
            //ArrivedTarget変数を宣言しておいて、そこに収納しておく。。
            if (ArrivedTarget)
            {
                enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Stop;
                //いま向かっているルートに到着したら少し待ってから次のルートにいく。
                WaitToMoveNextPointTimeNow += Time.deltaTime;
                if(WaitToMoveNextPointTimeNow > WaitToMoveNextPointTime)
                {
                    ArrivedNextRouteMode = true;
                    ArrivedTarget = false;
                    WaitToMoveNextPointTimeNow = 0;
                }
            }
            else
            {
                enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Walk;
            }
        }
        else
        {
            ArrivedTarget = false;
        }

    }


    public void PermitFunction(bool _permit)
    {
        //これでpermitを切り替えるのが基本
        if (_permit)
        {
            RoutingMovePermit = true;

        }
        else
        {
            follow_object_by_nav.ArrivedTarget = false;
            RoutingMovePermit = false;
        }
    }
    [Button]
    public void NearRouteSet()
    {
        //いちばん近く(仮)のルートをセットする関数
        enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Walk;
        follow_object_by_nav.ArrivedTarget = false;
        this.ArrivedTarget = false;
        this.PermitFunction(true);
        if (OriginalRouting) {
            //オリジナルルートどおりなら、元の配置に戻るように
            this.NextRouteObject = Routings[RouteIndexNow];
            this.NextRouteSet(this.NextRouteObject);
            return;
        } 

        //もし近くにルートがなかった場合はRouteIndexNowのものに対して進むようにする
        if (this.NearRouteObject == null) this.NearRouteObject = Routings[RouteIndexNow];
        this.NextRouteSet(this.NearRouteObject);
    }
    [Button]
    public void NextRouteSet(GameObject _NextRouteObject = null)
    {
        //次に行くルートを設定する関数。

        //次に行くルートに指定がなかったらIndexで順番通りに行く。
        if(_NextRouteObject == null)
        {
            NextRouteObject = Routings[RouteIndexNow];
        }
        else
        {
            //ルートに指定があったら指定されたルートに向かって進行。
            NextRouteObject = _NextRouteObject;

            //それとIndexも変更する。 NextRouteObjectに到着したらルートどおりに徘徊する。
            RouteIndexNow = Routings.IndexOf(NextRouteObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Route")
        {
            NearRouteObject = other.gameObject;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        
    }
}
