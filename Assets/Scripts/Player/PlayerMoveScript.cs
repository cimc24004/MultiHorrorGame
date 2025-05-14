using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMoveScript : NetworkBehaviour
{
    // Inspectorで調整
    [Header("移動速度")]
    public float RunSpeed = 3f;
    public float WalkSpeed = 1f;
    public float CrouchSpeed = 1f;
    [Header("回転の滑らかさ（大きいほど速く追従）")]
    [SerializeField] private float rotationSmoothSpeed = 5f;  // 使わないけど残しておく
    [Header("カメラ参照")]
    private GameObject DirectionCamera;
    // 内部用
    private CharacterController characterController;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction runChangeAction;
    private InputAction crouchChangeAction;
    private Animator anim;
    private Vector3 moveDirection = Vector3.zero;
    public float NowSpeed;
    public bool MovePermit = true;
    private float defaultRunSpeed;
    private float defaultWalkSpeed;
    private float defaultCrouchSpeed;
    private bool runWalkTriggerChange = false;
    public bool CrouchNow = false;

    [SerializeField]private PlayerStatus playerStatus;
    private PlayerStaminaController staminaController;
    private SubNoticedUIController subNoticedUI;
    private TimeManager timeManager;

    // スピードアップ用
    private bool moveSpeedUpNow = false;
    private float moveSpeedUpDuration;
    private float moveSpeedUpTimer = 0f;
    private Vector2 moveInput;
    private bool CrouchPressed;
    private bool RunChangePressed;
    public override void OnNetworkSpawn()
    {
        characterController = GetComponent<CharacterController>();
        playerStatus = this.gameObject.GetComponent<PlayerStatus>();
        playerInput = playerStatus.player_input;
        Debug.Log("playerInput: " + playerInput);
        moveAction = playerInput.actions["Move"];
        runChangeAction = playerInput.actions["RunChange"];
        crouchChangeAction = playerInput.actions["CrouchChange"];
        anim = this.gameObject.GetComponent<Animator>();
        defaultRunSpeed = RunSpeed;
        defaultWalkSpeed = WalkSpeed;
        defaultCrouchSpeed = CrouchSpeed;
        NowSpeed = WalkSpeed;

        staminaController = GetComponent<PlayerStaminaController>();
        subNoticedUI = playerStatus.sub_noticed_ui_controller;
        timeManager = TimeManager.instance;
        DirectionCamera = playerStatus.virtual_camera.VirtualCameraGameObject;
    }

    void Update()
    {
        if(!MovePermit && IsServer){
            characterController.Move(Vector3.zero);
            // playerStatus.MoveY = 0;
            // playerStatus.MoveX = 0;
            return;
        }
        if(IsOwner){
            MoveInputSendServerRPC(moveAction.ReadValue<Vector2>());
            ModeChangeServerRPC(crouchChangeAction.triggered, runChangeAction.triggered);
        }
        if(IsServer){
            // 入力取得＆移動方向設定
            anim.SetBool("Crouch", playerStatus.Crouch);
            if (moveInput.sqrMagnitude > 0.01f)
            {
                Quaternion camRot = Quaternion.Euler(0f, DirectionCamera.transform.eulerAngles.y, 0f);
                Vector3 forward = camRot * Vector3.forward;
                Vector3 right   = camRot * Vector3.right;
                moveDirection = (forward * moveInput.y + right * moveInput.x).normalized;
                playerStatus.MoveSpeed = NowSpeed;
            }
            else
            {
                moveDirection = Vector3.zero;
                playerStatus.MoveSpeed = 0f;
            }

            // 走る／歩く処理
            HandleRunWalk();
            // しゃがみ処理
            HandleCrouch();
            // スピードアップタイマー
            if (moveSpeedUpNow)
            {
                moveSpeedUpTimer += Time.deltaTime;
                if (moveSpeedUpTimer >= moveSpeedUpDuration)
                {
                    RunSpeed    = defaultRunSpeed;
                    WalkSpeed   = defaultWalkSpeed;
                    CrouchSpeed = defaultCrouchSpeed;
                    ResetMoveSpeed();
                    moveSpeedUpNow   = false;
                    moveSpeedUpTimer = 0f;
                }
            }
        }

    }

    void FixedUpdate()
    {
        if(IsServer){
            if(!MovePermit) return;
            // 実際の移動
            if (moveDirection != Vector3.zero)
                characterController.Move(moveDirection * NowSpeed * Time.fixedDeltaTime);
            else
                characterController.Move(Vector3.up * characterController.velocity.y * Time.fixedDeltaTime);
        }

    }


    private void HandleRunWalk()
    {
        if (playerStatus.player_move_state == PlayerStatus.PlayerMoveState.Run ||
            playerStatus.player_move_state == PlayerStatus.PlayerMoveState.Walk)
        {
            if (runChangeAction.triggered)
                runWalkTriggerChange = staminaController.StaminaCheck();

            if (runWalkTriggerChange && runChangeAction.IsPressed())
            {
                bool canRun = staminaController.StaminaIncreaseNow
                    ? staminaController.StaminaCheck()
                    : (playerStatus.StaminaValueNow > 0);
                if (canRun)
                {
                    playerStatus.player_move_state = PlayerStatus.PlayerMoveState.Run;
                    NowSpeed = RunSpeed;
                }
                else
                {
                    playerStatus.player_move_state = PlayerStatus.PlayerMoveState.Walk;
                    NowSpeed = WalkSpeed;
                }
            }

            if (runChangeAction.WasReleasedThisFrame())
            {
                playerStatus.player_move_state = PlayerStatus.PlayerMoveState.Walk;
                NowSpeed = WalkSpeed;
                runWalkTriggerChange = false;
            }
        }
    }

    private void HandleCrouch()
    {
        if (CrouchPressed)
        {
            if (!CrouchNow)
            {
                playerStatus.player_move_state = PlayerStatus.PlayerMoveState.Crouch;
                playerStatus.CrouchStandCameraPosChange(true);
                NowSpeed = CrouchSpeed;
                CrouchNow = true;
            }
            else
            {
                playerStatus.player_move_state = PlayerStatus.PlayerMoveState.Walk;
                playerStatus.CrouchStandCameraPosChange(false);
                NowSpeed = WalkSpeed;
                CrouchNow = false;
            }
        }
    }


    public void MoveSpeedUp(float addSpeed, float effectTime)
    {
        RunSpeed    += addSpeed;
        WalkSpeed   += addSpeed * 0.5f;
        CrouchSpeed += addSpeed * 0.25f;
        moveSpeedUpDuration = effectTime;
        moveSpeedUpNow      = true;
        moveSpeedUpTimer    = 0f;
    }

    private void ResetMoveSpeed()
    {
        subNoticedUI.Notice("脚力が元に戻った");
    }
    [ServerRpc]
    private void MoveInputSendServerRPC(Vector2 _moveInput){
        this.moveInput = _moveInput;
        playerStatus.MoveY = _moveInput.y;
        playerStatus.MoveX = _moveInput.x;
    }

    [ServerRpc]
    private void ModeChangeServerRPC(bool _crouchPressed, bool _runChangePressed){
        this.CrouchPressed = _crouchPressed;
        this.RunChangePressed = _runChangePressed;
    }
}
