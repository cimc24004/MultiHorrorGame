using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
public class StageController : Singleton<StageController>
{
    public CursorLockMode DefaultCursorLockMode;
    public string NowSceneName;
    private GameControllerOnAllScene game_controller_on_all_scene;
    private GameStateController game_state_controller;

    public UnityEvent2 StartEvent;

    public GameStateController.GameState game_state;
    public bool CursorVisible;
    //カーソルのロックはステージごとに違う可能性がある(GameOverやGameStart画面などではLockはしておかない)
    //なのでステージごとに配置するコントローラーを分けている。

    // Start is called before the first frame update
    void Start()
    {
        game_controller_on_all_scene = GameControllerOnAllScene.instance;
        game_state_controller = game_controller_on_all_scene.gameObject.GetComponent<GameStateController>();
        game_state_controller.ActionByModeSelect(game_state, CursorVisible);
        StartEvent.Invoke();
        Cursor.lockState = DefaultCursorLockMode;
        if(DefaultCursorLockMode != CursorLockMode.None)
            Cursor.visible = false;
        NowSceneName = SceneManager.GetActiveScene().name;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
