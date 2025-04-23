using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    //カメラの回転に沿って、プレイヤーの向きを変更するスクリプト
    public GameObject DirectionCamera;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
                // Y軸のみ、カメラの向きに即時同期
        float camYaw = DirectionCamera.transform.eulerAngles.y;
        transform.rotation = Quaternion.Euler(0f, camYaw, 0f);
    }
}
