using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowedEnemyHitController : MonoBehaviour
{
    public GameObject HitedEnemy;
    public ItemStatus item_status;

    public void EnemyThrowCollisionEvent()
    {
        //敵にアイテムが当たったときのイベント
        if (item_status.MakingStopEnemyItem)
        {
            //敵を止めることができるアイテムだったら？
            //ストップします。
            HitedEnemy.GetComponent<EnemyStatus>().StopEnemyByPlayer(item_status.status_of_making_stop_enemy.MakingStopTime);
        }
    }

}
