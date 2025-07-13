using Cinemachine;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuController : Singleton<PauseMenuController>
{
    public InputActionReference CommonActionESC;
    private PauseMenuUI pauseMenuUI;
    public bool OpenNow;
    private GameStateController gameStateController;
    private CMFPSController cMFPSController;
    private ConfigController configController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameStateController = GameStateController.instance;
        pauseMenuUI = PauseMenuUI.instance;
        cMFPSController = CMFPSController.instance;
        configController = ConfigController.instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (CommonActionESC.action.triggered)
        {
            if (!OpenNow)
            {
                this.OpenPauseMenu();
            }
            OpenNow = true;
        }
    }
    public void OpenPauseMenu()
    {
        gameStateController.ActionByModeSelect(GameStateController.GameState.MenuOnGamePlay, true);
        pauseMenuUI.PausePanel.gameObject.SetActive(true);
        cMFPSController.enabled = false;
        cMFPSController.InputProviderEnable(false);
    }
    public void ClosePauseMenu()
    {
        gameStateController.ActionByModeSelect(GameStateController.GameState.Gameplay, false);
        pauseMenuUI.PausePanel.gameObject.SetActive(false);
        cMFPSController.enabled = true;
        cMFPSController.InputProviderEnable(true);
        cMFPSController.MouseSensivityChange(configController.currentConfig.MouseSensitivity);
    }

}
