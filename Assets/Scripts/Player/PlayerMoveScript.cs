// PlayerMoveScript.cs
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

    // スピードアップ用
    private bool moveSpeedUpNow     = false;
    private float moveSpeedUpDuration;
    private float moveSpeedUpTimer  = 0f;

    private Vector2 moveInput;

    // ← 追加：前フレームの MoveState を覚えておく
    private PlayerStatus.PlayerMoveState prevMoveState;

    public override void OnNetworkSpawn()
    {
        if (IsOwner || IsServer)
        {
            playerStatus = this.gameObject.GetComponent<PlayerStatus>();
            anim = GetComponent<Animator>();
        }

        // アクション取得
        // サーバー起動時は WalkSpeed で初期化
        if (IsServer)
        {
            defaultRunSpeed = RunSpeed;
            defaultWalkSpeed = WalkSpeed;
            defaultCrouchSpeed = CrouchSpeed;
            characterController = GetComponent<CharacterController>();
            staminaController = GetComponent<PlayerStaminaController>();
            NowSpeed = WalkSpeed;
        }


        // ← 追加：初期の MoveState を保持

        if (IsOwner)
        {
            playerInput = playerStatus.player_input;
            subNoticedUI = playerStatus.sub_noticed_ui_controller;
            prevMoveState = playerStatus.player_move_state_network.Value;
            DirectionCamera = playerStatus.virtual_camera.VirtualCameraGameObject;
            moveAction = playerInput.actions["Move"];
            runChangeAction = playerInput.actions["RunChange"];
            crouchChangeAction = playerInput.actions["CrouchChange"];

            // ★ 必ず全 InputAction を Enable する ★
            moveAction.Enable();
            runChangeAction.Enable();
            crouchChangeAction.Enable();

            // しゃがみはPerformedでトグル
            crouchChangeAction.performed += OnCrouchPerformed;
            //歩き、走りの切り替えを行う。
            runChangeAction.started += OnRunChangePerformed;
            runChangeAction.canceled += OnRunChangePerformed;
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

            SubmitMoveServerRpc(moveInput, worldDir);

            // アニメ同期用の値はクライアント側でも即時更新
            anim.SetFloat("MoveY", moveInput.y);
            anim.SetFloat("MoveX", moveInput.x);
        }

        // ── サーバー権限で状態管理 ──
        if (IsServer && playerStatus.Dead.Value == false)
        {
            
            // ← 追加：しゃがみ解除直後の遷移検知
            var currentState = playerStatus.player_move_state_network.Value;
            var nowCrouch = playerStatus.Crouch;


            if (prevMoveState == PlayerStatus.PlayerMoveState.Crouch && !nowCrouch.Value)
            {
                // しゃがみ解除された瞬間だけ Walk に戻す
                playerStatus.player_move_state_network.Value = PlayerStatus.PlayerMoveState.Walk;
                NowSpeed = WalkSpeed;
                playerStatus.MoveSpeed = WalkSpeed;
            }

            // しゃがみ中はまず CrouchSpeed を優先
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
                    // ← 追加：Stop からの復帰を明示
                    if (currentState == PlayerStatus.PlayerMoveState.Stop)
                    {
                        playerStatus.player_move_state_network.Value = PlayerStatus.PlayerMoveState.Walk;
                        NowSpeed = WalkSpeed;
                        playerStatus.MoveSpeed = WalkSpeed;
                    }
                    else
                    {
                        // 入力アリ → Walk/Run 切り替え
                        HandleRunWalk();
                    }
                }
                else
                {
                    // 入力ナシ → 完全停止
                    playerStatus.player_move_state_network.Value = PlayerStatus.PlayerMoveState.Stop;
                    NowSpeed = 0f;
                    playerStatus.MoveSpeed = 0f;
                }
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
                    MoveSpeedUpFinishedClientRpc();
                    moveSpeedUpNow   = false;
                    moveSpeedUpTimer = 0f;
                }
            }

            // ← フレーム末に状態を更新
            prevMoveState = playerStatus.player_move_state_network.Value;
        }
        if (IsServer)
        {
            if (playerStatus.Dead.Value == true)
            {
                //ダウン状態なら
                playerStatus.player_move_state_network.Value = PlayerStatus.PlayerMoveState.Down;
                NowSpeed = CrouchSpeed;
                playerStatus.MoveSpeed = CrouchSpeed;
            }       
        }

        if (IsServer && MovePermit && characterController.enabled)
        {
            if (moveDirection != Vector3.zero)
                characterController.Move(moveDirection * NowSpeed * Time.deltaTime);
            else
                characterController.Move(Vector3.up * characterController.velocity.y * Time.deltaTime);
        }
    }

    // void FixedUpdate()
    // {
    //     // 空
    // }

    private void OnCrouchPerformed(InputAction.CallbackContext ctx)
    {
        if (!playerStatus.CrouchPermit.Value) return;
        playerStatus.CrouchStandCameraPosChange(!playerStatus.Crouch.Value);
        UpdateCrouchNetworkChangeServerRpc(!playerStatus.Crouch.Value);
    }
    private void OnRunChangePerformed(InputAction.CallbackContext ctx) {
        bool isRunPressed = ctx.ReadValue<float>() > 0f;
        UpdateRunStateServerRpc(isRunPressed);

    }
    [ClientRpc]
    private void MoveSpeedUpFinishedClientRpc()
    {
        if (!IsOwner) return;
        subNoticedUI.Notice("脚力が元に戻った");
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

    [ServerRpc]
    private void UpdateCrouchNetworkChangeServerRpc(bool newCrouch)
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
        moveInput            = _moveInput;
        playerStatus.MoveY   = _moveInput.y;
        playerStatus.MoveX   = _moveInput.x;
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
