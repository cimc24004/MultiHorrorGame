using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuController : Singleton<PauseMenuController>
{
    public InputActionReference CommonActionESC;
    private PauseMenuUI pauseMenuUI;
    public bool OpenNow;
    private GameStateController gameStateController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameStateController = GameStateController.instance;
        pauseMenuUI = PauseMenuUI.instance;
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
    }
    public void ClosePauseMenu()
    {
        pauseMenuUI.PausePanel.gameObject.SetActive(false);
    }
}
