// LightCullingGroup.cs
using System.Linq;
using UnityEngine;

public class LightCullingGroup : MonoBehaviour
{
    [Header("参照（未設定ならMainCameraに）")]
    public Transform reference; // プレイヤーやカメラ

    [Header("距離バンド（小→大）")]
    public float shadowDist = 14f;   // 影ON
    public float enableDist = 32f;   // 影OFFだがライトON
    public float disableDist = 36f;  // ライトOFF

    [Header("動的ライトを拾う条件")]
    public bool includeDirectional = false;

    CullingGroup cg;
    BoundingSphere[] spheres;
    Light[] lights;
    Camera cam;
    int n;
    float updatePosInterval = 0.25f; // ライト位置を何秒ごとに同期
    float t;

    void OnEnable()
    {
        cam = Camera.main;
        if (!reference) reference = cam ? cam.transform : null;

        // シーン内のライト収集（必要なら自前登録制に）
        lights = FindObjectsOfType<Light>(true)
            .Where(l => l && (includeDirectional || l.type != LightType.Directional))
            .ToArray();

        n = lights.Length;
        spheres = new BoundingSphere[n];
        for (int i = 0; i < n; i++)
        {
            var l = lights[i];
            var r = (l.type == LightType.Point || l.type == LightType.Spot) ? Mathf.Max(1f, l.range) : 10f;
            spheres[i] = new BoundingSphere(l.transform.position, r);
        }

        cg = new CullingGroup();
        if (cam) cg.targetCamera = cam;              // 画角外も切れる
        if (reference) cg.SetDistanceReferencePoint(reference);
        cg.SetBoundingSpheres(spheres);
        cg.SetBoundingSphereCount(n);
        cg.SetBoundingDistances(new float[] { shadowDist, enableDist, disableDist });
        cg.onStateChanged += OnStateChanged;
    }

    void LateUpdate()
    {
        // ライトが動く可能性があるなら、たまに位置だけ同期
        t += Time.deltaTime;
        if (t < updatePosInterval) return;
        t = 0f;

        for (int i = 0; i < n; i++)
        {
            var l = lights[i];
            if (!l) continue;
            spheres[i].position = l.transform.position;
            // rangeが変わるなら radius も更新すること
            // spheres[i].radius = Mathf.Max(1f, l.range);
        }
    }

    void OnStateChanged(CullingGroupEvent e)
    {
        var l = lights[e.index];
        if (!l) return;

        // currentDistance: 0=<shadowDist, 1=[shadowDist, enableDist), 2=[enableDist, disableDist), 3=それ以上
        // ついでに画角外は切る（isVisible）
        if (!e.isVisible || e.currentDistance >= 3)
        {
            l.enabled = false;
            return;
        }

        l.enabled = true;
    }

    void OnDisable()
    {
        if (cg != null)
        {
            cg.onStateChanged -= OnStateChanged;
            cg.Dispose();
            cg = null;
        }
    }
}
