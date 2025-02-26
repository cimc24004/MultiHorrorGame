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
            horror_director.NearEnemy();
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

                // 初めての敵が入った場合、近づいたイベントを実行
                if (enemiesInRange.Count == 1)
                {
                    OnEnemyApproach();
                }
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

                // 全ての敵が範囲外に出た場合、離れたイベントを実行
                if (enemiesInRange.Count == 0)
                {
                    OnEnemyLeave();
                }
            }
        }
    }

    // 敵が近づいたときのイベント
    private void OnEnemyApproach()
    {
        Debug.Log("敵が範囲内に近づいた！");
        // 特定のイベントをここに実行
        NearEnemy = true;
    }

    // 敵が全て範囲外に出たときのイベント
    private void OnEnemyLeave()
    {
        Debug.Log("敵が全て範囲外に離れた！");
        // 特定のイベントをここに実行
        NearEnemy = false;
        horror_director.NearEnemyReset();
    }
}
