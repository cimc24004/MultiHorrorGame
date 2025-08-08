using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseMenuUI : Singleton<PauseMenuUI>
{
    [SerializeField] private Button ReturnToGameButton;
    [SerializeField] private Button SettingOpenButton;
    [SerializeField] private Button ReturnToMainMenuButton;

    [SerializeField] public GameObject PausePanel;
    [SerializeField] private ConfigController configController;
    private PauseMenuController pauseMenuController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        configController = ConfigController.instance;
        pauseMenuController = PauseMenuController.instance;
        ReturnToGameButton.onClick.AddListener(() =>
        {
            pauseMenuController.OpenNow = false;
            pauseMenuController.ClosePauseMenu();
        });
        SettingOpenButton.onClick.AddListener(() =>
        {
            PausePanel.gameObject.SetActive(false);
            configController.OpenConfigMenu();
        });
        ReturnToMainMenuButton.onClick.AddListener(() =>
        {
            // --- ネットワーク終了 ---
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();   // これ一本でホスト／クライアント両対応
            }
            // 2) Steam ロビーから抜ける
            if (SteamLobby.Instance != null && SteamLobby.Instance.LobbyID != 0)
            {
                SteamLobby.Instance.LeaveCurrentLobby();   // ←後述するメソッド
            }

            // 3) タイトルシーンへ
            UnityEngine.SceneManagement.SceneManager.LoadScene("Title");

            //ゲーム終了です
        });
    }


}
