using UnityEngine;
[ExecuteInEditMode]
public class CircleArranger : MonoBehaviour
{
    [Header("配置したいキャラクター(子オブジェクト)をドラッグ")]
    public Transform[] characters;

    [Header("円の半径")]
    public float radius = 2f;

    [Header("キャラクターが円の中心を向くかどうか")]
    public bool lookAtCenter = true;

    void Update()
    {
        if (characters == null || characters.Length == 0)
        {
            Debug.LogWarning("characters が設定されていない、または 0 件です。");
            return;
        }

        // このスクリプトがアタッチされたGameObjectを “中心” とみなす
        // もし "Center" オブジェクトにこのスクリプトを付けていれば、centerPos は transform.position
        // ただし、円の配置に使うのはローカル座標にしたいので (0,0,0) とみなすことが多い
        // ここでは「子オブジェクトのローカル座標」で配置していきます。

        for (int i = 0; i < characters.Length; i++)
        {
            // 360度を characters.Length 個で分割
            float angle = i * (360f / characters.Length);
            float rad = angle * Mathf.Deg2Rad;

            // ローカル座標での円形配置
            float x = Mathf.Cos(rad) * radius;
            float z = Mathf.Sin(rad) * radius;
            // y は 0 のままにしておく (足元合わせなどがあるなら調整)

            // キャラクターを子にする (念のため)
            // すでに子階層にしているなら不要です
            characters[i].SetParent(this.transform);

            // ローカル座標を設定（= “Center” の原点から見て円形に並ぶ）
            characters[i].localPosition = new Vector3(x, 0f, z);

            if (lookAtCenter)
            {
                // 「Center」の位置(= ローカル原点) を向かせる
                // localPosition ゆえに、LookAt もワールド座標でなくローカル参照したいところですが
                // Unityの LookAt はワールド空間を使うので、一旦 transform.position に合わせて実装します

                // ワールド空間でいうと「自分（Center）の座標」を向かせる
                characters[i].LookAt(this.transform.position);
            }
        }
    }
}
