// PlayerMoveScript.cs
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMoveScript : NetworkBehaviour
{
    [Header("移動速度")]
    public float RunSpeed = 3f;
    public float WalkSpeed = 1f;
    public float CrouchSpeed = 1f;
    [Header("回転の滑らかさ（大きいほど速く追従）")]
    [SerializeField] private float rotationSmoothSpeed = 5f;
    [Header("カメラ参照")]
    private GameObject DirectionCamera;

    private CharacterController characterController;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction runChangeAction;
    private InputAction crouchChangeAction;
    private Animator anim;
    private Vector3 moveDirection = Vector3.zero;

    public NetworkVariable<float> NowSpeed = new NetworkVariable<float>();
    public bool MovePermit = true;
    private float defaultRunSpeed;
    private float defaultWalkSpeed;
    private float defaultCrouchSpeed;
    private bool runWalkTriggerChange = false;

    [SerializeField] private PlayerStatus playerStatus;
    private PlayerStaminaController staminaController;
    private SubNoticedUIController subNoticedUI;
    private TimeManager timeManager;

    // スピードアップ用
    private bool moveSpeedUpNow = false;
    private float moveSpeedUpDuration;
    private float moveSpeedUpTimer = 0f;

    private Vector2 moveInput;
    private bool CrouchNow;

    public override void OnNetworkSpawn()
    {
        characterController = GetComponent<CharacterController>();
        playerStatus        = GetComponent<PlayerStatus>();
        playerInput         = playerStatus.player_input;
        moveAction          = playerInput.actions["Move"];
        runChangeAction     = playerInput.actions["RunChange"];
        crouchChangeAction  = playerInput.actions["CrouchChange"];
        anim                = GetComponent<Animator>();
        defaultRunSpeed     = RunSpeed;
        defaultWalkSpeed    = WalkSpeed;
        defaultCrouchSpeed  = CrouchSpeed;
        if (IsServer)
        {
            NowSpeed.Value      = WalkSpeed;
        }

        staminaController = GetComponent<PlayerStaminaController>();
        subNoticedUI      = playerStatus.sub_noticed_ui_controller;
        timeManager       = TimeManager.instance;
        DirectionCamera   = playerStatus.virtual_camera.VirtualCameraGameObject;

        if (IsOwner)
        {
            crouchChangeAction.performed += OnCrouchPerformed;
            crouchChangeAction.Enable();
        }
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

            SubmitMoveServerRpc(moveInput, runChangeAction.triggered, worldDir);

            anim.SetFloat("MoveY", moveInput.y);
            anim.SetFloat("MoveX", moveInput.x);
        }

        if (IsServer)
        {
            if (!IsOwner)
                anim.SetBool("Crouch", playerStatus.CrouchNetwork.Value);

            if (moveInput.sqrMagnitude > 0.01f)
                playerStatus.MoveSpeed = NowSpeed.Value;
            else
                playerStatus.MoveSpeed = 0f;

            HandleRunWalk();

            if (moveSpeedUpNow)
            {
                moveSpeedUpTimer += Time.deltaTime;
                if (moveSpeedUpTimer >= moveSpeedUpDuration)
                {
                    RunSpeed    = defaultRunSpeed;
                    WalkSpeed   = defaultWalkSpeed;
                    CrouchSpeed = defaultCrouchSpeed;
                    subNoticedUI.Notice("脚力が元に戻った");
                    moveSpeedUpNow   = false;
                    moveSpeedUpTimer = 0f;
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (IsServer && MovePermit)
        {
            if (moveDirection != Vector3.zero)
                characterController.Move(moveDirection * NowSpeed.Value * Time.fixedDeltaTime);
            else
                characterController.Move(Vector3.up * characterController.velocity.y * Time.fixedDeltaTime);
        }
    }

    private void OnCrouchPerformed(InputAction.CallbackContext ctx)
    {
        // オーナーローカルで即時反映
        CrouchNow = !CrouchNow;
        anim.SetBool("Crouch", CrouchNow);
        playerStatus.CrouchStandCameraPosChange(CrouchNow);

        // サーバーへ送信して同期
        UpdateCrouchNetworkChangeServerRpc(CrouchNow);
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
    [ServerRpc]
    private void UpdateCrouchNetworkChangeServerRpc(bool newCrouch)
    {
        playerStatus.CrouchNetwork.Value = newCrouch;
    }

    private void HandleRunWalk()
    {
        if (playerStatus.player_move_state_network.Value == PlayerStatus.PlayerMoveState.Run ||
            playerStatus.player_move_state_network.Value == PlayerStatus.PlayerMoveState.Walk)
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
                    playerStatus.player_move_state_network.Value = PlayerStatus.PlayerMoveState.Run;
                    NowSpeed.Value = RunSpeed;
                }
                else
                {
                    playerStatus.player_move_state_network.Value = PlayerStatus.PlayerMoveState.Walk;
                    NowSpeed.Value = WalkSpeed;
                }
            }

            if (runChangeAction.WasReleasedThisFrame())
            {
                playerStatus.player_move_state_network.Value = PlayerStatus.PlayerMoveState.Walk;
                NowSpeed.Value = WalkSpeed;
                runWalkTriggerChange = false;
            }
        }
    }

    [ServerRpc]
    private void SubmitMoveServerRpc(Vector2 _moveInput, bool _runChangePressed, Vector3 worldDir)
    {
        moveInput             = _moveInput;
        playerStatus.MoveY    = _moveInput.y;
        playerStatus.MoveX    = _moveInput.x;
        runWalkTriggerChange  = _runChangePressed;
        moveDirection         = worldDir;
    }
}
