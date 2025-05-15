using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System;
public class PlayerStatus : NetworkBehaviour
{
    //ローカルの人を取得するためのinstance
    public static PlayerStatus LocalInstance { get; private set; }

    public override void OnNetworkDespawn()
    {
        if (IsOwner && LocalInstance == this)
            LocalInstance = null;    // 切断されたらクリア
    }
    public static event Action<PlayerStatus> OnLocalPlayerSpawned;

    //ここは値の受け渡しやスタミナなどの管理。 プレイヤーの状態なども保存する
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
    //円ゲージUI画像
    public GameObject GageCanvas;
    public Image GageImage;
    //死んだかどうか
    public bool Dead;
    //プレイヤーの移動状態 (走る 歩く しゃがむ)
    public enum PlayerMoveState
    {
        Walk,
        Run,
        Crouch
        
    }
    public NetworkVariable<PlayerMoveState> player_move_state_network = new NetworkVariable<PlayerMoveState>();
    [Header("移動速度")]
    public float MoveSpeed;


    //今隠れているかどうか
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
    public NetworkVariable<bool> CrouchNetwork = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
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
    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {

        virtual_camera = CMFPSController.instance.GetComponent<CinemachineVirtualCamera>();
        Debug.Log("virtual_camera??: " + CMFPSController.instance);
        player_stamina_controller = this.gameObject.GetComponent<PlayerStaminaController>();
        player_continue_controller = GameController.instance.gameObject.GetComponent<PlayerContinueController>();
        player_character_controller = this.gameObject.GetComponent<CharacterController>();
        player_move_permit = this.gameObject.GetComponent<CharacterMovePermit>();
        anim = this.gameObject.GetComponent<Animator>();
        player_hide_object_action = this.gameObject.GetComponent<PlayerHideObjectAction>();
        item_belonging_controller = ItemController.GetComponent<ItemBelongingsController>();
        item_menu_controller = ItemController.GetComponent<ItemMenuController>();
        horror_director = HorrorDirector.Instance;
        flash_light_controller = FlashLightController.instance;
        player_to_enemy_distance_controller = this.gameObject.GetComponent<PlayerToEnemyDistanceController>();
        InitPlayer = true;
        player_move_script = this.gameObject.GetComponent<PlayerMoveScript>();
        this.CrouchNetwork.OnValueChanged += OnCrouchChanged;
        if (IsOwner){
            LocalInstance = this;    // 自分のクライアントのプレイヤーだけセット
            OnLocalPlayerSpawned?.Invoke(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!InitPlayer) return;
        if(IsServer){
            if(!IsOwner){
                anim.SetFloat("MoveY", this.MoveY);
                anim.SetFloat("MoveX", this.MoveX);
            }

            if(player_move_state_network.Value == PlayerMoveState.Crouch)
            {
                CrouchNetwork.Value = true;
            }
            else if(player_move_state_network.Value == PlayerMoveState.Run || player_move_state_network.Value == PlayerMoveState.Walk)
            {
                CrouchNetwork.Value = false;
            }
            if (player_move_state_network.Value == PlayerMoveState.Walk)
            {
                MoveX /= 2;
                MoveY /= 2;
            }
            if(MoveSpeed != 0)
            {
                if (player_move_state_network.Value == PlayerMoveState.Run)
                {
                    foot_step_sound_controller.move_status = FootStepSoundController.MoveStatus.Run;
                }
                else if (player_move_state_network.Value == PlayerMoveState.Walk)
                {
                    foot_step_sound_controller.move_status = FootStepSoundController.MoveStatus.Walk;
                }
                else
                {
                    foot_step_sound_controller.move_status = FootStepSoundController.MoveStatus.Crouch;
                }
            }
            else
            {
                foot_step_sound_controller.move_status = FootStepSoundController.MoveStatus.Stop;
            }
        }


    }
    private void FixedUpdate()
    {
        if(IsServer){
            if(MoveSpeed != 0)
            {
                if(player_move_state_network.Value == PlayerMoveState.Run)
                {
                    player_stamina_controller.ConsumeStamina();
                }
            }
        }

    }
    public void StopEvent()
    {
        //MoveSpeed=0でなかったら押し入れとかに隠れたときにずーと敵が待っているバグが生じる為。
        this.player_move_state_network.Value = PlayerMoveState.Walk;
        this.MoveX = 0;
        this.MoveY = 0;
        this.MoveSpeed = 0;
    }
    public void DeadEvent(GameObject KilledMeEnemy)
    {
        //プレイヤーが死ぬときのイベント。
        Debug.Log("あなたは死にました！");
        if (KilledMeEnemy.tag == "Enemy")
        {
            //緊急攻撃アイテムを所持していたら、
            //緊急攻撃アイテムで攻撃して敵をひるませる。
            EnemyStatus enemy_status = KilledMeEnemy.GetComponent<EnemyStatus>();
            EnemyFlinchController enemy_flinch_controller = enemy_status.enemy_flinch_controller;
            List<ItemStatus> item_statues = item_belonging_controller.ItemFindByItemType(ItemStatus.ItemType.EmergencyAttack);

            if (item_statues != null)
            {
                Debug.Log("悪霊退散おふだをはつどう！");
                ItemStatus item_status = item_statues[0];
                GameObject g = Instantiate(item_status.ItemThrowPrefab, enemy_status.EyePosition.transform.position, Quaternion.identity, enemy_status.EyePosition.transform);
                item_belonging_controller.ItemUseFromBelonging(item_status);
                sub_noticed_ui_controller.Notice($"{item_status.ItemName}を使用");
                Destroy(g, item_status.StopEnemyTime);
                g = null;
                enemy_flinch_controller.FlinchStart(item_status.StopEnemyTime);
                return;
            }

            player_hide_object_action.HidingPermit = false;
        }


        //恐怖演出用のイベントを実行。
        anim.SetBool("AppearentDeath", true);
        MoveX = 0;
        MoveY = 0;
        
        player_hide_object_action.HidingPermit = true;
        player_to_enemy_distance_controller.enabled = false;
        horror_director.nearEnemyCount = 0;
        horror_director.discoveredEnemyCount = 0;
        horror_director.ChangeHorrorState(HorrorState.PlayerKilled);
        CameraEnemyFaceExpanding camera_enemy_face_expanding = KilledMeEnemy.GetComponent<CameraEnemyFaceExpanding>();
        camera_enemy_face_expanding.FaceExpandingActive(this.gameObject);
        this.gameObject.layer = LayerMask.NameToLayer("DeadPlayer");
        PlayerDeaded = true;
        player_character_controller.enabled = false;
        player_move_permit.Stop();
        flash_light_controller.LightObject.SetActive(false);
        item_menu_controller.enabled = false;




        DOVirtual.DelayedCall(4, () =>
        {
            SceneManager.LoadScene("GameOver");
            PlayerDeaded = false;
            camera_enemy_face_expanding.FaceExpandingUnActive();
            player_continue_controller.Continue();
        });
    }
    
    private void OnCrouchChanged(bool oldValue, bool newValue)
    {
        // 状態更新
        // Crouch = newValue;
        // アニメーション更新

        this.player_move_state_network.Value = PlayerStatus.PlayerMoveState.Crouch;
        player_move_script.NowSpeed.Value = player_move_script.CrouchSpeed;
        anim.SetBool("Crouch", newValue);
        // 自分のカメラ位置も動かす
        if (IsOwner)
            CrouchStandCameraPosChange(newValue);
    }

    public void CrouchStandCameraPosChange(bool _Crouch)
    {
        Transform tgt = _Crouch ? CrouchHeadPos : StandHeadPos;

        crouchTween?.Kill();

        Vector3 srcLocal = CameraRoot.localPosition;   // 現在のローカル座標を保持
        float   targetY  = tgt.localPosition.y;        // 変えるのは Y だけ

        crouchTween = DOTween.To(
            () => CameraRoot.localPosition.y,
            y  => CameraRoot.localPosition = new Vector3(srcLocal.x, y, srcLocal.z),
            targetY,
            0.2f
        )
        .SetEase(Ease.OutQuad)
        .OnComplete(() =>
        {
            // XZ はそのまま、Y だけ確定
            CameraRoot.localPosition = new Vector3(srcLocal.x, targetY, srcLocal.z);
        });
    }
 

}
