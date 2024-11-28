using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class GameStateControlelr : MonoBehaviour
{
    public enum GameState
    {
        Gameplay,
        MenuOnGamePlay,

    }

    [Header("現在のゲームの状態 メニュー画面だったり、スタート画面ならUIが選択")]
    public GameState game_state;
    private PlayerInput player_input;
    // Start is called before the first frame update
    void Start()
    {
        player_input = this.gameObject.GetComponent<PlayerInput>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ActionByModeSelect(GameState game_state)
    {
        if(game_state == GameState.Gameplay)
        {
            GameplayMode();
        }else if(game_state == GameState.MenuOnGamePlay)
        {
            MenuOnGamePlayMode();
        }
    }

    public void GameplayMode()
    {
        //ゲームをプレイしてるとき

        player_input.actions.FindActionMap("MenuOnGamePlay").Disable();
        player_input.actions.FindActionMap("Player").Enable();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void MenuOnGamePlayMode()
    {
        //ゲームプレイ中にメニューを開いたとき
        player_input.actions.FindActionMap("MenuOnGamePlay").Enable();
        player_input.actions.FindActionMap("Player").Disable();

        //カーソルは表示
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

    }
    public void StartClearOverMode()
    {
        //ゲームオーバーやスタート画面など

    }
}
