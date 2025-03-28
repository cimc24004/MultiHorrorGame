using UnityEngine;
using UnityEngine.Splines;

[ExecuteInEditMode]  // エディタ上でも動かしたい場合
public class SplineGroundSnap : MonoBehaviour
{
    [SerializeField]
    private SplineContainer splineContainer;
    
    [SerializeField, Tooltip("レイを飛ばす最大距離")]
    private float raycastDistance = 1000f;
    
    [SerializeField, Tooltip("地面として判定する Layer")]
    private LayerMask groundLayer;
    
    /// <summary>
    /// ノットを地面にスナップさせる処理
    /// </summary>
    public void SnapPointsToGround()
    {
        if (splineContainer == null)
        {
            Debug.LogWarning("SplineContainer がアサインされていません。");
            return;
        }

        // com.unity.splines パッケージの Spline 参照を取得
        Spline spline = splineContainer.Spline;

        // Spline 内のすべての Knot（コントロールポイント）を更新
        for (int i = 0; i < spline.Count; i++)
        {
            // 現在のノット情報を取得
            BezierKnot knot = spline[i];

            // レイをノットの真下方向に飛ばす
            Ray ray = new Ray(knot.Position, Vector3.down);

            // レイキャストして地面にヒットしたら、そのヒット座標にノットを移動
            if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, groundLayer))
            {
                // 位置をヒット地点に
                knot.Position = hit.point;

                // ↓ もしノーマル方向に合わせたいなら、以下のような回転調整をしてみてください。
                // var upBefore = knot.Rotation * Vector3.up;  // Knotが持っている"上"方向
                // var rotationToNormal = Quaternion.FromToRotation(upBefore, hit.normal);
                // knot.Rotation = rotationToNormal * knot.Rotation;

                // ノットを更新して反映
                spline[i] = knot;
            }
        }

        // もしエディタ上で Undo 機能を使いたい場合は以下のように Undo 登録してください。
        // Undo.RegisterCompleteObjectUndo(splineContainer, "Spline Snap to Ground");
        // そして値更新後に強制的にシーンを再描画したい場合は
        // EditorUtility.SetDirty(splineContainer);
    }
}
