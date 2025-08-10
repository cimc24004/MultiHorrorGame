using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Unity.Netcode;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Linq;
public class GameProgressController : NetworkBehaviour
{
    [Header("お祈りが必要な数")]
    public int NeedPrayCount = 5;

    [Header("現在のお祈り数")]
    public NetworkVariable<int> PrayCountNow = new NetworkVariable<int>(0);

    public bool DeliverItemGeted;
    
    public bool DeliveredToSakura;
    public NetworkVariable<bool> CanEscape = new NetworkVariable<bool>(false);
    private GameProgressUIData game_progress_ui_data;
    private ScoreTimeManager scoreTimeManager;
    private GameStateController gameStateController;
    [SerializeField] private List<ulong> ArrivedExitPlayers;
    // Start is called before the first frame update

    public override void OnNetworkSpawn()
    {
        int index = 0;
        gameStateController = GameStateController.instance;
        scoreTimeManager = ScoreTimeManager.Instance;
        game_progress_ui_data = this.gameObject.GetComponent<GameProgressUIData>();
        game_progress_ui_data.ButsudanCountNowTextUI.text = $"{PrayCountNow.Value}/{NeedPrayCount}";
        game_progress_ui_data.ProgressButtonAfterGameClear.onClick.AddListener(() =>
        {
            ProgressAfterGameClear();
        });
        foreach (GameProgressUIData.GameToDoListUIData game_to_do_list_ui_data in game_progress_ui_data.TodoName_with_game_todo_list_ui_datas.Values)
        {
            if (index != 0)
            {
                game_to_do_list_ui_data.TodoTextUI.alpha = 0.2f;
            }
            index++;
        }
        this.GameStart();
        this.PrayCountNow.OnValueChanged += PrayCountNowOnValueChanged;
    }
    public void GameStart()
    {
        scoreTimeManager.MeasureTimeStart();
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
        OnItemGetedUIChangeAllClientRpc();
        //サーバー側で実行
        DeliverItemGeted = true;
    }
    [ClientRpc]
    private void OnItemGetedUIChangeAllClientRpc()
    {
        FadeOutTextUI(game_progress_ui_data.TodoName_with_game_todo_list_ui_datas["OnItemGeted"].TodoTextUI);
        FadeInTextUI(game_progress_ui_data.TodoName_with_game_todo_list_ui_datas["OnDeliverSakura"].TodoTextUI);
        DOVirtual.DelayedCall(1, () => {
            game_progress_ui_data.TodoName_with_game_todo_list_ui_datas["OnItemGeted"].TodoBox.gameObject.SetActive(false);
        });

    }
    public void OnDeliverSakura()
    {
        CanEscape.Value = true;
        DeliveredToSakura = true;
        OnDeliverSakuraUIChangeAllClientRpc();
    }
    public void ExitGateArrived(ulong _PlayerClientId)
    {
        //サーバー側で実行
        ArrivedExitPlayers.Add(_PlayerClientId);
        var sendParams = new ClientRpcSendParams
        {
            TargetClientIds = new ulong[] { _PlayerClientId }
        };
        NetworkObject ExitGateArrivedPlayer_NetworkObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(_PlayerClientId);
        ExitGateArrivedPlayer_NetworkObject.gameObject.SetActive(false);
        
        //脱出した人だけに送る。
        ExitGateArrivedRequestForArrivedPlayerClientRpc(new ClientRpcParams{Send = sendParams});

        if (ArrivedExitPlayers.Count >= NetworkManager.Singleton.ConnectedClients.Count)
        {
            this.GameClear();
        }
    }
    [ClientRpc]
    private void ExitGateArrivedRequestForArrivedPlayerClientRpc(ClientRpcParams clientRpcParams)
    {
        NetworkObject ExitGateArrivedPlayer_NetworkObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(NetworkManager.Singleton.LocalClientId);
        ExitGateArrivedPlayer_NetworkObject.gameObject.SetActive(false);
    }
    public void GameClear()
    {
        Time.timeScale = 0;
        gameStateController.MenuOnGamePlayMode(true);
        GameClearUIClientRpc();
    }
    public void ProgressAfterGameClear()
    {
        //ゲームをクリアした後次に進むを押した後のイベント。
        NetworkManager.Singleton.SceneManager.LoadScene("Lobby", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
    [ClientRpc]
    private void GameClearUIClientRpc()
    {
        TimeSpan time = TimeSpan.FromSeconds(scoreTimeManager.ScoreTimeNow.Value);

        // "分:秒:ミリ秒" のフォーマットで文字列に変換
        // "mm"は2桁の分、"ss"は2桁の秒、"ff"はミリ秒の上位2桁
        string timeText = time.ToString(@"mm\:ss\:ff");

        game_progress_ui_data.scoreTimeTextUI.SetText($"{timeText}");
        game_progress_ui_data.GameClearPanel.SetActive(true);
    }
    
    [ClientRpc]
    private void OnDeliverSakuraUIChangeAllClientRpc()
    {
        FadeOutTextUI(game_progress_ui_data.TodoName_with_game_todo_list_ui_datas["OnDeliverSakura"].TodoTextUI);
        FadeInTextUI(game_progress_ui_data.TodoName_with_game_todo_list_ui_datas["Escape"].TodoTextUI);
        DOVirtual.DelayedCall(1, () =>
        {
            game_progress_ui_data.TodoName_with_game_todo_list_ui_datas["OnDeliverSakura"].TodoBox.gameObject.SetActive(false);
        });
    }
    private void FadeInTextUI(TextMeshProUGUI _text_ui)
    {
        _text_ui.DOFade(1, 0.5f).SetLink(this.gameObject);
    }
    private void FadeOutTextUI(TextMeshProUGUI _text_ui){
        _text_ui.DOFade(0, 0.5f).SetLink(this.gameObject);
    }
}
