using UnityEngine;
using UnityEngine.Rendering.Universal;
public class MainCameraController : MonoBehaviour
{
    private Camera MainCamera;     // Stack を持つベースカメラ
    private void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += OnLocalPlayerSpawned;
    }
    private void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= OnLocalPlayerSpawned;
    }
    private void OnLocalPlayerSpawned(PlayerStatus _playerStatus)
    {
        Camera item_equip_camera = _playerStatus.item_equip_controller.ItemEquipCamera;
        MainCamera = this.gameObject.GetComponent<Camera>();
        // 1. どっちのカメラも UniversalAdditionalCameraData を持つようにする
        AddCameraToMainCameraStack(item_equip_camera);
    }
    private void AddCameraToMainCameraStack(Camera _overlayCamera)
    {
        var baseCamData = MainCamera.GetUniversalAdditionalCameraData();

        // 3. ベースカメラのスタックに追加
        if (!baseCamData.cameraStack.Contains(_overlayCamera))
            baseCamData.cameraStack.Add(_overlayCamera);
    }
    
}
