using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeChaseToPlayerHead : MonoBehaviour
{
    [SerializeField]
    private Vector3 AddAngles;
    private PlayerStatus player_status;
    private GameObject PlayerCameraRoot;

    private Vector3 SubAngle;
    // Start is called before the first frame update
    public Transform cameraRoot;  // プレイヤーのカメラ位置を指す
    public float rotateSpeed = 5f; // 好みでスムーズ回転させたい場合
    private void Start()
    {
        player_status = PlayerStatus.instance;
        PlayerCameraRoot = player_status.CameraRoot.gameObject;
    }
    void LateUpdate()
    {
        Vector3 dir = (PlayerCameraRoot.transform.position - transform.position).normalized;
        // 基本の「dir を正面に向ける」回転
        Quaternion baseRot = Quaternion.LookRotation(dir);

        // さらに X軸に 90度回転を加える
        Quaternion offsetRot = baseRot * Quaternion.Euler(AddAngles);

        // スナップではなくスムーズに回転させたい場合は Slerp などを使う
        transform.rotation = Quaternion.Slerp(transform.rotation, offsetRot, rotateSpeed * Time.deltaTime);
    }

}
