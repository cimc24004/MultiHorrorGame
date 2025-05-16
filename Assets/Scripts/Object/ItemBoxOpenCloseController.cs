// ItemBoxOpenCloseController.cs
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using DG.Tweening;

public class ItemBoxOpenCloseController : MonoBehaviour
{
    [Header("開けたときに呼び出す UnityEvent")]
    [SerializeField] private UnityEvent OpenAddEvent;

    [Header("開閉アニメーション用フィールド")]
    public bool AnimationCompleted { get; private set; } = true;
    public bool AlreadyOpened { get; private set; }
    [SerializeField] private Transform OpenPos;
    [SerializeField] private float duration = 0.3f;

    private Vector3 DefaultPosition;
    private Tween AnimationTween;
    private AudioSource item_box_source;
    [SerializeField] private AudioClip OpenClip;
    [SerializeField] private AudioClip CloseCompletedClip;

    private ItemRandomCreateIndividual item_random_create_individual;
    public DrawerManager drawerManager;

    [Header("この引き出しは drawers リストの何番目か")]
    [SerializeField] private int drawerIndex;

    private void Awake()
    {
        // 元コードの OnNetworkSpawn 相当
        DefaultPosition = transform.position;
        item_box_source = GetComponent<AudioSource>();
        item_random_create_individual = GetComponent<ItemRandomCreateIndividual>();

        drawerManager = GetComponentInParent<DrawerManager>();
        if (drawerManager == null)
            Debug.LogError($"[{name}] DrawerManager が見つかりません");
    }

    // ボタン押下やプレイヤー接触などから呼ぶ
    [Button] // Odin のインスペクタテスト用
    public void RequestToggle()
    {
        if (!AnimationCompleted) return;
        // 親の ServerRpc を呼び出して状態変更をリクエスト
        drawerManager.RequestToggleBoxServerRpc(drawerIndex);
    }

    // DrawerManager から ClientRpc 経由で呼ばれる「開く」ローカル処理
    public void LocalOpen()
    {
        item_random_create_individual.ItemRandomCreateFromIndividual(AlreadyOpened);
        AlreadyOpened = true;
        AnimationCompleted = false;
        OpenAddEvent.Invoke();

        if (AnimationTween != null)
            AnimationTween.Kill();

        AnimationTween = transform
            .DOMove(OpenPos.position, duration)
            .SetLink(gameObject)
            .SetEase(Ease.Linear)
            .OnComplete(() => AnimationCompleted = true);

        item_box_source.PlayOneShot(OpenClip);
    }

    // DrawerManager から ClientRpc 経由で呼ばれる「閉じる」ローカル処理
    public void LocalClose()
    {
        AnimationCompleted = false;

        if (AnimationTween != null)
            AnimationTween.Kill();

        AnimationTween = transform
            .DOMove(DefaultPosition, duration)
            .SetLink(gameObject)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                AnimationCompleted = true;
                item_box_source.PlayOneShot(CloseCompletedClip);
            });
    }
}
