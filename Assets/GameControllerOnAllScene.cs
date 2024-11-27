using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameControllerOnAllScene : SingleTonDontDestory<GameControllerOnAllScene>
{
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
}
