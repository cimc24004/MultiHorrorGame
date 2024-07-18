using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyEvent : MonoBehaviour
{
    //UnityEvent‚©‚çÀs‚µ‚½‚¢ˆ×‚É‚±‚ÌŠÖ”‚Í‘¶İ‚·‚éB
    public void DODestroy(float WaitTime)
    {
        Destroy(this.gameObject, WaitTime);
    }
}
