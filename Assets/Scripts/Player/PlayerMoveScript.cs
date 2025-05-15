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

    [SerializeField] private PlayerStatus playerStatus;
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
        playerStatus       = this.gameObject.GetComponent<PlayerStatus>();
        playerInput        = playerStatus.player_input;
        moveAction         = playerInput.actions["Move"];
        runChangeAction    = playerInput.actions["RunChange"];
        crouchChangeAction = playerInput.actions["CrouchChange"];
        anim               = this.gameObject.GetComponent<Animator>();
        defaultRunSpeed    = RunSpeed;
        defaultWalkSpeed   = WalkSpeed;
        defaultCrouchSpeed = CrouchSpeed;
        NowSpeed           = WalkSpeed;

        staminaController = GetComponent<PlayerStaminaController>();
        subNoticedUI      = playerStatus.sub_noticed_ui_controller;
        timeManager       = TimeManager.instance;
        DirectionCamera   = playerStatus.virtual_camera.VirtualCameraGameObject;

        if(IsOwner){
            crouchChangeAction.performed += OnLocalCrouch;
        }

        // しゃがみ状態が変わったら全クライアントで反映
        playerStatus.Crouch.OnValueChanged += (_, newVal) =>
        {
            // サーバー権限で決まった本当の状態を反映
            ApplyCrouch(newVal);
        };

    }

    void Update()
    {
        if (!MovePermit && IsServer)
        {
            characterController.Move(Vector3.zero);
            return;
        }

        if (IsOwner)
        {
            moveInput = moveAction.ReadValue<Vector2>();

            float yaw = DirectionCamera.transform.eulerAngles.y;
            Vector3 forward = Quaternion.Euler(0, yaw, 0) * Vector3.forward;
            Vector3 right   = Quaternion.Euler(0, yaw, 0) * Vector3.right;
            Vector3 worldDir = (forward * moveInput.y + right * moveInput.x).normalized;

            SubmitMoveServerRpc(moveInput,
                                crouchChangeAction.triggered,
                                runChangeAction.triggered,
                                worldDir);
        }

        if (IsServer)
        {
            anim.SetBool("Crouch", playerStatus.Crouch.Value);

            if (moveInput.sqrMagnitude > 0.01f)
            {
                playerStatus.MoveSpeed = NowSpeed;
            }
            else
            {
                moveDirection          = Vector3.zero;
                playerStatus.MoveSpeed = 0f;
            }

            HandleRunWalk();
            HandleCrouch();

            if (moveSpeedUpNow)
            {
                moveSpeedUpTimer += Time.deltaTime;
                if (moveSpeedUpTimer >= moveSpeedUpDuration)
                {
                    RunSpeed       = defaultRunSpeed;
                    WalkSpeed      = defaultWalkSpeed;
                    CrouchSpeed    = defaultCrouchSpeed;
                    ResetMoveSpeed();
                    moveSpeedUpNow   = false;
                    moveSpeedUpTimer = 0f;
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (IsServer)
        {
            if (!MovePermit) return;

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
                playerStatus.player_move_state      = PlayerStatus.PlayerMoveState.Crouch;
                playerStatus.CrouchStandCameraPosChange(true);
                NowSpeed                             = CrouchSpeed;
                CrouchNow                            = true;
            }
            else
            {
                playerStatus.player_move_state      = PlayerStatus.PlayerMoveState.Walk;
                playerStatus.CrouchStandCameraPosChange(false);
                NowSpeed                             = WalkSpeed;
                CrouchNow                            = false;
            }
        }
    }
    private void ApplyCrouch(bool crouch)
    {
        playerStatus.CrouchStandCameraPosChange(crouch);
        anim.SetBool("Crouch", crouch);
        // 速度もここで切り替え
        playerStatus.player_move_state = crouch
            ? PlayerStatus.PlayerMoveState.Crouch
            : PlayerStatus.PlayerMoveState.Walk;
        // NowSpeed は Update/FixedUpdate 内で playerStatus.player_move_state に応じて参照するようにする
    }
    private void OnLocalCrouch(InputAction.CallbackContext ctx)
    {
        // 1) ローカル予測：見た目を即変更
        bool predicted = !playerStatus.Crouch.Value;
        ApplyCrouch(predicted);

        // 2) サーバーに「こうなったよ」を送信
        SyncCrouchServerRpc(predicted);
    }
    [ServerRpc]
    private void SyncCrouchServerRpc(bool newState)
    {
        // サーバー側で正式に状態を切り替え
        playerStatus.Crouch.Value = newState;
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
    private void SubmitMoveServerRpc(Vector2 _moveInput,
                                     bool _crouchPressed,
                                     bool _runChangePressed,
                                     Vector3 worldDir)
    {
        this.moveInput               = _moveInput;
        playerStatus.MoveY           = _moveInput.y;
        playerStatus.MoveX           = _moveInput.x;
        this.CrouchPressed           = _crouchPressed;
        this.RunChangePressed        = _runChangePressed;
        moveDirection                = worldDir;
    }
}
