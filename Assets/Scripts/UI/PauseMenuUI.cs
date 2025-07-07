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
            configController.OpenConfigMenu();
        });
        ReturnToMainMenuButton.onClick.AddListener(() =>
        {
            //ゲーム終了です
        });
    }


}
