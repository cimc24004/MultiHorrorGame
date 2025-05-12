using System.Collections;
using UnityEngine;
using DG.Tweening;
using Unity.Netcode;

public class EyeChaseToPlayerHead : MonoBehaviour
{
    [SerializeField]
    private Vector3 AddAngles;

    public float rotateSpeed = 5f; // 好みでスムーズ回転

    // ローカルプレイヤーの CameraRoot をキャッシュ
    private Transform cameraRoot;

    private IEnumerator Start()
    {
        // PlayerStatus.LocalInstance がセットされるまで待機
        yield return new WaitUntil(() => PlayerStatus.LocalInstance != null);

        // キャッシュしなおし
        var status = PlayerStatus.LocalInstance;
        cameraRoot = status.CameraRoot.transform;
    }

    private void LateUpdate()
    {
        if (cameraRoot == null) return;

        // 元のまま「視線方向ベクトル」→「LookRotation」→「オフセット回転」→「Slerp」
        Vector3 dir       = (cameraRoot.position - transform.position).normalized;
        Quaternion baseRot   = Quaternion.LookRotation(dir);
        Quaternion offsetRot = baseRot * Quaternion.Euler(AddAngles);
        transform.rotation   = Quaternion.Slerp(
            transform.rotation,
            offsetRot,
            rotateSpeed * Time.deltaTime
        );
    }
}
