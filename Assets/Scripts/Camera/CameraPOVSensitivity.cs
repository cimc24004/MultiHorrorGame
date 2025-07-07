using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class CameraPOVSensitivity : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private float minSensitivity = 0.1f;
    [SerializeField] private float maxSensitivity = 2.0f;

    private ConfigController config_controller;

    private Slider2 sensitivitySlider;
    private CinemachinePOV povComponent;

    void Start()
    {
        config_controller = ConfigController.instance;
        if (virtualCamera == null)
        {
            Debug.LogError("CinemachineVirtualCameraがアタッチされていません。");
            return;
        }

        if (sensitivitySlider == null)
        {
            Debug.LogError("Sliderがアタッチされていません。");
            return;
        }

        // CinemachinePOVコンポーネントの取得
        povComponent = virtualCamera.GetCinemachineComponent<CinemachinePOV>();
        if (povComponent == null)
        {
            Debug.LogError("CinemachinePOVが見つかりません。このスクリプトはPOVモードに対応しています。");
            return;
        }

        // Sliderの初期設定
        sensitivitySlider.value = povComponent.m_HorizontalAxis.m_MaxSpeed;
        sensitivitySlider.maxValue = maxSensitivity;
        sensitivitySlider.minValue = minSensitivity;
        // Sliderの値変更イベントにリスナーを追加
    }
    private void Update()
    {
        // UpdateSensitivity(mouse_sens_config_status.value);
    }
    private void UpdateSensitivity(float sensitivity)
    {
        if (povComponent != null)
        {
            // 水平方向と垂直方向の感度を更新
            povComponent.m_HorizontalAxis.m_MaxSpeed = sensitivity;
            povComponent.m_VerticalAxis.m_MaxSpeed = sensitivity;
        }
    }
}
