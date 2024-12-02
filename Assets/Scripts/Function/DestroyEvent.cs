using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyEvent : MonoBehaviour
{
    public bool OnStart;
    public float OnStartDestroyTime;
    private void Start()
    {
        if (OnStart)
        {
            DODestroy(OnStartDestroyTime);
        }
    }
    //UnityEvent‚©‚çÀs‚µ‚½‚¢ˆ×‚É‚±‚ÌŠÖ”‚Í‘¶İ‚·‚éB
    public void DODestroy(float WaitTime)
    {
        Destroy(this.gameObject, WaitTime);
    }
}
