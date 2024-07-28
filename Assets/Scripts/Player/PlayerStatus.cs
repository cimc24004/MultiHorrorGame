using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus : Singleton<PlayerStatus>
{
    //ここは値の受け渡しやスタミナなどの管理。 プレイヤーの状態なども保存する
    public Transform Head;
    //死んだかどうか
    public bool Dead;
    //プレイヤーの移動状態 (走る 歩く しゃがむ)
    public enum PlayerMoveState
    {
        Run,
        Walk,
        Crouch,
        Idle
    }
    public PlayerMoveState player_move_state;
    public float MoveSpeed;
    //今隠れているかどうか
    public bool HidingNow;

    private CharactorMovePermit player_move_permit;
    // Start is called before the first frame update
    void Start()
    {
        player_move_permit = this.gameObject.GetComponent<CharactorMovePermit>();

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
    public void DeadEvent(GameObject KilledMeEnemy)
    {
        //プレイヤーが死ぬときのイベント。
        Debug.Log("あなたは死にました！");
        //恐怖演出用のイベントを実行。
        CameraEnemyFaceExpanding camera_enemy_face_expanding = KilledMeEnemy.GetComponent<CameraEnemyFaceExpanding>();
        camera_enemy_face_expanding.FaceExpandingActive(this.gameObject);
        this.gameObject.layer = LayerMask.NameToLayer("DeadPlayer");
        player_move_permit.Stop();

    }
}
