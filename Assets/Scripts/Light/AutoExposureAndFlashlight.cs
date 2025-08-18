using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AutoExposureAndFlashlight : MonoBehaviour
{
    [Header("Volume設定")]
    public Volume volume;  // Global VolumeをInspectorから指定
    private ColorAdjustments colorAdjust;
    private FlashLightController flashLightController;
    [Header("懐中電灯設定")]
    public Light flashlight;      // Spot Lightを指定
    public float indoorIntensity = 6f;   // 室内ライト強さ
    public float outdoorIntensity = 14f; // 屋外ライト強さ

    [Header("露出設定")]
    public float indoorExposure = -0.5f; // 室内
    public float outdoorExposure = 0.5f; // 屋外

    [Header("共通パラメータ")]
    public float lerpSpeed = 2f; // 補間速度（大きいほど切替が速い）

    // 内部用
    private float targetExposure;
    private float targetIntensity;

    void Start()
    {
        flashLightController = this.gameObject.GetComponent<PlayerStatus>().flash_light_controller;
        flashlight = flashLightController.Light;
        volume = GameObject.FindWithTag("GlobalVolume").GetComponent<Volume>();
        if (volume.profile.TryGet(out colorAdjust))
        {
            // 初期は屋外にいると仮定
            targetExposure = outdoorExposure;
            targetIntensity = outdoorIntensity;

            colorAdjust.postExposure.Override(targetExposure);
            if (flashlight != null)
                flashlight.intensity = targetIntensity;
        }
        else
        {
            Debug.LogError("Global VolumeにColor Adjustmentsを追加して！");
        }
    }

    void Update()
    {
        if (colorAdjust == null || flashlight == null) return;

        // 露出を補間
        colorAdjust.postExposure.value = Mathf.Lerp(
            colorAdjust.postExposure.value,
            targetExposure,
            Time.deltaTime * lerpSpeed
        );

        // ライト強度を補間
        flashlight.intensity = Mathf.Lerp(
            flashlight.intensity,
            targetIntensity,
            Time.deltaTime * lerpSpeed
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("IndoorZone"))
        {
            targetExposure = indoorExposure;
            targetIntensity = indoorIntensity;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("IndoorZone"))
        {
            targetExposure = outdoorExposure;
            targetIntensity = outdoorIntensity;
        }
    }
}
