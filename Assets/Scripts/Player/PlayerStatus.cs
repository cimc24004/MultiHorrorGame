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
    //今隠れているかどうか
    public bool HidingNow;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
    public void StopEvent()
    {
        //MoveSpeed=0でなかったら押し入れとかに隠れたときにずーと敵が待っているバグが生じる為。
        this.MoveSpeed = 0;
    }
}
