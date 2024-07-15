using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus : Singleton<PlayerStatus>
{
    //ここは値の受け渡しやスタミナなどの管理。 プレイヤーの状態なども保存する

    public Transform Head;
    //プレイヤーの移動状態 (走る 歩く しゃがむ)
    public enum PlayerMoveState
    {
        Run,
        Walk,
        Crouch
    }
    public PlayerMoveState player_move_state;
    public float MoveSpeed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
