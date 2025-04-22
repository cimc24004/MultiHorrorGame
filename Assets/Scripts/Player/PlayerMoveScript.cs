using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMoveScript : MonoBehaviour
{
    // Inspectorで調整
    [Header("移動速度")]
    public float RunSpeed = 3f;
    public float WalkSpeed = 1f;
    public float CrouchSpeed = 1f;
    [Header("回転の滑らかさ（大きいほど速く追従）")]
    [SerializeField] private float rotationSmoothSpeed = 5f;
    [Header("カメラ参照")]
    public GameObject DirectionCamera;

    // 内部用
    private CharacterController characterController;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction runChangeAction;
    private InputAction crouchChangeAction;
    private Vector3 moveDirection = Vector3.zero;
    private Quaternion targetRotation;
    public float NowSpeed;
    private float defaultRunSpeed;
    private float defaultWalkSpeed;
    private float defaultCrouchSpeed;
    private bool runWalkTriggerChange = false;
    public bool CrouchNow = false;

    private PlayerStatus playerStatus;
    private PlayerStaminaController staminaController;
    private SubNoticedUIController subNoticedUI;
    private TimeManager timeManager;

    // 移動速度アップ用
    private bool moveSpeedUpNow = false;
    private float moveSpeedUpDuration;
    private float moveSpeedUpTimer = 0f;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerInput = GameControllerOnAllScene.instance.player_input;
        moveAction = playerInput.currentActionMap["Move"];
        runChangeAction = playerInput.currentActionMap["RunChange"];
        crouchChangeAction = playerInput.currentActionMap["CrouchChange"];

        defaultRunSpeed = RunSpeed;
        defaultWalkSpeed = WalkSpeed;
        defaultCrouchSpeed = CrouchSpeed;
        NowSpeed = WalkSpeed;

        playerStatus = PlayerStatus.instance;
        staminaController = GetComponent<PlayerStaminaController>();
        subNoticedUI = playerStatus.sub_noticed_ui_controller;
        timeManager = TimeManager.instance;
    }

    void Update()
    {
        // ── 入力取得＆方向・回転ターゲット設定 ──
        Vector2 input = moveAction.ReadValue<Vector2>();
        if (input.sqrMagnitude > 0.01f)
        {
            // カメラ基準の前・右ベクトル
            Quaternion camRot = Quaternion.Euler(0f, DirectionCamera.transform.eulerAngles.y, 0f);
            Vector3 forward = camRot * Vector3.forward;
            Vector3 right   = camRot * Vector3.right;

            // 移動方向
            moveDirection = (forward * input.y + right * input.x).normalized;
            // 回転ターゲット
            targetRotation = Quaternion.LookRotation(new Vector3(moveDirection.x, 0f, moveDirection.z));
            playerStatus.MoveSpeed = NowSpeed;
        }
        else
        {
            moveDirection = Vector3.zero;
            playerStatus.MoveSpeed = 0f;
        }

        // ── 走る／歩く切り替え処理 ──
        HandleRunWalk();

        // ── しゃがみ切り替え処理 ──
        HandleCrouch();

        // ── スピードアップタイマー更新 ──
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

    void FixedUpdate()
    {
        // ── 実際の移動 ──
        if (moveDirection != Vector3.zero)
        {
            characterController.Move(moveDirection * NowSpeed * Time.fixedDeltaTime);
        }
        else
        {
            // Y方向の慣性（必要なら重力に置き換えてOK）
            characterController.Move(
                Vector3.Scale(Vector3.up, characterController.velocity) 
                * Time.fixedDeltaTime
            );
        }
    }

    void LateUpdate()
    {
        // ── 滑らか回転 ──
        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSmoothSpeed * Time.deltaTime
            );
        }
    }

    private void HandleRunWalk()
    {
        if (playerStatus.player_move_state == PlayerStatus.PlayerMoveState.Run ||
            playerStatus.player_move_state == PlayerStatus.PlayerMoveState.Walk)
        {
            if (runChangeAction.triggered)
            {
                // 押した瞬間にスタミナチェック
                runWalkTriggerChange = staminaController.StaminaCheck();
            }

            if (runWalkTriggerChange && runChangeAction.IsPressed())
            {
                // 長押し中
                if (!staminaController.StaminaIncreaseNow)
                {
                    if (playerStatus.StaminaValueNow > 0)
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
                else
                {
                    if (staminaController.StaminaCheck())
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
        if (crouchChangeAction.triggered)
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

    // 外部から呼べるスピードアップ
    public void MoveSpeedUp(float addSpeed, float effectTime)
    {
        RunSpeed    += addSpeed;
        WalkSpeed   += addSpeed * 0.5f;
        CrouchSpeed += addSpeed * 0.25f;
        moveSpeedUpDuration = effectTime;
        moveSpeedUpNow      = true;
        moveSpeedUpTimer    = 0f;
    }

    // 効果終了時の通知
    private void ResetMoveSpeed()
    {
        subNoticedUI.Notice("脚力が元に戻った");
    }
}
