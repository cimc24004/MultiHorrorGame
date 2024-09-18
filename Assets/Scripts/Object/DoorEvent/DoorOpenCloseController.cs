using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using Sirenix.OdinInspector;

public class DoorOpenCloseController : SerializedMonoBehaviour
{

    private Vector3 DefaultPosition;
    [Header("開けたときにトリガーを切り替えるかどうか")]
    public bool TriggerChangeModeOpenClose = true;
    [Header("開けるタイプを指定 SideOpen: ふすまなど Rotate: 普通のドアなど")]
    public OpenType open_type;

    [ShowIf("open_type", OpenType.SideOpen)]
    [Required]public Transform ToOpenTransform;
    [ShowIf("open_type", OpenType.Rotate)]
    [Header("どのくらい開けるか")]
    public Vector3 TargetRotateVector;
    public Vector3 DefaultRotateVector;
    private Vector3 ToOpenVector3;

    [Required] public TheDoorController the_door_controller;
    public float duration = 0.3f;

    public UnityEvent OpenCompleted;
    public UnityEvent CloseCompleted;
    private Collider collider;
    public enum MoveXYZ
    {
        X,
        Y,
        Z
    }
    public enum OpenType
    {
        SideOpen,
        Rotate
    }
    [ShowIf("open_type", OpenType.SideOpen)]
    [Header("何の軸を移動するか")]
    public MoveXYZ move_x_y_z;

    private void Start()
    {
        DefaultPosition = this.transform.position;
        DefaultRotateVector = this.transform.localEulerAngles;
        collider = this.gameObject.GetComponent<Collider>();
        if (OpenType.SideOpen == open_type)
        {
            if (move_x_y_z == MoveXYZ.X)
            {
                ToOpenVector3 = new Vector3(ToOpenTransform.transform.position.x, DefaultPosition.y, DefaultPosition.z);

            }
            else if (move_x_y_z == MoveXYZ.Y)
            {
                ToOpenVector3 = new Vector3(DefaultPosition.x, ToOpenTransform.transform.position.y, DefaultPosition.z);

            }
            else if (move_x_y_z == MoveXYZ.Z)
            {
                ToOpenVector3 = new Vector3(DefaultPosition.x, DefaultPosition.y, ToOpenTransform.transform.position.z);

            }
        }

    }

    [Button]
    public void Open()
    {
        if(open_type == OpenType.SideOpen)
        {
            this.transform.DOMove(ToOpenVector3, duration).SetEase(Ease.Linear).SetLink(this.gameObject).OnComplete(() =>
            {
                the_door_controller.OpenedClosedCompletedAnimation = true;
                OpenCompleted.Invoke();
            });
        }else if(open_type == OpenType.Rotate)
        {
            this.transform.DOLocalRotate(TargetRotateVector, duration, RotateMode.Fast).SetEase(Ease.Linear).SetLink(this.gameObject).OnUpdate(() => {
                collider.isTrigger = true;
            }).OnComplete(() => {
                the_door_controller.OpenedClosedCompletedAnimation = true;
                if (TriggerChangeModeOpenClose)
                {
                    collider.isTrigger = false;
                }
                OpenCompleted.Invoke();
            });

        }

        //this.transform.position = ToOpenPos.position;
        //the_door_controller.OpenedClosedCompletedAnimation = true;
    }
    [Button]
    public void Close()
    {
        Debug.Log("Close関数呼び出し");
        if (open_type == OpenType.SideOpen)
        {
            this.transform.DOMove(DefaultPosition, duration).SetEase(Ease.Linear).SetLink(this.gameObject).OnComplete(() => {
                the_door_controller.OpenedClosedCompletedAnimation = true;
                CloseCompleted.Invoke();
            });
        }
        else if (open_type == OpenType.Rotate)
        {
            this.transform.DOLocalRotate(DefaultRotateVector, duration, RotateMode.Fast).SetEase(Ease.Linear).SetLink(this.gameObject).OnUpdate(() => {
                if (TriggerChangeModeOpenClose)
                {
                    collider.isTrigger = true;
                }
            }).OnComplete(() => {
                the_door_controller.OpenedClosedCompletedAnimation = true;
                collider.isTrigger = false;
                CloseCompleted.Invoke();
            });
        }


    }
}
