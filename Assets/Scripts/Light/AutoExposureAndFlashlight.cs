using System;
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
    public float outdoorIntensity = 14f;
    public float indoorIntensity  = 6f;
    public float outdoorExposure  = 0.5f;
    public float indoorExposure   = -0.5f;

    [Header("壁近接 距離しきい値（屋外）")]
    [Tooltip("屋外：この距離以下から減光/露出オフセット開始")]
    public float outdoorDimStartDist = 1.2f;
    [Tooltip("屋外：この距離以下で最大減光/最大オフセット")]
    public float outdoorDimEndDist   = 0.30f;

    [Header("壁近接 距離しきい値（室内）")]
    [Tooltip("室内：この距離以下から減光/露出オフセット開始")]
    public float indoorDimStartDist = 0.9f;
    [Tooltip("室内：この距離以下で最大減光/最大オフセット")]
    public float indoorDimEndDist   = 0.25f;

    [Header("壁近接 係数（共通）")]
    [Range(0f, 1f)]
    [Tooltip("至近時のIntensity倍率（0.5=半分）")]
    public float nearIntensityFactor = 0.5f;
    [Tooltip("至近時のPost Exposure追加オフセット（負値推奨。例:-0.7）")]
    public float nearExposureDelta = -0.7f;

    [Header("判定と補間")]
    [Tooltip("Rayの検出最大距離（広げすぎると常時ヒットしやすい）")]
    public float maxProbeDist = 4f;
    [Tooltip("“正面ヒット度”（-ray・normal）。この値以上のときのみ近接扱い")]
    [Range(0f, 1f)] public float normalFacingThreshold = 0.6f;
    [Tooltip("露出/Intensityの補間速度")]
    public float lerpSpeed = 5f;
    [Tooltip("Raycastが当たるレイヤー")]
    public LayerMask hitMask = ~0;
    [Tooltip("ここに指定したタグはRaycastヒットを無視する")]
    public string[] excludeTags = new string[] { "Player", "IgnoreRaycast" };

    // 内部
    private float baseIntensity;    // 室内/屋外の基準Intensity
    private float baseExposure;     // 室内/屋外の基準Exposure
    private bool  isIndoor = false; // 現在の環境フラグ
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

        // 初期は屋外想定
        SetEnvironment(false);

        // しきい値の逆転を回避（屋外/室内両方チェック）
        SanitizeThresholds(ref outdoorDimStartDist, ref outdoorDimEndDist);
        SanitizeThresholds(ref indoorDimStartDist,  ref indoorDimEndDist);
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

        // 現在環境のしきい値を選択
        float activeStart = isIndoor ? indoorDimStartDist : outdoorDimStartDist;
        float activeEnd   = isIndoor ? indoorDimEndDist   : outdoorDimEndDist;

        // === 有効ヒット選別（タグ除外＆正面ヒット）→ 近接度tを算出 ===
        float t = 0f;
        var hits = Physics.RaycastAll(ray, maxProbeDist, hitMask, QueryTriggerInteraction.Ignore);
        if (hits != null && hits.Length > 0)
        {
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance)); // 近い順

            foreach (var h in hits)
            {
                // タグ除外
                if (excludeTags != null && excludeTags.Length > 0)
                {
                    bool excluded = false;
                    foreach (var tag in excludeTags)
                    {
                        if (!string.IsNullOrEmpty(tag) && h.collider.CompareTag(tag))
                        { excluded = true; break; }
                    }
                    if (excluded) continue;
                }

                // 正面ヒット度
                float facing = Vector3.Dot(-ray.direction.normalized, h.normal.normalized);
                if (facing < normalFacingThreshold) continue;

                // 距離に応じた近接度t（0=影響なし, 1=最大）
                float dist = h.distance;
                if (dist <= activeStart)
                {
                    t = Mathf.Clamp01((activeStart - dist) / (activeStart - activeEnd));
                }
                break; // 条件を満たした最初のヒットで決定
            }
        }

        // === 近接効果の適用 ===
        float targetIntensity = baseIntensity * Mathf.Lerp(1f, nearIntensityFactor, t);
        float targetExposure  = baseExposure  + Mathf.Lerp(0f, nearExposureDelta, t);

        // === 補間して適用 ===
        flashlight.intensity = Mathf.Lerp(flashlight.intensity, targetIntensity, Time.deltaTime * lerpSpeed);
        colorAdjust.postExposure.value = Mathf.Lerp(colorAdjust.postExposure.value, targetExposure, Time.deltaTime * lerpSpeed);
    }

    // 室内外の切替（トリガー）
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("IndoorZone")) SetEnvironment(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("IndoorZone")) SetEnvironment(false);
    }

    // ---- Helper ----
    void SetEnvironment(bool indoor)
    {
        isIndoor = indoor;
        baseIntensity = isIndoor ? indoorIntensity : outdoorIntensity;
        baseExposure  = isIndoor ? indoorExposure  : outdoorExposure;

        // 初期適用（スナップ）
        if (flashlight) flashlight.intensity = baseIntensity;
        colorAdjust.postExposure.Override(baseExposure);
    }

    static void SanitizeThresholds(ref float start, ref float end)
    {
        if (start <= end) start = end + 0.05f; // 逆転防止の微調整
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = isIndoor ? Color.magenta : Color.cyan;
        Vector3 o, d;
        if (rayOrigin) { o = rayOrigin.position; d = rayOrigin.forward; }
        else if (Camera.main) { o = Camera.main.transform.position; d = Camera.main.transform.forward; }
        else { o = transform.position; d = transform.forward; }
        Gizmos.DrawLine(o, o + d.normalized * maxProbeDist);
    }
#endif
}
