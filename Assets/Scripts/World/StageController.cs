using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class StageController : Singleton<StageController>
{
    public CursorLockMode DefaultCursorLockMode;
    public string NowSceneName;
    //カーソルのロックはステージごとに違う可能性がある(GameOverやGameStart画面などではLockはしておかない)
    //なのでステージごとに配置するコントローラーを分けている。

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = DefaultCursorLockMode;
        Cursor.visible = false;
        NowSceneName = SceneManager.GetActiveScene().name;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
