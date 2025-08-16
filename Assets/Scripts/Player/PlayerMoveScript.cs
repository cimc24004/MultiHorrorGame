using System;
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

    public float NowSpeed = 0;
    public bool MovePermit = true;

    private float defaultRunSpeed;
    private float defaultWalkSpeed;
    private float defaultCrouchSpeed;
    private bool runWalkTriggerChange = false;

    [SerializeField] private PlayerStatus playerStatus;
    private PlayerStaminaController staminaController;
    private SubNoticedUIController subNoticedUI;

    private bool moveSpeedUpNow     = false;
    private float moveSpeedUpDuration;
    private float moveSpeedUpTimer  = 0f;

    private Vector2 moveInput;

    // 前フレームの MoveState
    private PlayerStatus.PlayerMoveState prevMoveState;

    public override void OnNetworkSpawn()
    {
        if (IsOwner || IsServer)
        {
            playerStatus = this.gameObject.GetComponent<PlayerStatus>();
            anim = GetComponent<Animator>();
        }

        if (IsServer)
        {
            defaultRunSpeed = RunSpeed;
            defaultWalkSpeed = WalkSpeed;
            defaultCrouchSpeed = CrouchSpeed;
            staminaController = GetComponent<PlayerStaminaController>();
            NowSpeed = WalkSpeed;
        }

        // ★ Owner側の物理移動はオーナーが担当
        if (IsOwner)
        {
            characterController = GetComponent<CharacterController>();
            playerInput = playerStatus.player_input;
            subNoticedUI = playerStatus.sub_noticed_ui_controller;
            prevMoveState = playerStatus.player_move_state_network.Value;
            DirectionCamera = playerStatus.virtual_camera.VirtualCameraGameObject;

            moveAction         = playerInput.actions["Move"];
            runChangeAction    = playerInput.actions["RunChange"];
            crouchChangeAction = playerInput.actions["CrouchChange"];

            moveAction.Enable();
            runChangeAction.Enable();
            crouchChangeAction.Enable();

            crouchChangeAction.performed += OnCrouchPerformed;
            runChangeAction.started  += OnRunChangePerformed;
            runChangeAction.canceled += OnRunChangePerformed;
        }
    }

    void Update()
    {
        // ───────────────────────────────────────────
        // クライアント（オーナー）：入力と実移動を担当
        // ───────────────────────────────────────────
        if (IsOwner)
        {
            // 入力からワールド方向を作る
            moveInput = moveAction.ReadValue<Vector2>();

            float yaw = DirectionCamera.transform.eulerAngles.y;
            Vector3 fwd = Quaternion.Euler(0, yaw, 0) * Vector3.forward;
            Vector3 rgt = Quaternion.Euler(0, yaw, 0) * Vector3.right;
            Vector3 worldDir = (fwd * moveInput.y + rgt * moveInput.x).normalized;

            // サーバーには入力と方向を送って「状態判定」だけしてもらう
            SubmitMoveServerRpc(moveInput, worldDir);

            // 自分の移動用にも保持
            moveDirection = worldDir;

            // アニメ同期用（ローカル即時）
            anim.SetFloat("MoveY", moveInput.y);
            anim.SetFloat("MoveX", moveInput.x);

            // サーバーが決めた MoveState を参照してローカル速度を決定
            var state = playerStatus.player_move_state_network.Value;
            switch (state)
            {
                case PlayerStatus.PlayerMoveState.Run:    NowSpeed = RunSpeed;    break;
                case PlayerStatus.PlayerMoveState.Crouch: NowSpeed = CrouchSpeed; break;
                case PlayerStatus.PlayerMoveState.Walk:   NowSpeed = WalkSpeed;   break;
                case PlayerStatus.PlayerMoveState.Down:   NowSpeed = CrouchSpeed; break;
                default:                                  NowSpeed = 0f;          break;
            }

            // 移動許可が無い/CC無効なら動かさない
            if (!MovePermit || characterController == null || !characterController.enabled)
                goto ServerState;

            if (moveDirection != Vector3.zero)
                characterController.Move(moveDirection * NowSpeed * Time.deltaTime);
            else
                characterController.Move(Vector3.up * characterController.velocity.y * Time.deltaTime);
        }

        // ───────────────────────────────────────────
        // サーバー：状態（歩き/走り/しゃがみ等）だけ決める
        // ───────────────────────────────────────────
        ServerState:
        if (IsServer && playerStatus.Dead.Value == false)
        {
            var currentState = playerStatus.player_move_state_network.Value;
            var nowCrouch = playerStatus.Crouch;

            if (prevMoveState == PlayerStatus.PlayerMoveState.Crouch && !nowCrouch.Value)
            {
                playerStatus.player_move_state_network.Value = PlayerStatus.PlayerMoveState.Walk;
                NowSpeed = WalkSpeed;
                playerStatus.MoveSpeed = WalkSpeed;
            }

            if (playerStatus.Crouch.Value)
            {
                playerStatus.player_move_state_network.Value = PlayerStatus.PlayerMoveState.Crouch;
                NowSpeed = CrouchSpeed;
                playerStatus.MoveSpeed = CrouchSpeed;
            }
            else
            {
                if (moveInput.sqrMagnitude > 0.01f)
                {
                    if (currentState == PlayerStatus.PlayerMoveState.Stop)
                    {
                        playerStatus.player_move_state_network.Value = PlayerStatus.PlayerMoveState.Walk;
                        NowSpeed = WalkSpeed;
                        playerStatus.MoveSpeed = WalkSpeed;
                    }
                    else
                    {
                        HandleRunWalk();
                    }
                }
                else
                {
                    playerStatus.player_move_state_network.Value = PlayerStatus.PlayerMoveState.Stop;
                    NowSpeed = 0f;
                    playerStatus.MoveSpeed = 0f;
                }
            }

            if (moveSpeedUpNow)
            {
                moveSpeedUpTimer += Time.deltaTime;
                if (moveSpeedUpTimer >= moveSpeedUpDuration)
                {
                    RunSpeed    = defaultRunSpeed;
                    WalkSpeed   = defaultWalkSpeed;
                    CrouchSpeed = defaultCrouchSpeed;
                    MoveSpeedUpFinishedClientRpc();
                    moveSpeedUpNow   = false;
                    moveSpeedUpTimer = 0f;
                }
            }

            prevMoveState = playerStatus.player_move_state_network.Value;
        }

        if (IsServer)
        {
            if (playerStatus.Dead.Value == true)
            {
                playerStatus.player_move_state_network.Value = PlayerStatus.PlayerMoveState.Down;
                NowSpeed = CrouchSpeed;
                playerStatus.MoveSpeed = CrouchSpeed;
            }
        }
    }

    private void OnCrouchPerformed(InputAction.CallbackContext ctx)
    {
        if (!playerStatus.CrouchPermit.Value) return;
        playerStatus.CrouchStandCameraPosChange(!playerStatus.Crouch.Value);
        UpdateCrouchNetworkChangeServerRpc(!playerStatus.Crouch.Value);
    }

    private void OnRunChangePerformed(InputAction.CallbackContext ctx)
    {
        bool isRunPressed = ctx.ReadValue<float>() > 0f;
        UpdateRunStateServerRpc(isRunPressed);
    }

    [ClientRpc]
    private void MoveSpeedUpFinishedClientRpc()
    {
        if (!IsOwner) return;
        subNoticedUI?.Notice("脚力が元に戻った");
    }

    [ServerRpc]
    public void MoveSpeedUpRequestServerRpc(float addSpeed, float effectTime)
    {
        RunSpeed += addSpeed;
        WalkSpeed += addSpeed * 0.5f;
        CrouchSpeed += addSpeed * 0.25f;
        moveSpeedUpDuration = effectTime;
        moveSpeedUpNow = true;
        moveSpeedUpTimer = 0f;
    }

    [ServerRpc(RequireOwnership=false)]
    public void UpdateCrouchNetworkChangeServerRpc(bool newCrouch)
    {
        if (!playerStatus.CrouchPermit.Value) return;
        anim.SetBool("Crouch", newCrouch);
        playerStatus.Crouch.Value = newCrouch;
    }

    [ServerRpc]
    private void UpdateRunStateServerRpc(bool isRun)
    {
        runWalkTriggerChange = isRun;
    }

    [ServerRpc]
    private void SubmitMoveServerRpc(
        Vector2 _moveInput,
        Vector3 worldDir
    )
    {
        // サーバーは「状態判定用」にだけ使う
        moveInput            = _moveInput;
        playerStatus.MoveY   = _moveInput.y;
        playerStatus.MoveX   = _moveInput.x;
        // moveDirection はサーバーでは使用しない（物理移動はクライアントが担当）
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
                    : (playerStatus.StaminaValueNow.Value > 0);

                if (canRun)
                {
                    playerStatus.player_move_state_network.Value = PlayerStatus.PlayerMoveState.Run;
                    NowSpeed = RunSpeed;
                }
                else
                {
                    playerStatus.player_move_state_network.Value = PlayerStatus.PlayerMoveState.Walk;
                    NowSpeed = WalkSpeed;
                }
            }
            else if (!runWalkTriggerChange)
            {
                playerStatus.player_move_state_network.Value = PlayerStatus.PlayerMoveState.Walk;
                NowSpeed = WalkSpeed;
                runWalkTriggerChange = false;
            }

            playerStatus.MoveSpeed = NowSpeed;
        }
    }
}
