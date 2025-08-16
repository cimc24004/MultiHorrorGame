using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;
using Unity.Netcode;
using UnityEngine;

public class PlayerCameraController : NetworkBehaviour
{
    // カメラの回転に沿って、プレイヤーの向きを変更するスクリプト
    public GameObject DirectionCamera;
    private bool initPlayer;
    private const float YawEpsilonDeg = 0.25f; // これ未満の変化は無視（微振動防止）

    public override void OnNetworkSpawn()
    {
        // オーナーでカメラ未設定なら念のため拾っておく
        if (IsOwner && DirectionCamera == null)
        {
            var ps = GetComponent<PlayerStatus>();
            DirectionCamera = ps != null ? ps.virtual_camera.VirtualCameraGameObject : null;
        }
    }

    void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InitOnPlayerSpawn;
    }

    void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitOnPlayerSpawn;
    }

    private void InitOnPlayerSpawn(PlayerStatus ps)
    {
        if (IsOwner)
        {
            DirectionCamera = ps.virtual_camera.VirtualCameraGameObject;
        }
        initPlayer = true;
    }

    // Cinemachine適用後に回したいのでLateUpdate推奨
    void LateUpdate()
    {
        if (!IsOwner || !initPlayer || DirectionCamera == null) return;

        float yaw = DirectionCamera.transform.eulerAngles.y;

        // 小さい角度差なら更新しない（ネット負荷/ジッター軽減）
        float diff = Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, yaw));
        if (diff < YawEpsilonDeg) return;

        // スナップでOK（スムースにしたければSlerpへ）
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        // スムースにしたい場合はこっち:
        // var target = Quaternion.Euler(0f, yaw, 0f);
        // transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * 15f);
    }
}
