using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;
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

    public NoticedUIController noticed_ui_controller;
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
    public float MoveSpeed;
    //今隠れているかどうか
    public bool HidingNow;
    public float MoveX;
    public float MoveY;


    private float offsetY;
    private CharacterMovePermit player_move_permit;
    private Animator anim;
    private bool Crouch;
    public CinemachineVirtualCamera virtual_camera;

    public FootStepSoundController foot_step_sound_controller;
    private CinemachineTransposer cinemachine_transposer;
    // Start is called before the first frame update
    void Start()
    {
        player_move_permit = this.gameObject.GetComponent<CharacterMovePermit>();
        anim = this.gameObject.GetComponent<Animator>();
        cinemachine_transposer = virtual_camera.GetCinemachineComponent<CinemachineTransposer>();
        player_hide_object_action = this.gameObject.GetComponent<PlayerHideObjectAction>();

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
        //恐怖演出用のイベントを実行。
        anim.SetBool("AppearentDeath", true);
        MoveX = 0;
        MoveY = 0;
        
        player_hide_object_action.HidingPermit = true;

        CameraEnemyFaceExpanding camera_enemy_face_expanding = KilledMeEnemy.GetComponent<CameraEnemyFaceExpanding>();
        camera_enemy_face_expanding.FaceExpandingActive(this.gameObject);

        this.gameObject.layer = LayerMask.NameToLayer("DeadPlayer");
        player_move_permit.Stop();

    }
    public void CrouchStandCameraPosChange(bool Crouch)
    {
        if (Crouch)
        {
            offsetY = StandHeadPos.position.y;
            DOTween.To(
                () => offsetY,
                num => offsetY = num,
                CrouchHeadPos.position.y,
                0.2f
            ).SetEase(Ease.Linear).OnUpdate(() => {
                //cinemachine_transposer.m_FollowOffset = new Vector3(0, offsetY, 0);
                CameraRoot.transform.position = new Vector3(CameraRoot.transform.position.x, offsetY, CameraRoot.transform.position.z);
            }).OnComplete(() => {
                HeadPos.transform.position = CrouchHeadPos.transform.position;
                CameraRoot.transform.position = CrouchHeadPos.transform.position;
            });

        }
        else
        {

            DOTween.To(
                () => offsetY,
                num => offsetY = num,
                StandHeadPos.position.y,
                0.2f
            ).SetEase(Ease.Linear).OnUpdate(() => {
                CameraRoot.transform.position = new Vector3(CameraRoot.position.x, offsetY, CameraRoot.position.z);
            }).OnComplete(() => {
                HeadPos.transform.position = StandHeadPos.transform.position;
                CameraRoot.transform.position = StandHeadPos.transform.position;
            });
        }
    }
}
