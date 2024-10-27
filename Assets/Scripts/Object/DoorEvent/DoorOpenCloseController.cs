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

    public AudioClip OpenClip;
    public AudioClip CloseClip;


    [ShowIf("open_type", OpenType.Rotate)]
    public AudioClip CloseCompletedClip;
    private Collider collider;
    private AudioSource audio_source;
    [Header("扉を開けたときにアイテムが出現するかどうか(台所の戸など)")]
    public bool ItemApperanceWhenOpended;
    private ItemRandomCreateIndividual item_random_create_individual;

    public bool AlreadyOpended;

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
        audio_source = this.gameObject.GetComponent<AudioSource>();
        collider = this.gameObject.GetComponent<Collider>();
        item_random_create_individual = this.gameObject.GetComponent<ItemRandomCreateIndividual>();
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
        if (ItemApperanceWhenOpended)
        {
            Debug.Log("Open---ItemApperanceWhenOpended");
            item_random_create_individual.ItemRandomCreateFromIndividual(AlreadyOpended);
        }
        AlreadyOpended = true;

        if (OpenClip != null && audio_source != null)
        {
            audio_source.PlayOneShot(OpenClip);
        }
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
        if(CloseClip != null && audio_source != null)
        {
            audio_source.PlayOneShot(CloseClip);
        }
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
                if(CloseCompletedClip != null && audio_source != null) {
                    audio_source.PlayOneShot(CloseCompletedClip);
                }
                the_door_controller.OpenedClosedCompletedAnimation = true;
                collider.isTrigger = false;
                CloseCompleted.Invoke();
            });
        }


    }
}
