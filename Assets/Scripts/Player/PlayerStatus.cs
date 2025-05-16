// PlayerStatus.cs
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Cinemachine;
using DG.Tweening;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PlayerStatus : NetworkBehaviour
{
    // ローカルの人を取得するためのinstance
    public static PlayerStatus LocalInstance { get; private set; }
    public static event Action<PlayerStatus> OnLocalPlayerSpawned;

    // 値の受け渡しやスタミナなどの管理。プレイヤーの状態などを保存
    public Transform CameraRoot;
    public Transform HeadPos;
    public Transform CrouchHeadPos;
    public Transform StandHeadPos;
    public Transform FaceDownHeadPos;
    public CrossHairEvent cross_hair_event;
    public PlayerHideObjectAction player_hide_object_action;
    public PlayerInput player_input;
    public SubNoticedUIController sub_noticed_ui_controller;
    public MainNoticedUIController main_noticed_ui_controller;
    public GameObject GageCanvas;
    public Image GageImage;
    public bool Dead;

    // プレイヤーの移動状態 (走る 歩く しゃがむ)
    public enum PlayerMoveState { Walk, Run, Crouch }
    public NetworkVariable<PlayerMoveState> player_move_state_network
        = new NetworkVariable<PlayerMoveState>(PlayerMoveState.Walk);

    [Header("移動速度")]
    public float MoveSpeed;

    public bool HidingNow;
    public float MoveX;
    public float MoveY;
    [Header("スタミナコスト")]
    public float StaminaCost = 0.3f;
    [Header("現在のスタミナ値")]
    public float StaminaValueNow = 100f;
    [Header("スタミナ1fにつきの回復力")]
    public float StaminaRetakePower;
    [Header("走れる最小スタミナ")]
    public float CanMinStaminaRun = 30;

    public AudioDatas HideActionAudioDatas;
    public bool PlayerDeaded;

    [SerializeField] private GameObject FlashLightBatteryUI;
    private float offsetY;
    private CharacterMovePermit player_move_permit;
    private PlayerStaminaController player_stamina_controller;
    private PlayerMoveScript player_move_script;
    private Animator anim;
    private bool InitPlayer;
    public bool Crouch;
    public CinemachineVirtualCamera virtual_camera;
    public GameObject ItemController;
    public FootStepSoundController foot_step_sound_controller;
    private ItemMenuController item_menu_controller;
    private ItemBelongingsController item_belonging_controller;
    private PlayerContinueController player_continue_controller;
    private CharacterController player_character_controller;
    private HorrorDirector horror_director;
    private Tween crouchTween;
    private PlayerToEnemyDistanceController player_to_enemy_distance_controller;
    private FlashLightController flash_light_controller;

    // しゃがみ状態同期用 NetworkVariable（オーナー書き込み）
    public NetworkVariable<bool> CrouchNetwork = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkDespawn()
    {
        if (IsOwner && LocalInstance == this)
            LocalInstance = null;
    }

    public override void OnNetworkSpawn()
    {
        virtual_camera                  = CMFPSController.instance.GetComponent<CinemachineVirtualCamera>();
        player_stamina_controller       = GetComponent<PlayerStaminaController>();
        player_continue_controller      = GameController.instance.GetComponent<PlayerContinueController>();
        player_character_controller     = GetComponent<CharacterController>();
        player_move_permit              = GetComponent<CharacterMovePermit>();
        anim                            = GetComponent<Animator>();
        player_hide_object_action       = GetComponent<PlayerHideObjectAction>();
        item_belonging_controller       = ItemController.GetComponent<ItemBelongingsController>();
        item_menu_controller            = ItemController.GetComponent<ItemMenuController>();
        horror_director                 = HorrorDirector.Instance;
        flash_light_controller          = FlashLightController.instance;
        player_to_enemy_distance_controller = GetComponent<PlayerToEnemyDistanceController>();

        player_move_script = GetComponent<PlayerMoveScript>();

        InitPlayer = true;
        if (IsServer)
        {
            CrouchNetwork.OnValueChanged += OnCrouchChanged;
            
        }

        if (IsOwner)
        {
            LocalInstance = this;
            OnLocalPlayerSpawned?.Invoke(this);
        }
    }

    void Update()
    {
        if (!InitPlayer) return;
        if (IsServer)
        {
            if (!IsOwner)
            {
                anim.SetFloat("MoveY", MoveY);
                anim.SetFloat("MoveX", MoveX);
            }

            // 足音ステータス更新
            if (MoveSpeed != 0)
            {
                if (player_move_state_network.Value == PlayerMoveState.Run)
                    foot_step_sound_controller.move_status = FootStepSoundController.MoveStatus.Run;
                else if (player_move_state_network.Value == PlayerMoveState.Walk)
                    foot_step_sound_controller.move_status = FootStepSoundController.MoveStatus.Walk;
                else
                    foot_step_sound_controller.move_status = FootStepSoundController.MoveStatus.Crouch;
            }
            else
            {
                foot_step_sound_controller.move_status = FootStepSoundController.MoveStatus.Stop;
            }
        }
    }

    void FixedUpdate()
    {
        if (IsServer && MoveSpeed != 0 && player_move_state_network.Value == PlayerMoveState.Run)
        {
            player_stamina_controller.ConsumeStamina();
        }
    }

    public void StopEvent()
    {
        player_move_state_network.Value = PlayerMoveState.Walk;
        MoveX = 0;
        MoveY = 0;
        MoveSpeed = 0;
    }

    public void DeadEvent(GameObject KilledMeEnemy)
    {
        Debug.Log("あなたは死にました！");
        if (KilledMeEnemy.tag == "Enemy")
        {
            EnemyStatus enemy_status = KilledMeEnemy.GetComponent<EnemyStatus>();
            EnemyFlinchController flinch = enemy_status.enemy_flinch_controller;
            var items = item_belonging_controller.ItemFindByItemType(ItemStatus.ItemType.EmergencyAttack);
            if (items != null)
            {
                var item = items[0];
                GameObject g = Instantiate(item.ItemThrowPrefab, enemy_status.EyePosition.transform.position, Quaternion.identity, enemy_status.EyePosition.transform);
                item_belonging_controller.ItemUseFromBelonging(item);
                sub_noticed_ui_controller.Notice($"{item.ItemName}を使用");
                Destroy(g, item.StopEnemyTime);
                flinch.FlinchStart(item.StopEnemyTime);
                return;
            }
            player_hide_object_action.HidingPermit = false;
        }
        anim.SetBool("AppearentDeath", true);
        MoveX = MoveY = 0;
        player_hide_object_action.HidingPermit = true;
        player_to_enemy_distance_controller.enabled = false;
        horror_director.nearEnemyCount = horror_director.discoveredEnemyCount = 0;
        horror_director.ChangeHorrorState(HorrorState.PlayerKilled);
        var expand = KilledMeEnemy.GetComponent<CameraEnemyFaceExpanding>();
        expand.FaceExpandingActive(gameObject);
        gameObject.layer = LayerMask.NameToLayer("DeadPlayer");
        PlayerDeaded = true;
        player_character_controller.enabled = false;
        player_move_permit.Stop();
        flash_light_controller.LightObject.SetActive(false);
        item_menu_controller.enabled = false;

        DOVirtual.DelayedCall(4, () =>
        {
            SceneManager.LoadScene("GameOver");
            PlayerDeaded = false;
            expand.FaceExpandingUnActive();
            player_continue_controller.Continue();
        });
    }

    private void OnCrouchChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            player_move_state_network.Value = PlayerMoveState.Crouch;
            player_move_script.NowSpeed.Value = player_move_script.CrouchSpeed;
        }
        else
        {
            player_move_state_network.Value = PlayerMoveState.Walk;
            player_move_script.NowSpeed.Value = player_move_script.WalkSpeed;
        }
        anim.SetBool("Crouch", newValue);
        if (IsOwner)
            CrouchStandCameraPosChange(newValue);
    }

    public void CrouchStandCameraPosChange(bool _Crouch)
    {
        Transform tgt = _Crouch ? CrouchHeadPos : StandHeadPos;
        crouchTween?.Kill();
        Vector3 src = CameraRoot.localPosition;
        float tgtY = tgt.localPosition.y;
        crouchTween = DOTween.To(
            () => src.y,
            y => CameraRoot.localPosition = new Vector3(src.x, y, src.z),
            tgtY,
            0.2f
        ).SetEase(Ease.OutQuad);
    }
}
