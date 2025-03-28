using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SplineGroundSnap))]
public class SplineGroundSnapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // ターゲットとなる SplineGroundSnap コンポーネントを取得
        SplineGroundSnap snapper = (SplineGroundSnap)target;

        // スナップボタン
        if (GUILayout.Button("Snap To Ground"))
        {
            // スナップ処理
            snapper.SnapPointsToGround();
            
            // シーンを再描画して結果を反映 (エディタの場合)
            SceneView.RepaintAll();
        }
    }
}
