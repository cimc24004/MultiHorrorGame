using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : SingleTonDontDestory<GameController>
{
    // Start is called before the first frame update
    void Start()
    {
        Resources.UnloadUnusedAssets();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
