using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
public class GameStateController : Singleton<GameStateController>
{
    public enum GameState
    {
        Gameplay,
        MenuOnGamePlay,
        Menu
    }

    [Header("現在のゲームの状態 メニュー画面だったり、スタート画面ならUIが選択")]
    public GameState game_state;
    private PlayerInput player_input;
    void Start()
    {

    }
    void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += OnPlayerSpawnInit;
        if (PlayerStatus.LocalInstance != null)
        {
            OnPlayerSpawnInit(PlayerStatus.LocalInstance);
        }
        SceneManager.sceneLoaded += SceneLoaded;
    }
    void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= OnPlayerSpawnInit;
    }
    private void OnPlayerSpawnInit(PlayerStatus _playerStatus)
    {
        this.player_input = _playerStatus.player_input;
    }
    public void ActionByModeSelect(GameState game_state, bool cursor_display)
    {
        if (game_state == GameState.Gameplay)
        {
            GameplayMode();
        }
        else if (game_state == GameState.MenuOnGamePlay)
        {
            MenuOnGamePlayMode(cursor_display);
        }
    }

    public void GameplayMode()
    {
        //ゲームをプレイしてるとき

        player_input.actions.FindActionMap("MenuOnGamePlay").Disable();
        player_input.actions.FindActionMap("ReadEvent").Disable();
        player_input.actions.FindActionMap("Player").Enable();
        player_input.actions.FindActionMap("GamePlayAndMenuCommon").Enable();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("GameplayModeGameplayModeGameplayModeGameplayModeGameplayMode");
        EventSystem.current.sendNavigationEvents = true;
    }

    public void MenuOnGamePlayMode(bool visibleCursor)
    {
        //ゲームプレイ中にメニューを開いたとき

        player_input.actions.FindActionMap("MenuOnGamePlay").Enable();
        player_input.actions.FindActionMap("Player").Disable();
        player_input.actions.FindActionMap("ReadEvent").Disable();
        player_input.actions.FindActionMap("GamePlayAndMenuCommon").Enable();
        if (visibleCursor)
        {
            //カーソルは表示
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        EventSystem.current.sendNavigationEvents = false;
    }

    public void ReadEventMode(bool visibleCursor)
    {
        player_input.actions.FindActionMap("Player").Disable();
        player_input.actions.FindActionMap("MenuOnGamePlay").Disable();
        player_input.actions.FindActionMap("ReadEvent").Enable();

        player_input.actions.FindActionMap("GamePlayAndMenuCommon").Disable();
        if (visibleCursor)
        {
            //カーソルは表示
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        EventSystem.current.sendNavigationEvents = false;
    }
    public void StartClearOverMode()
    {
        //ゲームオーバーやスタート画面など

    }
    private void SceneLoaded(Scene _scene, LoadSceneMode sceneMode)
    {
        if (_scene.name.Contains("Main") || _scene.name.Contains("Stage"))
        {
            game_state = GameState.Gameplay;
        }
        else if (_scene.name.Contains("Lobby") || _scene.name.Contains("Title") || _scene.name.Contains("GameStart"))
        {
            game_state = GameState.Menu;
        }
    }
}
