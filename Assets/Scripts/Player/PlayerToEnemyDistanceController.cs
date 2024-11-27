using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerToEnemyDistanceController : MonoBehaviour
{
    //敵との距離で心拍音を上げる。
    [SerializeField] private float to_enemy_distance;

    private HorrorDirector horror_director;

    public bool NearEnemyStart;
    public bool AwayEnemyWhenNearEnemyTrue;

    public bool NearEnemyNow;

    public bool AwayEnemy;

    [SerializeField]private float AwayEnemyTrueTimeNow;
    //NearEnemyがtrueのとき どのくらいtrueなのかを時間計測
    [SerializeField] private float AwayEnemyWhenNearEnemyTrueTimeNow;
    // Start is called before the first frame update
    void Start()
    {
        horror_director = HorrorDirector.instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (NearEnemyStart)
        {
            //NearEnemyStartは一フレームだけ通るようにしておく。
            if (!NearEnemyNow)
            {
                //NearEnemyNowがfalseの場合(すでに敵が近くにいるならこの関数は実行しない)
                NearStartDistaceEvent();
            }
            NearEnemyNow = true;

            NearEnemyStart = false;
        }
        if (NearEnemyNow)
        {
            //NearEnemyNowがtrueならNearDistanceNowEventを実行します。
            NearDistanceNowEvent();
            if (AwayEnemy)
            {
                //敵がプレイヤーから離れてから、数秒間たった場合完全にFarDistance実行。
                //もし、１りの敵が離れても、ほかの敵が近くにいるのであれば、このtimeは0となる。
                AwayEnemyTrueTimeNow += Time.deltaTime;
                if(AwayEnemyTrueTimeNow > 2)
                {
                    FarDistanceEvent();
                    ResetWhenFar();
                }
            }
            else
            {
                AwayEnemyTrueTimeNow = 0;
            }
        }
    }
    private void FixedUpdate()
    {
        AwayEnemy = true;
    }
    private void NearStartDistaceEvent()
    {
        horror_director.NearEnemy();
    }
    private void NearDistanceNowEvent()
    {
    }

    private void FarDistanceEvent()
    {
        horror_director.NearEnemyReset();
        NearEnemyNow = false;
    }

    private void ResetWhenFar()
    {
        NearEnemyNow = false;
        AwayEnemyTrueTimeNow = 0;
    }
}
