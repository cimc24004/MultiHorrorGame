using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using Sirenix.OdinInspector;

public class DoorOpenCloseController : SerializedMonoBehaviour
{
    private Vector3 DefaultPosition;
    public Transform ToOpenPos;
    public TheDoorController the_door_controller;
    public float duration = 0.3f;

    public UnityEvent OpenCompleted;
    public UnityEvent CloseCompleted;
    private void Start()
    {
        DefaultPosition = this.transform.position;
    }
    [Button]
    public void Open()
    {

        this.transform.DOMove(ToOpenPos.transform.position, duration).SetEase(Ease.Linear).SetLink(this.gameObject).OnComplete(() =>
        {
            the_door_controller.OpenedClosedCompletedAnimation = true;
            OpenCompleted.Invoke();
        });
        //this.transform.position = ToOpenPos.position;
        //the_door_controller.OpenedClosedCompletedAnimation = true;
    }
    [Button]
    public void Close()
    {
        this.transform.DOMove(DefaultPosition, duration).SetEase(Ease.Linear).SetLink(this.gameObject).OnComplete(() => {
            the_door_controller.OpenedClosedCompletedAnimation = true;
            CloseCompleted.Invoke();
        });
    }
}
