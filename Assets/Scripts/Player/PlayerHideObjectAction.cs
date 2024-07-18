using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerHideObjectAction : MonoBehaviour
{
    //プレイヤーが隠れるアクション
    private PlayerInput player_input;
    private bool ColHideObject;
    private TargetHideObject target_hide_object;
    private PlayerStatus player_status;
    private CharactorMovePermit charactor_move_permit;
    private TargetHideObject.HideType hide_type_target_object;
    private Rigidbody player_rigid;
    private Animator player_anim;
    private Collider player_collider;
    // Start is called before the first frame update
    void Start()
    {
        player_status = this.gameObject.GetComponent<PlayerStatus>();
        player_input = this.gameObject.GetComponent<PlayerInput>();
        player_rigid = this.gameObject.GetComponent<Rigidbody>();
        player_collider = this.gameObject.GetComponent<Collider>();
        player_anim = this.gameObject.GetComponent<Animator>();
        charactor_move_permit = this.gameObject.GetComponent<CharactorMovePermit>();
    }

    // Update is called once per frame
    void Update()
    {
        //隠れるキーを押したら
        if (player_input.currentActionMap["HideAction"].triggered)
        {
            if (!player_status.HidingNow && ColHideObject)
            {
                //ColHideObjectは隠れられるオブジェクトに触れてるかのチェックをしてから隠れる。
                Hide();

                //隠れている最中はプレイヤーの物理演算を無視してかつisTriggerも実質無効にする。
                player_status.HidingNow = true;
            }
            else if(player_status.HidingNow)
            {
                UnHide();
                player_status.HidingNow = false;
            }
        }
    }
    //隠すときは隠れられるオブジェクトのTargetHideObjectスクリプトのHideTarget関数を実行すればよい。
    private void Hide()
    {
        target_hide_object.HideTarget(this.gameObject);
        player_rigid.isKinematic = true;
        player_collider.isTrigger = true;
        PlayerDeformedByHideType();
        charactor_move_permit.Stop();
    }
    private void UnHide()
    {
        target_hide_object.UnHideTarget(this.gameObject);
        player_rigid.isKinematic = false;
        player_collider.isTrigger = false;
        player_anim.SetBool("FaceDown", false);
        charactor_move_permit.Move();
    }
    private void PlayerDeformedByHideType()
    {
        //HideType(うつぶせや座るや立つ)の値によってプレイヤーの状態を変える。
        if(hide_type_target_object == TargetHideObject.HideType.Stand)
        {
            //立つタイプなら

        }else if(hide_type_target_object == TargetHideObject.HideType.Crouch)
        {
            //座るタイプなら

        }
        else if(hide_type_target_object == TargetHideObject.HideType.FaceDown)
        {
            //うつ伏せタイプならうつ伏せアニメーションを流す。
            player_anim.SetBool("FaceDown", true);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.tag == "HideActionObject")
        {
            target_hide_object = other.gameObject.GetComponent<TargetHideObject>();
            //隠れるプレイヤーの状態タイプを取得
            hide_type_target_object = target_hide_object.hide_type;
            ColHideObject = true;
        }
    }
    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.tag == "HideActionObject" && !player_status.HidingNow)
        {
            target_hide_object = null;
            ColHideObject = false;
        }
    }
}
