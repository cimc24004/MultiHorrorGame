using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SmoothFollowCamera : NetworkBehaviour
{
    public Transform cameraTransform;
    public float positionSmoothTime = 0.1f;  // 小さめの値にすると「ピタッ」と追従
    public float rotationSmoothTime = 0.1f;
    public Vector3 positionOffset;
    public Vector3 rotationOffset;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            cameraTransform = Camera.main.transform;
        }
        
    }
    void LateUpdate()
    {
        if (IsOwner)
        {
            Debug.Log("IsOwner---LateUpdate");
            // ターゲットとなる位置と回転
            TargetCameraSetRequestServerRpc();
        }   



    }
    [ServerRpc]
    private void TargetCameraSetRequestServerRpc()
    {
        Vector3 targetPos = cameraTransform.position + cameraTransform.TransformDirection(positionOffset);
        Quaternion targetRot = cameraTransform.rotation * Quaternion.Euler(rotationOffset);
        TargetCameraMoveClientRpc(targetPos, targetRot);
    }
    [ClientRpc]
    private void TargetCameraMoveClientRpc(Vector3 _targetPos, Quaternion _targetRot)
    {
        // 位置をスムーズに追従
        transform.position = Vector3.Lerp(transform.position, _targetPos, Time.deltaTime / positionSmoothTime);

        // 回転をスムーズに追従
        transform.rotation = Quaternion.Slerp(transform.rotation, _targetRot, Time.deltaTime / rotationSmoothTime);
    }
}
