using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using Sirenix.OdinInspector;
public class PlayerStatus : Singleton<PlayerStatus>
{
    //ここは値の受け渡しやスタミナなどの管理。 プレイヤーの状態なども保存する
    public Transform CameraRoot;
    public Transform HeadPos;
    public Transform CrouchHeadPos;
    public Transform StandHeadPos;
    public Transform FaceDownHeadPos;

    public CrossHairEvent cross_hair_event;
    public PlayerHideObjectAction player_hide_object_action;

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
    public PlayerMoveState player_move_state;
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


    public bool PlayerDeaded;


    private float offsetY;
    private CharacterMovePermit player_move_permit;
    private PlayerStaminaController player_stamina_controller;
    private Animator anim;
    private bool Crouch;
    public CinemachineVirtualCamera virtual_camera;
    public GameObject ItemController;
    public FootStepSoundController foot_step_sound_controller;
    private CinemachineTransposer cinemachine_transposer;
    private ItemMenuController item_menu_controller;
    private ItemBelongingsController item_belonging_controller;
    private PlayerContinueController player_continue_controller;
    private CharacterController player_character_controller;
    private HorrorDirector horror_director;
    private Tween crouchTween;
    private PlayerToEnemyDistanceController player_to_enemy_distance_controller;
    // Start is called before the first frame update
    void Start()
    {
        player_stamina_controller = this.gameObject.GetComponent<PlayerStaminaController>();
        player_continue_controller = GameController.instance.gameObject.GetComponent<PlayerContinueController>();
        player_character_controller = this.gameObject.GetComponent<CharacterController>();
        player_move_permit = this.gameObject.GetComponent<CharacterMovePermit>();
        anim = this.gameObject.GetComponent<Animator>();
        cinemachine_transposer = virtual_camera.GetCinemachineComponent<CinemachineTransposer>();
        player_hide_object_action = this.gameObject.GetComponent<PlayerHideObjectAction>();
        item_belonging_controller = ItemController.GetComponent<ItemBelongingsController>();
        item_menu_controller = ItemController.GetComponent<ItemMenuController>();
        horror_director = HorrorDirector.instance;
        player_to_enemy_distance_controller = this.gameObject.GetComponent<PlayerToEnemyDistanceController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(player_move_state == PlayerMoveState.Crouch)
        {
            Crouch = true;
        }
        else if((player_move_state == PlayerMoveState.Run || player_move_state == PlayerMoveState.Walk))
        {
            Crouch = false;
        }
        if (player_move_state == PlayerMoveState.Walk)
        {
            MoveX /= 2;
            MoveY /= 2;
        }
        if(MoveSpeed != 0)
        {
            if (player_move_state == PlayerMoveState.Run)
            {
                foot_step_sound_controller.move_status = FootStepSoundController.MoveStatus.Run;
            }
            else if (player_move_state == PlayerMoveState.Walk)
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
        anim.SetBool("Crouch", Crouch);
        anim.SetFloat("MoveY", MoveY);
        anim.SetFloat("MoveX", MoveX);
    }
    private void FixedUpdate()
    {
        if(MoveSpeed != 0)
        {
            if(player_move_state == PlayerMoveState.Run)
            {
                player_stamina_controller.ConsumeStamina();
            }
        }

    }
    public void StopEvent()
    {
        //MoveSpeed=0でなかったら押し入れとかに隠れたときにずーと敵が待っているバグが生じる為。
        this.player_move_state = PlayerMoveState.Walk;
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
        item_menu_controller.LightObject.SetActive(false);
        item_menu_controller.enabled = false;




        DOVirtual.DelayedCall(4, () =>
        {
            PlayerDeaded = false;
            camera_enemy_face_expanding.FaceExpandingUnActive();
            player_continue_controller.Continue();
        });
    }
    public void CrouchStandCameraPosChange(bool Crouch)
    {
        // ターゲット位置の設定
        Transform targetHeadPos = Crouch ? CrouchHeadPos : StandHeadPos;

        // 既存のアニメーションを停止
        crouchTween?.Kill();

        // アニメーション開始
        crouchTween = DOTween.To(
            () => CameraRoot.localPosition.y,
            y => CameraRoot.localPosition = new Vector3(CameraRoot.localPosition.x, y, CameraRoot.localPosition.z),
            targetHeadPos.localPosition.y,
            0.2f
        )
        .SetEase(Ease.OutQuad)
        .OnUpdate(() =>
        {
            // 必要であれば、更新中の処理をここに追加可能
        })
        .OnComplete(() =>
        {
            // 最終位置を正確に設定
            CameraRoot.localPosition = targetHeadPos.localPosition;
            HeadPos.position = targetHeadPos.position;
        });

    }
 

}
