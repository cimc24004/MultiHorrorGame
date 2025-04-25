using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using Sirenix.OdinInspector;

public class DoorOpenCloseController : SerializedMonoBehaviour
{
    private Vector3 DefaultLocalPosition;
    private Vector3 OpenLocalPosition;
    [Header("開けたときにトリガーを切り替えるかどうか")]
    public bool TriggerChangeModeOpenClose = true;
    [Header("開けるタイプを指定 SideOpen: ふすまなど Rotate: 普通のドアなど")]
    public OpenType open_type;

    [ShowIf("open_type", OpenType.SideOpen)]
    [Required] public Transform ToOpenTransform;
    [ShowIf("open_type", OpenType.Rotate)]
    [Header("どのくらい開けるか")]
    public Vector3 TargetRotateVector;
    private Vector3 DefaultRotateVector;

    [Required] public TheDoorController the_door_controller;
    public float Duration = 0.5f;

    public UnityEvent OpenCompleted;
    public UnityEvent CloseCompleted;

    public AudioClip OpenClip;
    public AudioClip CloseClip;

    [ShowIf("open_type", OpenType.Rotate)]
    public AudioClip CloseCompletedClip;
    private Collider collider;
    private AudioSource audio_source;
    [Header("扉を開けたときにアイテムが出現するかどうか(台所の戸など)")]
    public bool ItemAppearanceWhenOpened;
    private ItemRandomCreateIndividual item_random_create_individual;

    public bool AlreadyOpened;

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
        DefaultLocalPosition = this.transform.localPosition;
        DefaultRotateVector = this.transform.localEulerAngles;
        audio_source = this.gameObject.GetComponent<AudioSource>();
        collider = this.gameObject.GetComponent<Collider>();
        item_random_create_individual = this.gameObject.GetComponent<ItemRandomCreateIndividual>();

        if (open_type == OpenType.SideOpen)
        {
            // ローカル座標系での移動量を計算
            Vector3 localOffset = ToOpenTransform.localPosition - DefaultLocalPosition;

            // 指定した軸以外の成分をゼロにする
            switch (move_x_y_z)
            {
                case MoveXYZ.X:
                    localOffset.y = 0;
                    localOffset.z = 0;
                    break;
                case MoveXYZ.Y:
                    localOffset.x = 0;
                    localOffset.z = 0;
                    break;
                case MoveXYZ.Z:
                    localOffset.x = 0;
                    localOffset.y = 0;
                    break;
            }

            // 開いたときのローカル位置を計算
            OpenLocalPosition = DefaultLocalPosition + localOffset;
        }
    }

    [Button]
    public void Open()
    {
        if (ItemAppearanceWhenOpened)
        {
            Debug.Log("Open---ItemAppearanceWhenOpened");
            item_random_create_individual.ItemRandomCreateFromIndividual(AlreadyOpened);
        }
        AlreadyOpened = true;

        if (OpenClip != null && audio_source != null)
        {
            audio_source.PlayOneShot(OpenClip);
        }

        if (open_type == OpenType.SideOpen)
        {
            // ローカル座標系で移動
            this.transform.DOLocalMove(OpenLocalPosition, Duration)
                .SetEase(Ease.Linear)
                .SetLink(this.gameObject)
                .OnComplete(() =>
                {
                    the_door_controller.OpenedClosedCompletedAnimation = true;
                    OpenCompleted.Invoke();
                });
        }
        else if (open_type == OpenType.Rotate)
        {
            this.transform.DOLocalRotate(TargetRotateVector, Duration, RotateMode.Fast)
                .SetEase(Ease.Linear)
                .SetLink(this.gameObject)
                .OnUpdate(() =>
                {
                    collider.isTrigger = true;
                })
                .OnComplete(() =>
                {
                    the_door_controller.OpenedClosedCompletedAnimation = true;
                    if (TriggerChangeModeOpenClose)
                    {
                        collider.isTrigger = false;
                    }
                    OpenCompleted.Invoke();
                });
        }
    }

    [Button]
    public void Close()
    {
        Debug.Log("Close関数呼び出し");
        if (CloseClip != null && audio_source != null)
        {
            audio_source.PlayOneShot(CloseClip);
        }

        if (open_type == OpenType.SideOpen)
        {
            // ローカル座標系で移動
            this.transform.DOLocalMove(DefaultLocalPosition, Duration)
                .SetEase(Ease.Linear)
                .SetLink(this.gameObject)
                .OnComplete(() =>
                {
                    the_door_controller.OpenedClosedCompletedAnimation = true;
                    CloseCompleted.Invoke();
                });
        }
        else if (open_type == OpenType.Rotate)
        {
            this.transform.DOLocalRotate(DefaultRotateVector, Duration, RotateMode.Fast)
                .SetEase(Ease.Linear)
                .SetLink(this.gameObject)
                .OnUpdate(() =>
                {
                    if (TriggerChangeModeOpenClose)
                    {
                        collider.isTrigger = true;
                    }
                })
                .OnComplete(() =>
                {
                    if (CloseCompletedClip != null && audio_source != null)
                    {
                        audio_source.PlayOneShot(CloseCompletedClip);
                    }
                    the_door_controller.OpenedClosedCompletedAnimation = true;
                    collider.isTrigger = false;
                    CloseCompleted.Invoke();
                });
        }
    }
}
