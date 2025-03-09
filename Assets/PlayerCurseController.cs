using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;
public class PlayerCurseController : MonoBehaviour
{
    [SerializeField] private Slider CurseGageUI;
    private PlayerStatus player_status;
    private UIFadeController ui_fade_controller;
    private CurseController curse_controller;
    // Start is called before the first frame update
    void Start()
    {
        player_status = PlayerStatus.instance;
        ui_fade_controller = CurseGageUI.GetComponent<UIFadeController>();
        curse_controller = CurseController.instance;
        player_status.OnValueChangeCurseEvent.AddListener(() =>
        {
            Debug.Log("ui_fade_controller: " + ui_fade_controller);
            ui_fade_controller.AutoFadeIn();
        });
        player_status.OnCurseGageMaxEvent.AddListener(() =>
        {
            Debug.Log("------------------OnCurseGageMaxEvent-------------------------");
            CurseGageMaxed();
        });
    }

    // Update is called once per frame
    void Update()
    {
        CurseGageUI.DOValue(player_status.CurseValueNow / 100.0f, 0.3f).SetEase(Ease.Linear).SetLink(this.gameObject);
    }

    private void CurseGageMaxed()
    {
        //Max‚É’B‚µ‚½‚ç
        curse_controller.CurseMaxed(5);
    }

}
