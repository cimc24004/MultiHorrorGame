using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[DisallowMultipleComponent]
public class AutoExposureFlashlight : MonoBehaviour
{
    [Header("Volume設定")]
    [Tooltip("Color Adjustments(Post Exposure)入りの Global/Local Volume")]
    public Volume volume;  // 未設定なら自動で探す
    private ColorAdjustments colorAdjust;

    [Header("懐中電灯設定")]
    [Tooltip("懐中電灯に使う Spot Light")]
    public Light flashlight;
    [Tooltip("Rayの基準。未指定ならViewport中央(0.5,0.5)から撃つ")]
    public Transform rayOrigin; // 任意（カメラ推奨）

    [Header("室内/屋外の基準値")]
    [Tooltip("屋外での基準Intensity")]
    public float outdoorIntensity = 14f;
    [Tooltip("室内での基準Intensity")]
    public float indoorIntensity = 6f;
    [Tooltip("屋外での基準Post Exposure(+で明るく)")]
    public float outdoorExposure = 0.5f;
    [Tooltip("室内での基準Post Exposure(-で暗く)")]
    public float indoorExposure = -0.5f;

    [Header("壁近接 減衰(距離)")]
    [Tooltip("この距離以下から“近接減光/露出オフセット”を開始")]
    public float dimStartDist = 1.2f;      // ここより遠い→影響なし
    [Tooltip("この距離以下で“最大減光/最大オフセット”")]
    public float dimEndDist   = 0.30f;     // 至近距離
    [Tooltip("Rayの検出最大距離（広げすぎると常時ヒットしやすい）")]
    public float maxProbeDist = 4f;

    [Header("壁近接 係数")]
    [Range(0f, 1f)]
    [Tooltip("至近時のIntensity倍率（0.5=半分）")]
    public float nearIntensityFactor = 0.5f;
    [Tooltip("至近時のPost Exposure追加オフセット（負値推奨。例:-0.7）")]
    public float nearExposureDelta = -0.7f; // 0→遠い時, 近いほどこの値に近づく（暗くする）

    [Header("判定と補間")]
    [Tooltip("“正面ヒット度”（-ray・normal）。この値以上の時だけ近接扱いにする")]
    [Range(0f, 1f)] public float normalFacingThreshold = 0.6f;
    [Tooltip("露出/Intensityの補間速度")]
    public float lerpSpeed = 5f;
    [Tooltip("Raycastが当たるレイヤー")]
    public LayerMask hitMask = ~0;

    // 内部
    private float baseIntensity;    // 室内/屋外の基準Intensity
    private float baseExposure;     // 室内/屋外の基準Exposure
    private Camera mainCamera;

    void Awake()
    {
        if (!volume)
        {
            var tagged = GameObject.FindWithTag("GlobalVolume");
            if (tagged) volume = tagged.GetComponent<Volume>();
            if (!volume) volume = FindObjectOfType<Volume>();
        }
    }

    void Start()
    {
        mainCamera = Camera.main;

        if (!volume || volume.profile == null || !volume.profile.TryGet(out colorAdjust))
        {
            Debug.LogError("Color Adjustments(Post Exposure) が入った Volume を割り当ててね。");
            enabled = false; return;
        }

        // 初期は屋外扱い
        baseIntensity = outdoorIntensity;
        baseExposure  = outdoorExposure;

        colorAdjust.postExposure.Override(baseExposure);
        if (flashlight) flashlight.intensity = baseIntensity;

        if (dimStartDist <= dimEndDist) dimStartDist = dimEndDist + 0.1f; // 逆転防止
    }

    void Update()
    {
        if (!colorAdjust || !flashlight) return;

        // === Ray生成（rayOrigin優先／未指定ならViewport中央） ===
        Ray ray;
        if (rayOrigin) ray = new Ray(rayOrigin.position, rayOrigin.forward);
        else
        {
            if (!mainCamera) mainCamera = Camera.main;
            if (!mainCamera) return;
            ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        }

        // === 近接判定：遠ければ影響0、近ければ 0→1 に近づく t を作る ===
        float t = 0f;
        if (Physics.Raycast(ray, out var hit, maxProbeDist, hitMask, QueryTriggerInteraction.Ignore))
        {
            // “ほぼ正面ヒット”だけ近接扱い（地面の浅い当たり等で誤反応しないように）
            float facing = Vector3.Dot(-ray.direction.normalized, hit.normal.normalized);
            if (facing >= normalFacingThreshold)
            {
                float dist = hit.distance;
                if (dist <= dimStartDist)
                {
                    t = Mathf.Clamp01((dimStartDist - dist) / (dimStartDist - dimEndDist));
                    // dist = dimStartDist → t=0（影響なし）
                    // dist = dimEndDist   → t=1（最大影響）
                }
            }
        }

        // === 近接効果の適用 ===
        // Intensity：基準×(1→nearIntensityFactor を t で補間)
        float targetIntensity = baseIntensity * Mathf.Lerp(1f, nearIntensityFactor, t);

        // Exposure：基準 + (0→nearExposureDelta を t で補間)  ※nearExposureDeltaは負値推奨
        float targetExposure  = baseExposure  + Mathf.Lerp(0f, nearExposureDelta, t);

        // === 補間して適用 ===
        flashlight.intensity = Mathf.Lerp(flashlight.intensity, targetIntensity, Time.deltaTime * lerpSpeed);
        colorAdjust.postExposure.value = Mathf.Lerp(colorAdjust.postExposure.value, targetExposure, Time.deltaTime * lerpSpeed);
    }

    // 屋内外の切替（トリガー）
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("IndoorZone"))
        {
            baseIntensity = indoorIntensity;
            baseExposure  = indoorExposure;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("IndoorZone"))
        {
            baseIntensity = outdoorIntensity;
            baseExposure  = outdoorExposure;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 o, d;
        if (rayOrigin) { o = rayOrigin.position; d = rayOrigin.forward; }
        else if (Camera.main) { o = Camera.main.transform.position; d = Camera.main.transform.forward; }
        else { o = transform.position; d = transform.forward; }
        Gizmos.DrawLine(o, o + d.normalized * maxProbeDist);
    }
#endif
}
