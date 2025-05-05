using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerHideObjectAction : MonoBehaviour
{
    public bool HideAction;
    public bool HidingPermit = true;
    //プレイヤーが隠れるアクション
    private TargetHideObject target_hide_object;
    private PlayerStatus player_status;
    private CharacterMovePermit charactor_move_permit;
    public TargetHideObject.HideType hide_type_target_object;
    private Rigidbody player_rigid;
    private Animator player_anim;
    private Collider player_collider;
    private CharacterController player_character_controller;
    private PlayerInput player_input;
    private PlayerMoveScript player_move_script;
    private CrossHairStatus cross_hair_status;
    // Start is called before the first frame update
    void Start()
    {
        player_status = this.gameObject.GetComponent<PlayerStatus>();
        cross_hair_status = player_status.cross_hair_event.cross_hair_status;
        player_rigid = this.gameObject.GetComponent<Rigidbody>();
        player_collider = this.gameObject.GetComponent<Collider>();
        player_anim = this.gameObject.GetComponent<Animator>();
        player_character_controller = this.gameObject.GetComponent<CharacterController>();
        charactor_move_permit = this.gameObject.GetComponent<CharacterMovePermit>();
        player_input = GameControllerOnAllScene.instance.player_input;
        player_move_script = this.gameObject.GetComponent<PlayerMoveScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!HidingPermit)
        {
            HideAction = false;
            return;
        }
        if (cross_hair_status.HitedTag == "Hide"){

            if (cross_hair_status.RaycastHitedExistsByCrossHairEvent && cross_hair_status.HitedObj == this.gameObject){

            }
        }
        if (player_status.HidingNow)
        {
            //隠れている状態でRaycastActionキーをおした場合　隠れるオブジェクトから抜けるようにする。
            if (player_input.currentActionMap["RaycastAction"].triggered)
            {
                //target_hide_object = null;
                HideAction = true;
            }
        }
        //隠れるキーを押したら
        if (HideAction)
        {
            if (!player_status.HidingNow )
            {
                //ColHideObjectは隠れられるオブジェクトに触れてるかのチェックをしてから隠れる。
                Hide();
                //隠れている最中はプレイヤーの物理演算を無視してかつisTriggerも実質無効にする。
            }
            else if(player_status.HidingNow)
            {
                UnHide();
            }
            HideAction = false;
        }
    }
    //隠すときは隠れられるオブジェクトのTargetHideObjectスクリプトのHideTarget関数を実行すればよい。
    public void Hide()
    {
        player_character_controller.enabled = false;
        target_hide_object.HideTarget(this.gameObject);
        this.gameObject.layer = LayerMask.NameToLayer("HidePlayer");
        //player_rigid.isKinematic = true;
        //player_collider.isTrigger = true;
        //player_character_controller.stepOffset = 0;
        //player_collider.isTrigger = true;
        PlayerDeformedByHideType();
        charactor_move_permit.Stop();

        player_status.HidingNow = true;
    }
    public void UnHide()
    {
        target_hide_object.UnHideTarget(this.gameObject);
        //player_rigid.isKinematic = false;
        player_character_controller.enabled = true;
        //player_collider.isTrigger = false;
        player_status.HeadPos.position = player_status.StandHeadPos.position;
        player_status.CameraRoot.position = player_status.StandHeadPos.position;
        player_anim.SetBool("FaceDown", false);
        player_anim.SetBool("Crouch", false);
        this.gameObject.layer = LayerMask.NameToLayer("Player");
        charactor_move_permit.Move();

        player_move_script.NowSpeed = player_move_script.WalkSpeed;

        player_status.HidingNow = false;

    }
    private void PlayerDeformedByHideType()
    {
        player_move_script.CrouchNow = false;
        //HideType(うつぶせや座るや立つ)の値によってプレイヤーの状態を変える。
        if (hide_type_target_object == TargetHideObject.HideType.Stand)
        {
            //立つタイプなら
            player_status.HeadPos.position = player_status.StandHeadPos.position;
            player_status.CameraRoot.position = player_status.StandHeadPos.position;
        }
        else if(hide_type_target_object == TargetHideObject.HideType.Crouch)
        {
            //座るタイプなら
            player_status.HeadPos.position = player_status.CrouchHeadPos.position;
            player_status.CameraRoot.position = player_status.CrouchHeadPos.position;
            player_anim.SetBool("Crouch", true);
        }
        else if(hide_type_target_object == TargetHideObject.HideType.FaceDown)
        {
            //うつ伏せタイプならうつ伏せアニメーションを流す。
            player_status.HeadPos.position = player_status.FaceDownHeadPos.position;
            player_status.CameraRoot.position = player_status.FaceDownHeadPos.position;
            player_anim.SetBool("FaceDown", true);
        }
    }
    public void TargetHideObjectSet(GameObject TargetObject)
    {
        //CrossHairEvent.csから実行する。 隠れる対称のオブジェクト(おしいれとか机とか)を取得し、そのHideTypeを取得。
        target_hide_object = TargetObject.GetComponent<TargetHideObject>();
        hide_type_target_object = target_hide_object.hide_type;
    }
    
    public void GetCoughtWhenHidingByEnemy(GameObject _SightningEnemy)
    {
        //隠れている時 もし敵に隠れてることが知られていたら。
        Animator enemy_anim = _SightningEnemy.GetComponent<Animator>();
        //敵にバレているかつ近くにいる最中なら、そこからもう出られないようにする。

        if (hide_type_target_object == TargetHideObject.HideType.FaceDown)
        {
            //机の下に隠れるタイプだった場合は下をのぞくアニメーションをつけてからHidingPermitをFalseにする。
            Debug.Log("FacedownnnnnnnNByEnemyyyy");
            enemy_anim.SetTrigger("LayLookDown");
            //↑のアニメーションが終了したら、プレイヤー殺す関数 DeadEventを発生させる。
        }
        else
        {
            //押し入れに隠れるタイプとかロッカーに隠れるタイプだったらいきなり敵がドアを開けてくるアニメーション的なことをすればいい。
            if (player_status.HidingNow)
            {
                UnHide();
            }
            player_status.DeadEvent(_SightningEnemy);
        }
    }
}
