using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Sirenix.OdinInspector;
public class CameraEnemyFaceExpanding : SerializedMonoBehaviour
{
    //敵に見つかって触れた時に敵の顔がドアップになる。 恐怖演出
    public GameObject FaceExpandingCameraObj;
    private Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        anim = this.gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [Button]
    public void FaceExpandingActive(GameObject Player)
    {
        //カメラをオンにする。 Priorityを決めているのでメインカメラよりこのカメラが必ず描画される。
        FaceExpandingCameraObj.SetActive(true);
        anim.SetBool("KubiSinu", true);
    }
    [Button]
    public void FaceExpandingUnActive()
    {
        FaceExpandingCameraObj.SetActive(false);
        anim.SetBool("KubiSinu", false);
    }
}
