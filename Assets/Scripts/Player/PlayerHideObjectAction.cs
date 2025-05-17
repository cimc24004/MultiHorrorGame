using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerHideObjectAction : NetworkBehaviour
{
    //プレイヤーが隠れるアクション
    private TargetHideObject target_hide_object;
    private PlayerStatus player_status;
    private CharacterMovePermit charactor_move_permit;
    public TargetHideObject.HideType hide_type_target_object;
    private Animator player_anim;
    private CharacterController player_character_controller;
    private PlayerInput player_input;
    private PlayerMoveScript player_move_script;
    private CrossHairStatus cross_hair_status;
    public NetworkVariable<bool> HidingPermit = new NetworkVariable<bool>(true);
    // Start is called before the first frame update
    void Start()
    {
        player_status = this.gameObject.GetComponent<PlayerStatus>();
        cross_hair_status = player_status.cross_hair_event.cross_hair_status;
        player_anim = this.gameObject.GetComponent<Animator>();
        player_character_controller = this.gameObject.GetComponent<CharacterController>();
        charactor_move_permit = this.gameObject.GetComponent<CharacterMovePermit>();
        player_input = player_status.player_input;
        player_move_script = this.gameObject.GetComponent<PlayerMoveScript>();
        player_input.actions["RaycastAction"].performed += PerfomedHideRaycastAction;
    }

    // Update is called once per frame
    void Update()
    {
        if (!HidingPermit.Value)
        {
            return;
        }
        // if (cross_hair_status.HitedTag == "Hide")
        // {

        //     if (cross_hair_status.RaycastHitedExistsByCrossHairEvent && cross_hair_status.HitedObj == this.gameObject)
        //     {

        //     }
        // }
        // if (player_status.HidingNow.Value == true)
        // {
        //     //隠れている状態でRaycastActionキーをおした場合　隠れるオブジェクトから抜けるようにする。
        //     if (player_input.actions["RaycastAction"].triggered)
        //     {
        //         HideUnHideRayCastActionPerformedRequestServerRpc();
        //     }
        // }

    }

    //隠すときは隠れられるオブジェクトのTargetHideObjectスクリプトのHideTarget関数を実行すればよい。
    [ClientRpc]
    public void HideClientRpc(ulong _TargetNetworkObjectId)
    {
        player_character_controller.enabled = false;
        GameObject TargetObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[_TargetNetworkObjectId].gameObject;

        TargetHideObjectSet(TargetObject);
        target_hide_object.HideTarget(this.gameObject);
        this.gameObject.layer = LayerMask.NameToLayer("HidePlayer");
        //player_rigid.isKinematic = true;
        //player_collider.isTrigger = true;
        //player_character_controller.stepOffset = 0;
        //player_collider.isTrigger = true;
        PlayerDeformedByHideType();
        charactor_move_permit.Stop();

        player_status.HidingNow.Value = true;
    }
    [ClientRpc]
    public void UnHideClientRpc()
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
        if (IsServer)
        {
            player_move_script.NowSpeed.Value = player_move_script.WalkSpeed;
        }

        player_status.HidingNow.Value = false;

    }
    private void PlayerDeformedByHideType()
    {
        player_status.CrouchNetwork.Value = false;
        //HideType(うつぶせや座るや立つ)の値によってプレイヤーの状態を変える。
        if (hide_type_target_object == TargetHideObject.HideType.Stand)
        {
            //立つタイプなら
            player_status.HeadPos.position = player_status.StandHeadPos.position;
            player_status.CameraRoot.position = player_status.StandHeadPos.position;
        }
        else if (hide_type_target_object == TargetHideObject.HideType.Crouch)
        {
            //座るタイプなら
            player_status.HeadPos.position = player_status.CrouchHeadPos.position;
            player_status.CameraRoot.position = player_status.CrouchHeadPos.position;
            player_anim.SetBool("Crouch", true);
        }
        else if (hide_type_target_object == TargetHideObject.HideType.FaceDown)
        {
            //うつ伏せタイプならうつ伏せアニメーションを流す。
            player_status.HeadPos.position = player_status.FaceDownHeadPos.position;
            player_status.CameraRoot.position = player_status.FaceDownHeadPos.position;
            player_anim.SetBool("FaceDown", true);
        }
    }
    private void PerfomedHideRaycastAction(InputAction.CallbackContext ctx)
    {
        //HidingNowがtrueの場合だけ行う。 隠れてる最中に
        if (player_status.HidingNow.Value)
        {
            if (this.HidingPermit.Value == true)
            {
                HideUnHideRayCastActionPerformedRequestServerRpc();
            }
        }
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
            if (player_status.HidingNow.Value)
            {
                if (IsServer)
                {
                    UnHideClientRpc();
                }
            }
            player_status.DeadEvent(_SightningEnemy);
        }
    }
    [ServerRpc]
    public void HideUnHideRayCastActionPerformedRequestServerRpc(ulong _TargetNetworkObjectId = 0)
    {
        if (!HidingPermit.Value) return;
        //隠れるキーを押したら
        if (!player_status.HidingNow.Value)
        {
            //ColHideObjectは隠れられるオブジェクトに触れてるかのチェックをしてから隠れる。
            HideClientRpc(_TargetNetworkObjectId);
            //隠れている最中はプレイヤーの物理演算を無視してかつisTriggerも実質無効にする。
        }
        else if (player_status.HidingNow.Value)
        {
            UnHideClientRpc();
        }
    }
    
    private void TargetHideObjectSet(GameObject _TargetObject)
    {
        //初期化みたいなとこです 隠れる対称のオブジェクト(おしいれとか机とか)を取得し、そのHideTypeを取得。
        target_hide_object = _TargetObject.GetComponent<TargetHideObject>();
        hide_type_target_object = target_hide_object.hide_type;
    }

}
