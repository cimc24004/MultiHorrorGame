using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
public class CrossHairStatus : SerializedMonoBehaviour
{
    // クロスヘアの状態

    public enum EventColHitedStatus{
        None,
        Item,
        HideObj,
        OpenObj
    }

    // 通常時のクロスヘアアイコン
    public Image DefaultCrossHairIcon;
    // 何かにヒットしている時のアイコン
    public Image CheckCrossHairIcon;

    // CrossHairEventのレイキャストが何かに当たっているか
    public bool RaycastHitedExistsByCrossHairEvent;
    public string HitedTag;
    public GameObject HitedObj;
    private PlayerStatus player_status;
    public EventColHitedStatus event_col_hited_status;
    private bool InitPlayer;
    // OnEnable は最初のフレーム前に呼ばれる
    private void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InitWhenPlayerSpawned;
    }
    private void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitWhenPlayerSpawned;
    }
    private void InitWhenPlayerSpawned(PlayerStatus _playerStatus){
        player_status = _playerStatus;
        InitPlayer = true;
    }
    // 毎フレーム呼び出される
    void Update()
    {
        if(!InitPlayer) return;
        // 何かに当たっている、または隠れている間はチェックアイコンを表示
        if (RaycastHitedExistsByCrossHairEvent || player_status.HidingNow.Value)
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
        // チェックモードに応じてスプライトを切り替える
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
