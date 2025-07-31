using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class ReadEventUIController : MonoBehaviour
{
    // UI表示に必要な変数
    public GameObject CanvasUI;
    public TextMeshProUGUI sentenceUI;

    // シングルトンなど他システムへの参照
    private GameControllerOnAllScene game_controller_on_all_scene;
    private PlayerStatus player_status;
    private AudioSource audio_source;
    private PlayerInput player_input;
    private GameStateController game_state_controller;

    // ページ管理用
    private int Page;        // 現在のページ (1-based)
    private int MaxPage;     // 最大ページ数
    private bool DisplayNow; // 今本を表示しているか

    // 表示する文章一覧
    private List<string> sentences = new List<string>();

    private InputAction ESCAction;

    private InputAction NextPageAction;
    private InputAction BeforePageAction;
    [SerializeField]
    private float TimeToPermitPageChange = 0.5f;
    [SerializeField]
    private float TimeToPermitPageChangeNow = 0.5f;
    [SerializeField]
    private GameObject VectorPageUI;
    [SerializeField]
    private GameObject NextUI;
    [SerializeField]
    private GameObject BeforeUI;
    [SerializeField]
    private TextMeshProUGUI PageNowCountTextUI;
    [SerializeField]
    private TextMeshProUGUI MaxPageCountTextUI;
    private URPFxController urpFxController;
    private CMFPSController cMFPSController;
    void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InitOnPlayerSpawn;
    }
    void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitOnPlayerSpawn;
    }
    void Start()
    {
        cMFPSController = CMFPSController.instance;
        urpFxController = URPFxController.instance;
        audio_source = this.gameObject.GetComponent<AudioSource>();
        game_controller_on_all_scene = GameControllerOnAllScene.instance;
        game_state_controller = game_controller_on_all_scene.GetComponent<GameStateController>();

    }

    private void InitOnPlayerSpawn(PlayerStatus _playerStatus){
        player_status = _playerStatus;
        player_input = player_status.player_input;
        ESCAction = player_input.actions.FindActionMap("ReadEvent")["ESC"];
        NextPageAction = player_input.actions.FindActionMap("ReadEvent")["NextPage"];
        BeforePageAction = player_input.actions.FindActionMap("ReadEvent")["BeforePage"];
    }
    void Update()
    {
        if (!DisplayNow) return;
        Debug.Log("DIsplayNowwwww: " + player_input.actions);
        if (ESCAction.triggered)
        {
            CloseBook();
        }
        // 次ページへ進む処理 (例: "NavigateNext" など適宜ActionMapで設定)
        TimeToPermitPageChangeNow += Time.deltaTime;
        if (NextPageAction.WasPerformedThisFrame())
        {
            // ここでは「右スティック or キー入力などで次ページに進む」処理にしてる
            // 実際は「NavigateNext」「NavigatePrev」などを分けてもOK
            if(TimeToPermitPageChange < TimeToPermitPageChangeNow)
            {
                GoToNextPage();
                TimeToPermitPageChangeNow = 0;
            }
        }
        else if (BeforePageAction.WasPerformedThisFrame())
        {
            if (TimeToPermitPageChange < TimeToPermitPageChangeNow)
            {
                GoToPreviousPage();
                TimeToPermitPageChangeNow = 0;
            }
        }
    }

    /// <summary>
    /// 本を開き、各ページ分の文章をセットして表示開始
    /// </summary>
    public void StartReading(List<string> _sentences)
    {
        if (_sentences == null || _sentences.Count == 0) return;
        cMFPSController.InputProviderEnable(false);
        // 本文リストをセット
        sentences = _sentences;
        urpFxController.CameraMosaic(true);
        // 1ページ目を表示する処理
        Page = 1;
        MaxPage = sentences.Count;
        // 画面表示の更新
        game_state_controller.ReadEventMode(true);
        CanvasUI.SetActive(true);
        sentenceUI.SetText(sentences[0]);  // 最初のページ文をセット
        MaxPageCountTextUI.text = MaxPage.ToString();
        audio_source.Play();
        DisplayNow = true;
        NextAndBeforePageUIDisplayCheck();
    }

    /// <summary>
    /// 次のページに進む処理
    /// </summary>
    private void GoToNextPage()
    {
        // まだ最終ページでないなら次へ
        if (Page < MaxPage)
        {
            Page++;
            sentenceUI.SetText(sentences[Page - 1]); // Pageは1-basedなので-1
            audio_source.Play();
            NextAndBeforePageUIDisplayCheck();
        }
        else
        {
            // 最終ページなら閉じる
            //CloseBook();
        }
    }

    /// <summary>
    /// 前のページに戻る処理
    /// </summary>
    private void GoToPreviousPage()
    {
        // 1ページ目より前には戻れないようにする
        if (Page > 1)
        {
            Page--;
            sentenceUI.SetText(sentences[Page - 1]);
            audio_source.Play();
            NextAndBeforePageUIDisplayCheck();
        }
    }

    /// <summary>
    /// 本を閉じる (UI消してリセット)
    /// </summary>
    private void CloseBook()
    {
        DisplayNow = false;
        game_state_controller.GameplayMode();
        CanvasUI.SetActive(false);
        sentenceUI.SetText("");
        urpFxController.CameraMosaic(false);
        cMFPSController.InputProviderEnable(true);
    }

    // 次のページ、前のページへの表示をチェックしている
    private void NextAndBeforePageUIDisplayCheck()
    {
        if(MaxPage == 1)
        {
            VectorPageUI.SetActive(false);
            return;
        }
        PageNowCountTextUI.text = Page.ToString();
        VectorPageUI.SetActive(true);
        if (MaxPage > Page)
        {
            NextUI.SetActive(true);
            BeforeUI.SetActive(true);
        }
        else
        {
            NextUI.SetActive(false);
            BeforeUI.SetActive(true);
        }
        if (Page != 1)
        {
            BeforeUI.SetActive(true);
        }
        else
        {
            BeforeUI.SetActive(false);
        }
    }
}
