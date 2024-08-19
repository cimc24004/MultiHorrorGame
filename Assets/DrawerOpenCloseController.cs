using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;
public class DrawerOpenCloseController : MonoBehaviour
{
    //ˆø‚«o‚µ‚ÌŠJ‚¯•Â‚ßController
    public bool AnimationCompleted = true;
    public Transform OpenPos;
    public float duration = 0.3f;
    private Vector3 DefaultPosition;
    private Tween AnimationTween;
    public enum OpenCloseStatus
    {
        Close,
        Open
    }
    public OpenCloseStatus open_close_status;
    // Start is called before the first frame update
    void Start()
    {
        DefaultPosition = this.transform.position;
    }
    public void OpenCloseChangeEventByStatus()
    {
        if (!AnimationCompleted) return;
        if(open_close_status == OpenCloseStatus.Open)
        {
            Close();
        }else if(open_close_status == OpenCloseStatus.Close)
        {
            Open();
        }
    }
    [Button]
    public void Open()
    {
        AnimationCompleted = false;
        if (AnimationTween != null)
            AnimationTween.Kill();

        AnimationTween = this.transform.DOMove(OpenPos.position, duration).SetLink(this.gameObject).SetEase(Ease.Linear).OnComplete(() => {
            AnimationCompleted = true;
        });
        open_close_status = OpenCloseStatus.Open;
    }
    [Button]
    public void Close()
    {
        AnimationCompleted = false;
        if(AnimationTween != null)
        AnimationTween.Kill();
        AnimationTween = this.transform.DOMove(DefaultPosition, duration).SetLink(this.gameObject).SetEase(Ease.Linear).OnComplete(() => {
            AnimationCompleted = true;
        });
        open_close_status = OpenCloseStatus.Close;
    }
}
