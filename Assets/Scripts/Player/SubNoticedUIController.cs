using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
public class SubNoticedUIController : SerializedMonoBehaviour
{
    [Header("通知を表示する親オブジェクト")]
    [SerializeField] private RectTransform noticeRoot;

    [Header("NoticeとNoticeの間隔")]
    [SerializeField] private float spacing = 10f;

    [Header("削除時に上へスライドする時間(DOTween)")]
    [SerializeField] private float slideDuration = 0.5f;

    [Header("表示用プレハブ(テキストや背景Imageなど)")]
    [SerializeField] private GameObject noticePrefab;

    // 現在アクティブなNoticeのリスト
    [SerializeField]private List<RectTransform> noticeList = new List<RectTransform>();

    private AudioSource notie_audio_source;
    void Start()
    {
        notie_audio_source = this.gameObject.GetComponent<AudioSource>();
    }
    /// <summary>
    /// Noticeを追加表示するメソッド。
    /// 名前を変えず "Notice()" のまま利用できるように実装。
    /// </summary>
    /// <param name="message">表示したい文字列</param>
    public void Notice(string message, float WaitStart = 0)
    {
        DOVirtual.DelayedCall(WaitStart, () => {
            // プレハブを生成して、親をnoticeRootに設定
            GameObject noticeObj = Instantiate(noticePrefab, noticeRoot);
            NoticeUIData notice_ui_data = noticeObj.GetComponent<NoticeUIData>();
            // テキストをセット（プレハブ内にTextコンポーネントがある想定）
            TextMeshProUGUI textComp = notice_ui_data.NoticeTextUI;
            textComp.text = message;

            // RectTransformを取得してリストに加える
            RectTransform rect = noticeObj.GetComponent<RectTransform>();
            noticeList.Add(rect);
            notie_audio_source.Play();
            // 追加時はアニメーションしないで即座に並べる
            LayoutNoticesImmediate();

            // もし6つ以上になったら古いNoticeを自動的に削除する
            // if (noticeList.Count >= 6)
            // {
            //     RemoveOldestNotice();
            // }
            // テスト用: 3秒後に自動的に古いNoticeを削除するサンプル
            DOVirtual.DelayedCall(5, () =>
            {
                RemoveOldestNotice();
            });

        });


    }

    /// <summary>
    /// 特定のNoticeを削除し、ほかの要素を上へアニメーションさせる
    /// </summary>
    public void RemoveNotice(RectTransform target)
    {
        if (!noticeList.Contains(target)) return;
        
        // リストから外す
        noticeList.Remove(target);

        // 削除（フェードアウトなどを挟むならここでCanvasGroupをTweenする）
        Destroy(target.gameObject);
        target = null;
        // 残った要素をアニメーションで上へスライドさせる
        SlideUpNotices();
    }

    /// <summary>
    /// もっとも古いNoticeを削除する例
    /// </summary>
    public void RemoveOldestNotice()
    {
        if (noticeList.Count == 0) return;
        CanvasGroup notice_canvas_group = noticeList[0].GetComponent<CanvasGroup>();
        // notice_canvas_group.DOFade(0, 0.3f).SetLink(this.gameObject).OnComplete(() => {
        RemoveNotice(noticeList[0]);
        // });
    }

    /// <summary>
    /// 現在のnoticeListを隙間なく瞬時に並べる
    /// </summary>
    private void LayoutNoticesImmediate()
    {
        float currentY = 0f;
        for (int i = 0; i < noticeList.Count; i++)
        {
            RectTransform rt = noticeList[i];
            rt.anchoredPosition = new Vector2(0f, -currentY);

            // 要素の高さ + スペーシング分だけ下げる
            currentY += rt.sizeDelta.y + spacing;
        }
    }

    /// <summary>
    /// DOTweenを使って、削除後の要素を上にスライド
    /// </summary>
    private void SlideUpNotices()
    {
        // まず「最終的にどこに並べたいか」計算し、targetPositionsに保存
        float currentY = 0f;
        Vector2[] targetPositions = new Vector2[noticeList.Count];
        for (int i = 0; i < noticeList.Count; i++)
        {
            float h = noticeList[i].sizeDelta.y;
            targetPositions[i] = new Vector2(0f, -currentY);
            currentY += (h + spacing);
        }

        // DOTweenで並行アニメーションさせるためにSequenceを作る
        Sequence seq = DOTween.Sequence();

        for (int i = 0; i < noticeList.Count; i++)
        {
            RectTransform rt = noticeList[i];

            // それぞれの要素をtargetPositions[i]までslideDuration秒かけて移動
            // Easeは好みで設定
            Tween t = rt.DOAnchorPos(targetPositions[i], slideDuration)
                .SetEase(Ease.OutCubic);

            // SequenceにJoinすることで全要素を同時に動かす
            seq.Join(t);
        }

        // 全部の移動が終わったあとに微調整
        seq.OnComplete(() =>
        {
            // 最終位置を再度確定（ズレがある場合用）
            LayoutNoticesImmediate();
        });
    }
}
