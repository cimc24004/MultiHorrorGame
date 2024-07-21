using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerMoveScript : MonoBehaviour
{
    //プレイヤー移動スクリプト
    private Rigidbody rigid;
    public Vector3 MoveDirection;
    public float RunSpeed = 3;
    public float WalkSpeed = 1;
    public float CrouchSpeed = 1;
    public float NowSpeed;
    public bool CrouchNow;
    //このカメラの方向とプレイヤーの向く方向が同じになるようにする。
    public GameObject DirectionCamera;

    private float TimeWaitNow;
    private PlayerInput player_input;
    private Vector3 MoveKeyValue;
    
    private InputAction MoveAction;
    private InputAction RunChangeAction;
    private InputAction CrouchChangeAction;
    private TimeManager time_manager;
    private PlayerStatus player_status;
    // Start is called before the first frame update
    void Start()
    {
        rigid = this.gameObject.GetComponent<Rigidbody>();
        NowSpeed = WalkSpeed;
        player_input = this.gameObject.GetComponent<PlayerInput>();
        MoveAction = player_input.currentActionMap["Move"];
        RunChangeAction = player_input.currentActionMap["RunChange"];
        CrouchChangeAction = player_input.currentActionMap["CrouchChange"];

        time_manager = TimeManager.instance;
        player_status = PlayerStatus.instance;
    }

    // Update is called once per frame
    void Update()
    {
        MoveKeyValue = MoveAction.ReadValue<Vector2>();
        if(player_status.player_move_state == PlayerStatus.PlayerMoveState.Run || player_status.player_move_state == PlayerStatus.PlayerMoveState.Walk)
        {
            if (RunChangeAction.IsPressed())
            {
                //RunChangeキーを長押ししている場合はRunSpeedを代入
                player_status.player_move_state = PlayerStatus.PlayerMoveState.Run;
                NowSpeed = RunSpeed;
            }
            if (RunChangeAction.WasReleasedThisFrame())
            {
                //RunChangeキーを離した場合はWalkSpeedに戻す。
                player_status.player_move_state = PlayerStatus.PlayerMoveState.Walk;
                NowSpeed = WalkSpeed;
            }
        }
        if (CrouchChangeAction.triggered)
        {
            if (!CrouchNow)
            {
                //しゃがむ
                player_status.player_move_state = PlayerStatus.PlayerMoveState.Crouch;
                NowSpeed = CrouchSpeed;
                CrouchNow = true;
            }
            else
            {
                player_status.player_move_state = PlayerStatus.PlayerMoveState.Walk;
                NowSpeed = WalkSpeed;
                CrouchNow = false;
            }
        }

    }
    private void FixedUpdate()
    {
        if(MoveKeyValue.magnitude != 0)
        {
            this.transform.localRotation = Quaternion.Euler(0, DirectionCamera.transform.rotation.eulerAngles.y, 0);
            MoveDirection = (this.transform.forward * MoveKeyValue.y) + (this.transform.right * MoveKeyValue.x);
            rigid.velocity = MoveDirection * NowSpeed;
            player_status.MoveSpeed = rigid.velocity.magnitude;
        }
        else
        {
            rigid.velocity = Vector3.zero;
            player_status.MoveSpeed = 0;
        }
    }
}
