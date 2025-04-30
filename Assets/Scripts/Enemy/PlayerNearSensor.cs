using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNearSensor : MonoBehaviour
{
    [Header("プレイヤーとの距離が近い時にHorrorDirectorのNearEnemyの加算対象となるか")]
    public bool IsNearEffect = true;
    [Header("プレイヤーとの距離によって殺される感じのエフェクト")]
    public bool IsNearToKillEffect = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
