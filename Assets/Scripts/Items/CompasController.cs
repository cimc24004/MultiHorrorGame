using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompasController : MonoBehaviour
{
    [Header("コンパスの磁針を指定")]
    [SerializeField] private GameObject CompasBlue;
    [SerializeField] private GameObject CompasRed;
    [SerializeField] private GameObject CompasGreen;

    [Header("コンパスが向くターゲットの場所を指定")]
    [SerializeField] private Transform BlueTargetDirectionTransform;
    [SerializeField] private Transform RedTargetDirectionTransform;
    [SerializeField] private Transform GreenTargetDirectionTransform;

    private Vector3 BlueDirection;
    private Vector3 GreenDirection;
    private Vector3 RedDirection;

    private PlayerStatus player_status;
    private GameObject Player;
    private GameObject MainCamera;

    void Start()
    {
        player_status = PlayerStatus.instance;
        Player = player_status.gameObject;
        MainCamera = Camera.main.gameObject;
    }

    void Update()
    {
        // ワールド空間での (ターゲット -> カメラ) 方向ベクトル
        BlueDirection  = MainCamera.transform.position - BlueTargetDirectionTransform.position;
        RedDirection   = MainCamera.transform.position - RedTargetDirectionTransform.position;
        GreenDirection = MainCamera.transform.position - GreenTargetDirectionTransform.position;

        // それぞれの磁針オブジェクトをY軸回転だけで向かせる
        RotateCompassY(CompasBlue,  BlueDirection);
        RotateCompassY(CompasRed,   RedDirection);
        RotateCompassY(CompasGreen, GreenDirection);
    }

    /// <summary>
    /// 親の回転に追従しつつ、コンパスの針(子オブジェクト)を
    /// 「ローカル空間で Y軸のみ回転」してターゲット方向を向かせる
    /// </summary>
    private void RotateCompassY(GameObject compassObj, Vector3 worldDirection)
    {
        if (compassObj == null) return;

        var compassTransform = compassObj.transform;
        // 親がいないならローカル回転の意味が無いのでスキップ
        if (compassTransform.parent == null) return;

        // 1) 親のローカル空間での方向に変換
        Vector3 localDir = compassTransform.parent.InverseTransformDirection(worldDirection);

        // 2) 上下(Y)成分を0にし、水平方向だけにする
        localDir.y = 0f;
        // 小さすぎる場合は回転不要
        if (localDir.sqrMagnitude < 1e-6f)
        {
            return;
        }

        // 3) localDir の向きに相当する「Y 軸回転角度 (度数)」を算出
        //    Z軸を「前」とみなすなら (x, z) の順で Atan2
        float angleY = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;

        // 4) 現在のローカル角度を取得し、Y 軸だけ書き換える
        Vector3 currentEuler = compassTransform.localEulerAngles;
        currentEuler.y = angleY;

        // 5) ローカル角度を適用（瞬時に向けたいならこれでOK）
        compassTransform.localEulerAngles = currentEuler;

        // --- もしスムーズに回転させたいなら、Slerp や DoTween を使って補間する ---
        // 例:
        // compassTransform.localRotation =
        //     Quaternion.Slerp(
        //         compassTransform.localRotation,
        //         Quaternion.Euler(currentEuler),
        //         Time.deltaTime * 5f // 回転スピードは適宜調整
        //     );
    }
}
