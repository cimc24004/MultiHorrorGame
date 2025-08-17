using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class GameController : Singleton<GameController>
{
    private GameControllerOnAllScene game_controller_on_all_scene;
    private GameStateController game_state_controller;
    public ReadEventUIController readEventUIController;

    public MiniTutorialController miniTutorialController;
    //ひとまず Networkに接続を簡単にできるようにMainシーンでつなげれれるようにするためのスクリプト部分を記載しています。
    // Start is called before the first frame update
    void Start()
    {
        game_controller_on_all_scene = GameControllerOnAllScene.instance;
        game_state_controller = game_controller_on_all_scene.GetComponent<GameStateController>();
    }


    
}
