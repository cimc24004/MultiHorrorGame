using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetHideObject : MonoBehaviour
{
    //どこに隠すか場所を指定！
    public Transform HideBeyondPos;
    //隠れた後その場所から出るときにどこに出るか。
    public Transform UnHideBeyondPos;

    //誰かが隠れているかどうか。
    public bool SomeoneHided;

    //隠れるタイプは何なのか。
    public enum HideType{
        Stand,
        Crouch,
        FaceDown
    };
    [Header("隠れるタイプ")]
    [Header("Stand: 立った状態 \n Crouch: 座った状態 \n FaceDown: うつ伏せ状態")]
    public HideType hide_type;

    public void HideTarget(GameObject Target)
    {
        Target.transform.position = HideBeyondPos.transform.position;

        SomeoneHided = true;
    }
    public void UnHideTarget(GameObject Target)
    {
        Target.transform.position = UnHideBeyondPos.transform.position;
        SomeoneHided = false;
    }
}
