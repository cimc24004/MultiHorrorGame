using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
public class CrossHairStatus : SerializedMonoBehaviour
{
    //クロスヘアのステータス

    public enum EventColHitedStatus{
        None,
        Item,
        HideObj,
        OpenObj
    }

    //デフォルトのクロスヘアアイコンオブジェクト
    public Image DefaultCrossHairIcon;
    //チェックのクロスヘアアイコンオブジェクト
    public Image CheckCrossHairIcon;

    //CrossHairEventの　ドアやアイテムなどにレイキャストが当たっているのかどうか
    public bool RaycastHitedExistsByCrossHairEvent;
    public string HitedTag;
    public GameObject HitedObj;
    private PlayerStatus player_status;
    public EventColHitedStatus event_col_hited_status;
    // Start is called before the first frame update
    void Start()
    {
        player_status = PlayerStatus.instance;
    }

    // Update is called once per frame
    void Update()
    {
        //プレイヤーが隠れていたら、またはドアやアイテムなどにレイキャストが当たっているならば チェック画像を表示。
        if (RaycastHitedExistsByCrossHairEvent || player_status.HidingNow)
        {
            SpriteChange(_CheckMode: true);
        }
        else
        {
            SpriteChange(_CheckMode: false);
        }
    }


    public void SpriteChange( bool _CheckMode)
    {
        //チェックモードによって表示する画像を変更する。
        if(_CheckMode == true)
        {

            DefaultCrossHairIcon.gameObject.SetActive(false);
            CheckCrossHairIcon.gameObject.SetActive(true);
        }
        else
        {

            DefaultCrossHairIcon.gameObject.SetActive(true);
            CheckCrossHairIcon.gameObject.SetActive(false);

        }
    }
}
