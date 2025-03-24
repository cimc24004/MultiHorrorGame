using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MichibikinoHuurinCollider : MonoBehaviour
{
    [SerializeField]
    private MichibikinoHuurinController michibikino_huurin_controller;

    // 「どれくらいの半径を探すか」指定
    [SerializeField] private float detectionRadius = 5f;

    [SerializeField] private Collider[] hits;

    [SerializeField] private LayerMask OnlyLayerMask;
    void Update()
    {
        // 1) 周囲 detectionRadius 内の Collider を全部取得
        hits = Physics.OverlapBox(transform.position, new Vector3(1,0.7f,1) * detectionRadius,Quaternion.identity, OnlyLayerMask);
        // 今回検知した Butsudan を一時的に集めておく
        HashSet<GameObject> foundButsudan = new HashSet<GameObject>();

        // 2) 衝突したオブジェクトを総なめチェック
        foreach (var hit in hits)
        {
            // タグが "Butsudan" のものだけ処理
            if (hit.CompareTag("Butsudan"))
            {
                // 見つけた Butsudan を記録
                foundButsudan.Add(hit.gameObject);

                // 現在の距離を計算
                float dist = Vector3.Distance(transform.position, hit.transform.position);

                // Dictionary にまだ無いなら追加、あれば距離更新
                if (!michibikino_huurin_controller.MichibikiObjectWithDistanceIntoRange.ContainsKey(hit.gameObject))
                {
                    michibikino_huurin_controller.MichibikiObjectWithDistanceIntoRange.Add(hit.gameObject, dist);
                }
                else
                {
                    michibikino_huurin_controller.MichibikiObjectWithDistanceIntoRange[hit.gameObject] = dist;
                }
            }
        }

        // 3) 今回見つからなかった (範囲外に消えた) Butsudan を Dictionary から削除
        var keysToRemove = new List<GameObject>();
        foreach (var kvp in michibikino_huurin_controller.MichibikiObjectWithDistanceIntoRange)
        {
            if (!foundButsudan.Contains(kvp.Key))
            {
                keysToRemove.Add(kvp.Key);
            }
        }
        foreach (var key in keysToRemove)
        {
            michibikino_huurin_controller.MichibikiObjectWithDistanceIntoRange.Remove(key);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(1,0.7f,1) * detectionRadius);
    
    }
}
