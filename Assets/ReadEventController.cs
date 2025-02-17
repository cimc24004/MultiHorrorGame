using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadEventController : MonoBehaviour
{
    private GameControllerOnAllScene game_controller_on_all_scene;
    private ReadEventUIController read_event_ui_controller;

    [TextArea]
    public List<string> Sentence;  // 1ページごとに表示する文章

    void Start()
    {
        game_controller_on_all_scene = GameControllerOnAllScene.instance;
        read_event_ui_controller = game_controller_on_all_scene.read_event_ui_controller;
    }

    // 本を読み始めるときに呼ばれる
    public void ReadEventStart()
    {
        // UIコントローラーに「全ページ文章リスト」を渡す
        // 以降UIで文章ページの制御が可能になる
        read_event_ui_controller.StartReading(Sentence);
    }
}
