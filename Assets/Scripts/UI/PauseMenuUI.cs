using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
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
            SessionController sessionController = SessionController.Instance;
            if (NetworkManager.Singleton.IsServer)
            {
                NetworkManager.Singleton.SceneManager.LoadScene("Lobby", UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                sessionController.ReturnToTitle();
            }
        });
    }


}
