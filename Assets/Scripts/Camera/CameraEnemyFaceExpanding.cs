using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Sirenix.OdinInspector;
using Unity.Netcode;
public class CameraEnemyFaceExpanding : NetworkBehaviour
{
    //敵に見つかって触れた時に敵の顔がドアップになる。 恐怖演出
    [Required]public GameObject FaceExpandingCameraObj;
    private Animator anim;
    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        anim = this.gameObject.GetComponent<Animator>();
    }

    [Button]
    public void FaceExpandingActive(GameObject Player)
    {
        //カメラをオンにする。 Priorityを決めているのでメインカメラよりこのカメラが必ず描画される。
        FaceExpandingCameraObj.SetActive(true);
        anim.SetBool("KillPlayer", true);
    }
    [Button]
    public void FaceExpandingUnActive()
    {
        //カメラオフにする。
        FaceExpandingCameraObj.SetActive(false);
        anim.SetBool("KillPlayer", false);
    }
}
