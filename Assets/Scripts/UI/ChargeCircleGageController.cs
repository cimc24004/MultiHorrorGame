using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class ChargeCircleGageController : MonoBehaviour
{
    private PlayerStatus player_status;
    private GameObject GageCanvas;
    private NoticedUIController noticed_ui_controller;
    private CrossHairStatus cross_hair_status;
    [SerializeField]private bool UIShow;
    [Header("チャージ中かどうか")]
    public bool ChargeMode;
    [Header("最大値を指定")]
    public float MaxChargeValue;
    [Header("チャージの今の値")]
    public float ChargeNowValue;

    public UnityEvent2 CompletedEvent;
    [Header("Raycastが当たるタグの名前")]
    public string WillHitTagName;
    public GameObject WillHitObject;

    [Header("Raycastが当たってるときのメッセージ")]
    public string NoticeMessage;

    [Header("チャージ一回しかできないかどうか")]
    public bool ChargeOnlyOnce;
    [SerializeField]private bool ChargeOnlyOnceNow;

    private bool once;
    private void Start()
    {
        player_status = PlayerStatus.instance;
        cross_hair_status = player_status.cross_hair_event.gameObject.GetComponent<CrossHairStatus>();
        GageCanvas = player_status.GageCanvas;
        noticed_ui_controller = player_status.noticed_ui_controller;
        CompletedEvent.AddListener(() => { 
            
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (!once)
        {
            GageCanvas.SetActive(false);
            once = true;
        }
        //一度だけチャージできる設定ならChargeOnlyOnceNowをtrueにしてこれがtrueならreturn
        if (ChargeOnlyOnceNow) return;

        if (ChargeMode)
        {
            Debug.Log("ChargeModeTrue");
            ChargeNowValue += Time.deltaTime;
            Debug.Log("ChargeNowValue: " + ChargeNowValue);
            player_status.GageImage.fillAmount = ChargeNowValue / MaxChargeValue;
            if(ChargeNowValue >= MaxChargeValue)
            {
                Debug.Log("COmpleted!!!!!!!!Charge");
                ChargeCompleted();
            }
            ChargeMode = false;

        }
        if (UIShow)
        {
            noticed_ui_controller.Notice(NoticeMessage);
            GageCanvas.SetActive(true);
            once = false;
            UIShow = false;
        }
    }
    public void UIShowTrigger()
    {
        UIShow = true;
    }
    public void GageCharge()
    {
        if (ChargeOnlyOnceNow) return;
        ChargeMode = true;
    }
    public void GageCircleGageControllerHide()
    {
        GageCanvas.SetActive(false);
        this.enabled = false;

    }
    private void ChargeCompleted()
    {

        CompletedEvent.Invoke();
        player_status.GageImage.fillAmount = 0;
        ChargeNowValue = 0;
        if (ChargeOnlyOnce)
        {
            ChargeOnlyOnceNow = true;
        }
    }
}
