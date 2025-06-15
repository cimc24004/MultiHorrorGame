using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Unity.Netcode;
public class GameProgressController : NetworkSingleton<GameProgressController>
{
    [Header("お祈りが必要な数")]
    public int NeedPrayCount = 5;

    [Header("現在のお祈り数")]
    public NetworkVariable<int> PrayCountNow = new NetworkVariable<int>(0);

    public bool DeliverItemGeted;
    
    public bool DeliveredToSakura;

    private class GameToDoListUIData{
        public TextMeshProUGUI TodoTextUI;
        public GameObject TodoBox;
    }

    [Header("UI情報")]
    [SerializeField] private Dictionary<string, GameToDoListUIData> TodoName_with_game_todo_list_ui_datas;
    [SerializeField]private TextMeshProUGUI ButsudanCountNowTextUI;
    // Start is called before the first frame update

    public override void OnNetworkSpawn()
    {
        ButsudanCountNowTextUI.text = $"{PrayCountNow}/{NeedPrayCount}";
        int index = 0;
        foreach(GameToDoListUIData game_to_do_list_ui_data in TodoName_with_game_todo_list_ui_datas.Values){
            if(index != 0){
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
    private void PrayCountNowOnValueChanged(int _PrayCountNow, int _)
    {
        //全クライアントに共有
        ButsudanCountNowTextUI.text = $"{_PrayCountNow}/{NeedPrayCount}";
        FadeOutTextUI(TodoName_with_game_todo_list_ui_datas["OnPrayedButsudan"].TodoTextUI);
        FadeOutTextUI(ButsudanCountNowTextUI);
        FadeInTextUI(TodoName_with_game_todo_list_ui_datas["OnItemGeted"].TodoTextUI);
        if (_PrayCountNow >= NeedPrayCount)
        {
            ButsudanCountNowTextUI.text = $"{_PrayCountNow}/{NeedPrayCount}";
            DOVirtual.DelayedCall(1, () => {
                TodoName_with_game_todo_list_ui_datas["OnPrayedButsudan"].TodoBox.gameObject.SetActive(false);
            });
        }
    }
    public void OnItemGeted()
    {
        DeliverItemGeted = true;
        FadeOutTextUI(TodoName_with_game_todo_list_ui_datas["OnItemGeted"].TodoTextUI);
        FadeInTextUI(TodoName_with_game_todo_list_ui_datas["OnDeliverSakura"].TodoTextUI);
        DOVirtual.DelayedCall(1, () => {
            TodoName_with_game_todo_list_ui_datas["OnItemGeted"].TodoBox.gameObject.SetActive(false);
        });
    }

    public void OnDeliverSakura(){
        DeliveredToSakura = true;
        FadeOutTextUI(TodoName_with_game_todo_list_ui_datas["OnDeliverSakura"].TodoTextUI);
        FadeInTextUI(TodoName_with_game_todo_list_ui_datas["Escape"].TodoTextUI);
        DOVirtual.DelayedCall(1, () => {
            TodoName_with_game_todo_list_ui_datas["OnDeliverSakura"].TodoBox.gameObject.SetActive(false);
        });
    }

    private void FadeInTextUI(TextMeshProUGUI _text_ui){
        _text_ui.DOFade(1, 0.5f).SetLink(this.gameObject);
    }
    private void FadeOutTextUI(TextMeshProUGUI _text_ui){
        _text_ui.DOFade(0, 0.5f).SetLink(this.gameObject);
    }
}
