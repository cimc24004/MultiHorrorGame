using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerToEnemyDistanceController : MonoBehaviour
{
    // 範囲内の敵を追跡するリスト
    [SerializeField]
    private List<Collider> enemiesInRange = new List<Collider>();

    public bool NearEnemy;

    private HorrorDirector horror_director;

    private void Start()
    {
        horror_director = HorrorDirector.instance;
    }

    private void Update()
    {
        if (NearEnemy)
        {
        }
    }
    // トリガーに敵が入った時
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerNearSensor")) // "Enemy"タグが付いたオブジェクトのみ反応
        {
            // リストに敵を追加
            if (!enemiesInRange.Contains(other))
            {
                enemiesInRange.Add(other);
                horror_director.AddNearEnemy();
            }
        }
    }

    // トリガーから敵が出た時
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerNearSensor")) // "Enemy"タグが付いたオブジェクトのみ反応
        {
            // リストから敵を削除
            if (enemiesInRange.Contains(other))
            {
                enemiesInRange.Remove(other);
                horror_director.RemoveNearEnemy();
            }
        }
    }

}
