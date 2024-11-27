using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerMoveScript : MonoBehaviour
{
    //プレイヤー移動スクリプト
    //private Rigidbody rigid;
    private CharacterController character_controller;
    public Vector3 MoveDirection;
    public float RunSpeed = 3;
    public float WalkSpeed = 1;
    public float CrouchSpeed = 1;
    public float NowSpeed;
    public bool CrouchNow;
    //このカメラの方向とプレイヤーの向く方向が同じになるようにする。
    public GameObject DirectionCamera;

    private bool RunWalkTriggerChange;
    private PlayerInput player_input;
    private Vector3 MoveKeyValue;
    
    private InputAction MoveAction;
    private InputAction RunChangeAction;
    private InputAction CrouchChangeAction;
    private TimeManager time_manager;
    private PlayerStatus player_status;
    private PlayerStaminaController player_stamina_controller;
    // Start is called before the first frame update
    void Start()
    {
        //rigid = this.gameObject.GetComponent<Rigidbody>();
        character_controller = this.gameObject.GetComponent<CharacterController>();
        player_stamina_controller = this.gameObject.GetComponent<PlayerStaminaController>();
        NowSpeed = WalkSpeed;
        player_input = GameControllerOnAllScene.instance.player_input;
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
        player_status.MoveY = MoveKeyValue.y;
        player_status.MoveX = MoveKeyValue.x;

        if (MoveKeyValue.magnitude != 0)
        {
            this.transform.localRotation = Quaternion.Euler(0, DirectionCamera.transform.rotation.eulerAngles.y, 0);
            MoveDirection = (this.transform.forward * MoveKeyValue.y) + (this.transform.right * MoveKeyValue.x);
            character_controller.Move(MoveDirection * Time.deltaTime * NowSpeed);
            player_status.MoveSpeed = NowSpeed;
        }
        else
        {
            character_controller.Move(Vector3.Scale(Vector3.up,character_controller.velocity) * Time.deltaTime);
            player_status.MoveSpeed = 0;
        }

        if (player_status.player_move_state == PlayerStatus.PlayerMoveState.Run || player_status.player_move_state == PlayerStatus.PlayerMoveState.Walk)
        {
            if (RunChangeAction.triggered)
            {
                if (player_stamina_controller.StaminaCheck())
                {
                    RunWalkTriggerChange = true;
                }
                else
                {
                    RunWalkTriggerChange = false;
                }

            }
            if (RunWalkTriggerChange)
            {
                if (RunChangeAction.IsPressed())
                {
                    if (!player_stamina_controller.StaminaIncreaseNow)
                    {
                        if (player_status.StaminaValueNow > 0)
                        {
                            //RunChangeキーを長押ししている場合はRunSpeedを代入
                            player_status.player_move_state = PlayerStatus.PlayerMoveState.Run;
                            NowSpeed = RunSpeed;
                        }
                        else
                        {
                            player_status.player_move_state = PlayerStatus.PlayerMoveState.Walk;
                            NowSpeed = WalkSpeed;
                        }
                    }
                    else
                    {
                        if (player_stamina_controller.StaminaCheck())
                        {
                            //RunChangeキーを長押ししている場合はRunSpeedを代入
                            player_status.player_move_state = PlayerStatus.PlayerMoveState.Run;
                            NowSpeed = RunSpeed;
                        }
                        else
                        {
                            player_status.player_move_state = PlayerStatus.PlayerMoveState.Walk;
                            NowSpeed = WalkSpeed;
                        }
                    }


                }
            }



            if (RunChangeAction.WasReleasedThisFrame())
            {
                //RunChangeキーを離した場合はWalkSpeedに戻す。
                player_status.player_move_state = PlayerStatus.PlayerMoveState.Walk;
                NowSpeed = WalkSpeed;
                RunWalkTriggerChange = false;
            }
        }
        if (CrouchChangeAction.triggered)
        {
            if (!CrouchNow)
            {
                //しゃがむ
                player_status.player_move_state = PlayerStatus.PlayerMoveState.Crouch;
                player_status.CrouchStandCameraPosChange(true);
                NowSpeed = CrouchSpeed;
                CrouchNow = true;
            }
            else
            {
                player_status.player_move_state = PlayerStatus.PlayerMoveState.Walk;
                player_status.CrouchStandCameraPosChange(false);
                NowSpeed = WalkSpeed;
                CrouchNow = false;
            }
        }

    }

}
