using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class GameController : Singleton<GameController>
{
    [SerializeField] private GameObject NetCanvas;
    private PlayerInput player_input;
    [SerializeField]private bool NetMode;
    private GameControllerOnAllScene game_controller_on_all_scene;
    private GameStateController game_state_controller;
    [SerializeField]private InputActionReference  GamePlayAndMenuCommonTabActionReference;


    //ひとまず Networkに接続を簡単にできるようにMainシーンでつなげれれるようにするためのスクリプト部分を記載しています。
    // Start is called before the first frame update
    void Start()
    {
        player_input = PlayerStatus.instance.player_input;
        game_controller_on_all_scene = GameControllerOnAllScene.instance;
        game_state_controller = game_controller_on_all_scene.GetComponent<GameStateController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(GamePlayAndMenuCommonTabActionReference.action.triggered){
            if(!NetMode){
                NetCanvasSetActiveByMode(NetMode);
            }
        }
    }
    public void NetCanvasSetActiveByMode(bool _netMode){
        if(!_netMode){
            game_state_controller.ActionByModeSelect(GameStateController.GameState.MenuOnGamePlay, true);
            NetCanvas.gameObject.SetActive(true);
        }else{
            game_state_controller.ActionByModeSelect(GameStateController.GameState.Gameplay, false);
            NetCanvas.gameObject.SetActive(false);
        }
        this.NetMode = !_netMode;
    }

    
}
