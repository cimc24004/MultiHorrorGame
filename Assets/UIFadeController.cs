using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class UIFadeController : MonoBehaviour
{
    private float EndFadeOutTimeNow;
    private float EndFadeOutTime;
    private bool EndFadeOut;
    private bool StartFadeIn;
    [SerializeField]
    private CanvasGroup canvas_group;
    private Tween FadeOutTween;
    private Tween FadeInTween;
    // Start is called before the first frame update
    void Start()
    {
        if(canvas_group == null)
        {
            canvas_group = this.gameObject.GetComponent<CanvasGroup>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (EndFadeOut)
        {
            EndFadeOutTimeNow += Time.deltaTime;
            if(EndFadeOutTime < EndFadeOutTimeNow)
            {
                FadeOutTween = canvas_group.DOFade(0, 0.6f).SetEase(Ease.Linear).SetLink(this.gameObject);
                EndFadeOutTimeNow = 0;
                EndFadeOut = false;
            }
        }

        if (StartFadeIn)
        {
            FadeInTween = canvas_group.DOFade(1, 0.6f).SetEase(Ease.Linear).SetLink(this.gameObject).OnComplete(() => {
                StartFadeIn = false;
                AutoFadeOut(3);
            });

        }
    }
    public void AutoFadeOut(float _EndFadeOutTime)
    {
        EndFadeOutTime = _EndFadeOutTime;
        EndFadeOut = true;
    }

    public void AutoFadeIn()
    {
        if(FadeOutTween != null)
        {
            FadeOutTween.Kill();
        }
        StartFadeIn = true;
    }
}
