using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;
public class ItemBoxOpenCloseController : SerializedMonoBehaviour
{
    //アイテムが入っているボックス用の開け閉めController
    //引き出しや戸など
    public bool AnimationCompleted = true;
    public Transform OpenPos;
    public float duration = 0.3f;
    private Vector3 DefaultPosition;
    private Tween AnimationTween;
    private AudioSource item_box_source;
    public bool AlreadyOpened;
    public enum OpenCloseStatus
    {
        Close,
        Open
    }
    public OpenCloseStatus open_close_status;

    public AudioClip CloseCompletedClip;
    public AudioClip OpenClip;
    private ItemRandomCreateIndividual item_random_create_individual;

    // Start is called before the first frame update
    void Start()
    {
        DefaultPosition = this.transform.position;
        item_box_source = this.gameObject.GetComponent<AudioSource>();
        item_random_create_individual = this.gameObject.GetComponent<ItemRandomCreateIndividual>();
    }
    public void OpenCloseChangeEventByStatus()
    {
        //今あけてるか閉めてるかで開ける閉めるを実行する。
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
        item_random_create_individual.ItemRandomCreateFromIndividual(AlreadyOpened);

        AlreadyOpened = true;


        AnimationCompleted = false;

        if (AnimationTween != null)
            AnimationTween.Kill();
        AnimationTween = this.transform.DOMove(OpenPos.position, duration).SetLink(this.gameObject).SetEase(Ease.Linear).OnComplete(() => {
            AnimationCompleted = true;
        });
        item_box_source.PlayOneShot(OpenClip);
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
            item_box_source.PlayOneShot(CloseCompletedClip);
        });
        open_close_status = OpenCloseStatus.Close;
    }
}
