using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Sirenix.OdinInspector;
public class NoticedUIController : SerializedMonoBehaviour
{

    public TextMeshProUGUI m_notice_text_ui;
    public CanvasGroup NoticeUIs;

    private Tween notice_uis_tween;
    private float notice_time_now;
    private float notice_time;
    private bool notice_now;


    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void Update()
    {
        if (notice_now)
        {
            notice_time_now += Time.deltaTime;
            if (notice_time_now > notice_time)
            {
                NoticeReset();
                notice_time_now = 0;
            }
        }

    }
    [Button]
    public void Notice(string _sentence,float _display_time = 1)
    {
        notice_uis_tween.Kill();
        NoticeUIs.alpha = 1;
        notice_time_now = 0;
        notice_now = true;
        m_notice_text_ui.text = _sentence;
        notice_time = _display_time;

    }
    public void NoticeReset()
    {
        notice_uis_tween = NoticeUIs.DOFade(0, 0.1f).SetEase(Ease.Linear).SetLink(this.gameObject).OnComplete(() => {
            m_notice_text_ui.text = "";
        });
        notice_now = false;
    }
}
