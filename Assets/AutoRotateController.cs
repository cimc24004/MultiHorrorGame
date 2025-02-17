using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class AutoRotateController : MonoBehaviour
{
    [SerializeField] private float RotateSpeed;
    [SerializeField] private Vector3 ToRotate;
    void Start()
    {
        Rotate();
    }

    void Rotate()
    {
        transform.DORotate(ToRotate, RotateSpeed, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .OnComplete(Rotate).SetLink(this.gameObject); // ‰ñ“]‚ªI‚í‚Á‚½‚ç‚à‚¤ˆê“xŒÄ‚Ño‚·
    }

}
