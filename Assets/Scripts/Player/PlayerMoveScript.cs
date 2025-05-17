// PlayerMoveScript.cs
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMoveScript : NetworkBehaviour
{
    [Header("移動速度")]
    public float RunSpeed    = 3f;
    public float WalkSpeed   = 1f;
    public float CrouchSpeed = 1f;

    [Header("回転の滑らかさ（大きいほど速く追従）")]
    [SerializeField] private float rotationSmoothSpeed = 5f;

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
    private bool moveSpeedUpNow     = false;
    private float moveSpeedUpDuration;
    private float moveSpeedUpTimer  = 0f;

    private Vector2 moveInput;
    private bool CrouchNow;

    public override void OnNetworkSpawn()
    {
        characterController = GetComponent<CharacterController>();
        playerStatus        = GetComponent<PlayerStatus>();
        playerInput         = playerStatus.player_input;

        // アクション取得
        moveAction          = playerInput.actions["Move"];
        runChangeAction     = playerInput.actions["RunChange"];
        crouchChangeAction  = playerInput.actions["CrouchChange"];

        anim                = GetComponent<Animator>();
        defaultRunSpeed     = RunSpeed;
        defaultWalkSpeed    = WalkSpeed;
        defaultCrouchSpeed  = CrouchSpeed;

        // サーバー起動時は WalkSpeed で初期化
        if (IsServer)
            NowSpeed.Value = WalkSpeed;

        staminaController = GetComponent<PlayerStaminaController>();
        subNoticedUI      = playerStatus.sub_noticed_ui_controller;
        timeManager       = TimeManager.instance;
        DirectionCamera   = playerStatus.virtual_camera.VirtualCameraGameObject;

        if (IsOwner)
        {
            // ★ 必ず全 InputAction を Enable する ★
            moveAction.Enable();
            runChangeAction.Enable();
            crouchChangeAction.Enable();

            // しゃがみはPerformedでトグル
            crouchChangeAction.performed += OnCrouchPerformed;
        }
    }

    void Update()
    {
        if (!MovePermit && IsServer)
        {
            characterController.Move(Vector3.zero);
            return;
        }

        // ── クライアント（オーナー）入力部 ──
        if (IsOwner)
        {
            moveInput = moveAction.ReadValue<Vector2>();

            float yaw = DirectionCamera.transform.eulerAngles.y;
            Vector3 fwd = Quaternion.Euler(0, yaw, 0) * Vector3.forward;
            Vector3 rgt = Quaternion.Euler(0, yaw, 0) * Vector3.right;
            Vector3 worldDir = (fwd * moveInput.y + rgt * moveInput.x).normalized;

            // ★ RunChange は “押しっぱなし” 情報を毎フレ送る ★
            bool isRunHeld = runChangeAction.ReadValue<float>() > 0f;

            SubmitMoveServerRpc(moveInput, isRunHeld, worldDir);

            // アニメ同期用の値はクライアント側でも即時更新
            anim.SetFloat("MoveY", moveInput.y);
            anim.SetFloat("MoveX", moveInput.x);
        }

        // ── サーバー権限で状態管理 ──
        if (IsServer)
        {
            // しゃがみ中はまず CrouchSpeed を優先
            if (playerStatus.CrouchNetwork.Value)
            {
                playerStatus.player_move_state_network.Value = PlayerStatus.PlayerMoveState.Crouch;
                NowSpeed.Value = CrouchSpeed;
                playerStatus.MoveSpeed = CrouchSpeed;
            }
            else
            {
                // 移動入力アリ → Walk/Run へ戻す
                if (moveInput.sqrMagnitude > 0.01f)
                {
                    // Stop → Walk 復帰
                    if (playerStatus.player_move_state_network.Value == PlayerStatus.PlayerMoveState.Stop)
                    {
                        playerStatus.player_move_state_network.Value = PlayerStatus.PlayerMoveState.Walk;
                        NowSpeed.Value = WalkSpeed;
                    }
                    playerStatus.MoveSpeed = NowSpeed.Value;
                }
                else
                {
                    // 入力ナシ → 完全停止
                    playerStatus.player_move_state_network.Value = PlayerStatus.PlayerMoveState.Stop;
                    NowSpeed.Value = 0f;
                    playerStatus.MoveSpeed = 0f;
                }

                HandleRunWalk();
            }

            // スピードアップ終了タイマー
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
        if (IsServer && MovePermit && characterController.enabled)
        {
            if (moveDirection != Vector3.zero)
                characterController.Move(moveDirection * NowSpeed.Value * Time.fixedDeltaTime);
            else
                characterController.Move(Vector3.up * characterController.velocity.y * Time.fixedDeltaTime);
        }
    }

    private void OnCrouchPerformed(InputAction.CallbackContext ctx)
    {
        CrouchNow = !CrouchNow;
        anim.SetBool("Crouch", CrouchNow);
        playerStatus.CrouchStandCameraPosChange(CrouchNow);

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

    [ServerRpc]
    private void SubmitMoveServerRpc(
        Vector2 _moveInput,
        bool _runChangePressed,
        Vector3 worldDir
    )
    {
        moveInput            = _moveInput;
        playerStatus.MoveY   = _moveInput.y;
        playerStatus.MoveX   = _moveInput.x;
        runWalkTriggerChange = _runChangePressed;
        moveDirection        = worldDir;
    }

    private void HandleRunWalk()
    {
        var state = playerStatus.player_move_state_network.Value;
        if (state == PlayerStatus.PlayerMoveState.Run ||
            state == PlayerStatus.PlayerMoveState.Walk)
        {
            if (runWalkTriggerChange)
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
            else if (runChangeAction.WasReleasedThisFrame())
            {
                playerStatus.player_move_state_network.Value = PlayerStatus.PlayerMoveState.Walk;
                NowSpeed.Value = WalkSpeed;
                runWalkTriggerChange = false;
            }
        }
    }
}
