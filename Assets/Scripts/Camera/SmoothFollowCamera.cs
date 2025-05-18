using Unity.Netcode;
using UnityEngine;

public class SmoothFollowCamera : NetworkBehaviour
{
    [SerializeField] Transform cameraTransform;
    [SerializeField] float positionSmoothTime = 0.1f;
    [SerializeField] float rotationSmoothTime = 0.1f;
    [SerializeField] Vector3 positionOffset;
    [SerializeField] Vector3 rotationOffset;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }
        // 自分のカメラ参照をセット
        cameraTransform = PlayerStatus.LocalInstance.virtual_camera.VirtualCameraGameObject.transform;
    }

    void LateUpdate()
    {
        if (!IsOwner || cameraTransform == null) return;

        // 位置と回転を両方渡す
        var camPos = cameraTransform.position;
        var camRot = cameraTransform.rotation;
        TargetCameraSetRequestServerRpc(camPos, camRot);
    }

    [ServerRpc]
    void TargetCameraSetRequestServerRpc(Vector3 camPos, Quaternion camRot, ServerRpcParams rpcParams = default)
    {
        // サーバー側では自分の cameraTransform を使わず、
        // 渡されたデータからオフセットをかけて計算する
        Vector3 targetPos = camPos + camRot * positionOffset;
        Quaternion targetRot = camRot * Quaternion.Euler(rotationOffset);
        TargetCameraMoveClientRpc(targetPos, targetRot);
    }

    [ClientRpc]
    void TargetCameraMoveClientRpc(Vector3 targetPos, Quaternion targetRot, ClientRpcParams rpcParams = default)
    {
        // 念のためオーナーだけ動かす
        if (!IsOwner) return;

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime / positionSmoothTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime / rotationSmoothTime);
    }
}
