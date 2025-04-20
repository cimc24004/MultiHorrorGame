using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothFollowCamera : MonoBehaviour
{
    public Transform cameraTransform;
    public float positionSmoothTime = 0.1f;  // 小さめの値にすると「ピタッ」と追従
    public float rotationSmoothTime = 0.1f;
    public Vector3 positionOffset;
    public Vector3 rotationOffset;
    void Start()
    {
        cameraTransform = Camera.main.transform;
    }
    void LateUpdate()
    {
        // ターゲットとなる位置と回転
        Vector3 targetPos = cameraTransform.position + cameraTransform.TransformDirection(positionOffset);
        Quaternion targetRot = cameraTransform.rotation * Quaternion.Euler(rotationOffset);

        // 位置をスムーズに追従
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime / positionSmoothTime);
        
        // 回転をスムーズに追従
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime / rotationSmoothTime);
    }
}
