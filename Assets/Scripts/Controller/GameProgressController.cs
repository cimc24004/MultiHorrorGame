using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Unity.Netcode;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
public class GameProgressController : NetworkBehaviour
{
    [Header("お祈りが必要な数")]
    public int NeedPrayCount = 5;

    [Header("現在のお祈り数")]
    public NetworkVariable<int> PrayCountNow = new NetworkVariable<int>(0);

    public bool DeliverItemGeted;
    
    public bool DeliveredToSakura;

    private GameProgressUIData game_progress_ui_data;
    // Start is called before the first frame update

    public override void OnNetworkSpawn()
    {
        int index = 0;
        game_progress_ui_data = this.gameObject.GetComponent<GameProgressUIData>();
        game_progress_ui_data.ButsudanCountNowTextUI.text = $"{PrayCountNow.Value}/{NeedPrayCount}";
        foreach (GameProgressUIData.GameToDoListUIData game_to_do_list_ui_data in game_progress_ui_data.TodoName_with_game_todo_list_ui_datas.Values)
        {
            if (index != 0)
            {
                game_to_do_list_ui_data.TodoTextUI.alpha = 0.2f;
            }
            index++;
        }
        this.PrayCountNow.OnValueChanged += PrayCountNowOnValueChanged;
    }
    public void OnPrayedButsudan()
    {
        PrayCountNow.Value += 1;
    }
    private void PrayCountNowOnValueChanged(int _oldPrayCountNow, int _PrayCountNow)
    {
        //全クライアントに共有
        Debug.Log("OnValueChanged1 : " + _PrayCountNow);
        Debug.Log("OnValueChanged2 : " + _oldPrayCountNow);
        Debug.Log("NeedPrayCount: " + NeedPrayCount);
        game_progress_ui_data.ButsudanCountNowTextUI.text = $"{_PrayCountNow}/{NeedPrayCount}";
        if (_PrayCountNow >= NeedPrayCount)
        {
            FadeOutTextUI(game_progress_ui_data.TodoName_with_game_todo_list_ui_datas["OnPrayedButsudan"].TodoTextUI);
            FadeOutTextUI(game_progress_ui_data.ButsudanCountNowTextUI);
            FadeInTextUI(game_progress_ui_data.TodoName_with_game_todo_list_ui_datas["OnItemGeted"].TodoTextUI);
            game_progress_ui_data.ButsudanCountNowTextUI.text = $"{_PrayCountNow}/{NeedPrayCount}";
            DOVirtual.DelayedCall(1, () => {
                game_progress_ui_data.TodoName_with_game_todo_list_ui_datas["OnPrayedButsudan"].TodoBox.gameObject.SetActive(false);
            });
        }
    }
    public void OnItemGeted()
    {
        DeliverItemGeted = true;
        FadeOutTextUI(game_progress_ui_data.TodoName_with_game_todo_list_ui_datas["OnItemGeted"].TodoTextUI);
        FadeInTextUI(game_progress_ui_data.TodoName_with_game_todo_list_ui_datas["OnDeliverSakura"].TodoTextUI);
        DOVirtual.DelayedCall(1, () => {
            game_progress_ui_data.TodoName_with_game_todo_list_ui_datas["OnItemGeted"].TodoBox.gameObject.SetActive(false);
        });
    }

    public void OnDeliverSakura(){
        DeliveredToSakura = true;
        FadeOutTextUI(game_progress_ui_data.TodoName_with_game_todo_list_ui_datas["OnDeliverSakura"].TodoTextUI);
        FadeInTextUI(game_progress_ui_data.TodoName_with_game_todo_list_ui_datas["Escape"].TodoTextUI);
        DOVirtual.DelayedCall(1, () => {
            game_progress_ui_data.TodoName_with_game_todo_list_ui_datas["OnDeliverSakura"].TodoBox.gameObject.SetActive(false);
        });
    }

    private void FadeInTextUI(TextMeshProUGUI _text_ui){
        _text_ui.DOFade(1, 0.5f).SetLink(this.gameObject);
    }
    private void FadeOutTextUI(TextMeshProUGUI _text_ui){
        _text_ui.DOFade(0, 0.5f).SetLink(this.gameObject);
    }
}
