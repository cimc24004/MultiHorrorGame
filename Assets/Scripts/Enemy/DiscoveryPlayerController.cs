using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscoveryPlayerController : MonoBehaviour
{
    public bool DiscoveryPlayerNow;

    public List<string> ObstacleLayers;
    public Transform EyePosition;
    [Header("視野角")]
    public float VisionFieldAngle;
    [Header("Playerを見つけられる範囲")]
    public float PlayerDiscoveryRange;
    [Header("Playerを見つけた時追いかけるスピード")]
    public float SpeedWhenDiscoveried;

    //特定のレイヤーオブジェクトのみで視野判定を行っていくための設定
    public List<string> layer_mask_lists = new List<string>{ "Player", "Object" };
    //プレイヤーが自分の視界から外れている間の時間
    public float SightOutPlayerTime = 1;
    //プレイヤーを視認しているかどうか
    public bool SightningPlayer;
    private int layerMask;
    private GameObject Player;
    private Transform PlayersHeadTransform;
    private FollowObjectByNav follow_object_nav;
    private PlayerStatus player_status;
    private RaycastHit raycast_hit;
    //プレイヤーが見つけられる範囲以内にいるかどうか。
    private bool InsidePlayer;
    private Vector3 direction;
    private Vector3 DefaultPos;
    private float ToPlayerDistance;
    //プレイヤーが自分の視界から外れている間の時間Now
    public float SightOutPlayerTimeNow;
    // Start is called before the first frame update
    void Start()
    {
        player_status = PlayerStatus.instance;

        Player = player_status.gameObject;
        PlayersHeadTransform = player_status.Head;
        layerMask = LayerMask.GetMask(layer_mask_lists.ToArray());
        DefaultPos = this.transform.position;
        follow_object_nav = this.gameObject.GetComponent<FollowObjectByNav>();
    }

    // Update is called once per frame
    void Update()
    {
        direction = (PlayersHeadTransform.transform.position - EyePosition.transform.position).normalized;
        if (Vector3.Distance(Player.transform.position, this.transform.transform.position) <= PlayerDiscoveryRange)
            InsidePlayer = true;
        else
            InsidePlayer = false;
        if (DiscoveryPlayerNow)
        {
            DiscoveryPlayer(true);
        }
        if (DiscoveryPlayerNow && !SightningPlayer)
        {
            //敵に見つかっているかつ視認状態がfalseならば
            //時間を計り、ある程度の時間がたったら追いかけるのをやめる
            SightOutPlayerTimeNow += TimeManager.instance.deltaTime;
            if (SightOutPlayerTimeNow > SightOutPlayerTime)
            {
                Debug.Log("時間経過しましたよ");
                DiscoveryPlayer(false);
                //敵が追いかけてる最中もしプレイヤーを見失った場合は元の位置に戻る
                follow_object_nav.ChaseToTargetSet(true, DefaultPos, SpeedWhenDiscoveried);
                SightOutPlayerTimeNow = 0;
            }

        }
        else
        {
            SightOutPlayerTimeNow = 0;
        }


        if (Vector3.Angle(direction, EyePosition.forward) <= VisionFieldAngle && InsidePlayer)
        {
            //プレイヤーが敵の視界以内にいるかどうか && プレイヤーが範囲以内にいるかどうか。
            if(Physics.Raycast(EyePosition.transform.position, direction, out raycast_hit,layerMask))
            {
                //Raycastをプレイヤーにとりあえず飛ばす。

                if (!ObstacleLayers.Contains(LayerMask.LayerToName(raycast_hit.collider.gameObject.layer)))
                {
                    //プレイヤーにRaycastが当たっているが、まずその前に障害物が存在する場合は見つからなかった判定にしておく。
                    if (raycast_hit.collider.gameObject.tag == "Player")
                    {
                        Debug.Log("Playerが敵に見つかりました！");
                        //プレイヤーを視認しているナウ。
                        SightningPlayer = true;
                        //即見つかる判定ではある。
                        DiscoveryPlayerNow = true;
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
            Debug.DrawRay(EyePosition.transform.position, direction * 1000, Color.red);
        }
        else
        {
            SightningPlayer = false;
        }
    }
    private void DiscoveryPlayer(bool _discovery)
    {
        //敵に見つかったとき、または見つかってる状態をリセットしたい時に使う。
        //Trueならば見つかったときのイベント、Falseならば見つからなくなったときにリセットするための関数
        DiscoveryPlayerNow = _discovery;
        if (_discovery)
        {
            follow_object_nav.ChaseToTargetSet(true, Player.transform.position, SpeedWhenDiscoveried);
        }
        else
        {
            follow_object_nav.ChaseToTargetSet(true, DefaultPos, SpeedWhenDiscoveried);
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(EyePosition.transform.position, (Quaternion.Euler(0, -VisionFieldAngle, 0) * EyePosition.forward) * 10);
        Gizmos.DrawRay(EyePosition.transform.position, (Quaternion.Euler(0, VisionFieldAngle, 0) * EyePosition.forward) * 10);
    }
}
