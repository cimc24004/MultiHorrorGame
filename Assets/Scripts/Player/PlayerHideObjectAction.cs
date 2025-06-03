using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHideObjectAction : NetworkBehaviour
{
    //プレイヤーが隠れるアクション
    private TargetHideObject    target_hide_object;
    private PlayerStatus        player_status;
    private CharacterMovePermit charactor_move_permit;
    public  NetworkVariable<TargetHideObject.HideType> hide_type_target_object = new NetworkVariable<TargetHideObject.HideType>();
    private Animator            player_anim;
    private CharacterController player_character_controller;
    private PlayerInput         player_input;
    private PlayerMoveScript    player_move_script;
    private CrossHairStatus     cross_hair_status;
    public  NetworkVariable<bool> HidingPermit = new NetworkVariable<bool>(true);

    // Start is called before the first frame update
    void Start()
    {
        player_status               = this.gameObject.GetComponent<PlayerStatus>();
        cross_hair_status           = player_status.cross_hair_event.cross_hair_status;
        player_anim                 = this.gameObject.GetComponent<Animator>();
        player_character_controller = this.gameObject.GetComponent<CharacterController>();
        charactor_move_permit       = this.gameObject.GetComponent<CharacterMovePermit>();
        player_input                = player_status.player_input;
        player_move_script          = this.gameObject.GetComponent<PlayerMoveScript>();

        if (IsOwner)
        {
            player_input.actions["RaycastAction"]
                        .performed += PerfomedHideRaycastAction;
        }
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
        //     if (cross_hair_status.RaycastHitedExistsByCrossHairEvent 
        //         && cross_hair_status.HitedObj == this.gameObject)
        //     {
        //     }
        // }
        // if (player_status.HidingNow.Value == true)
        // {
        //     //隠れている状態でRaycastActionキーをおした場合
        //     if (player_input.actions["RaycastAction"].triggered)
        //     {
        //         HideUnHideRayCastActionPerformedRequestServerRpc();
        //     }
        // }
    }

    // ──────────────────────────────────
    // クライアント RPC：隠れる際の自分側処理
    // ──────────────────────────────────
    [ClientRpc]
    public void HideClientRpc()
    {
        PlayerDeformedByHideType();
        player_character_controller.enabled = false;
        this.gameObject.layer = LayerMask.NameToLayer("HidePlayer");
        //player_rigid.isKinematic = true;
        //player_collider.isTrigger = true;
        //player_character_controller.stepOffset = 0;
        //player_collider.isTrigger = true;
        if (IsOwner)
        {
            HidingNowChangeServerRpc(true);
        }
    }

    // ──────────────────────────────────
    // クライアント RPC：出る際の自分側処理
    // ──────────────────────────────────
    [ClientRpc]
    public void UnHideClientRpc()
    {
        //player_rigid.isKinematic = false;
        player_character_controller.enabled = true;
        //player_collider.isTrigger = false;
        player_status.HeadPos.position    = player_status.StandHeadPos.position;
        player_status.CameraRoot.position = player_status.StandHeadPos.position;
        player_anim.SetBool("FaceDown", false);
        player_anim.SetBool("Crouch",   false);
        this.gameObject.layer = LayerMask.NameToLayer("Player");
        charactor_move_permit.Move();
        if (IsOwner)
        {
            HidingNowChangeServerRpc(false);
        }
    }

    private void PlayerDeformedByHideType()
    {
        Debug.Log("PlayerDeformedByHideType---Start------");
        Debug.Log("OwnerId: " + OwnerClientId);
        Debug.Log("PlayerOwnerId: " + player_status.NetworkObject.OwnerClientId);
        Debug.Log("IsOwner: "+ IsOwner);
        Debug.Log("hide_type_target_object: "+ hide_type_target_object.Value);
        Debug.Log("--------------End---------");
        if (IsOwner)
        {
            PlayerDeformedByHideTypeRequestServerRpc();
            //HideType(うつぶせや座るや立つ)の値によってプレイヤーの状態を変える。
            if (hide_type_target_object.Value == TargetHideObject.HideType.Stand)
            {
                //立つタイプなら
                player_status.HeadPos.position = player_status.StandHeadPos.position;
                player_status.CameraRoot.position = player_status.StandHeadPos.position;
            }
            else if (hide_type_target_object.Value == TargetHideObject.HideType.Crouch)
            {
                //座るタイプなら
                player_status.HeadPos.position = player_status.CrouchHeadPos.position;
                player_status.CameraRoot.position = player_status.CrouchHeadPos.position;
            }
            else if (hide_type_target_object.Value == TargetHideObject.HideType.FaceDown)
            {
                //うつ伏せタイプならうつ伏せアニメーションを流す。
                player_status.HeadPos.position = player_status.FaceDownHeadPos.position;
                player_status.CameraRoot.position = player_status.FaceDownHeadPos.position;
                Debug.Log("FaceDownPlayerDeformedByHideType------Start----");
                Debug.Log("PlayerのPos----------------: " + player_status.NetworkObjectId);
                Debug.Log("FaceDownPlayerDeformedByHideType---End-------");
            }
        }

    }

    [ServerRpc]
    private void PlayerDeformedByHideTypeRequestServerRpc()
    {
        player_status.CrouchNetwork.Value = false;
        if (hide_type_target_object.Value == TargetHideObject.HideType.Crouch)
        {
            player_anim.SetBool("Crouch", true);
        }
        else if (hide_type_target_object.Value == TargetHideObject.HideType.FaceDown)
        {
            player_anim.SetBool("FaceDown", true);
        }
    }

    [ServerRpc]
    private void HidingNowChangeServerRpc(bool _HidingNow)
    {
        if (_HidingNow)
        {
            charactor_move_permit.Stop();
        }
        else
        {
            player_move_script.NowSpeed.Value = player_move_script.WalkSpeed;
            charactor_move_permit.Move();
        }
        player_status.HidingNow.Value = _HidingNow;
    }

    private void PerfomedHideRaycastAction(InputAction.CallbackContext ctx)
    {
        //HidingNowがtrueの場合だけ行う。 隠れてる最中に
        if (player_status.HidingNow.Value && HidingPermit.Value)
        {
            HideUnHideRayCastActionPerformedRequestServerRpc();
        }
    }

    public void GetCoughtWhenHidingByEnemy(GameObject _SightningEnemy)
    {
        //隠れている時 もし敵に隠れてることが知られていたら。
        Animator enemy_anim = _SightningEnemy.GetComponent<Animator>();
        if (hide_type_target_object.Value == TargetHideObject.HideType.FaceDown)
        {
            //机の下に隠れるタイプだった場合は下をのぞくアニメーションをつけてからHidingPermitをFalseにする。
            Debug.Log("FacedownnnnnnnNByEnemyyyy");
            enemy_anim.SetTrigger("LayLookDown");
            //↑アニメ終了後にDeadEventなど呼ぶ
        }
        else
        {
            if (player_status.HidingNow.Value && IsServer)
            {
                UnHideClientRpc();
            }
            player_status.DeadEvent(_SightningEnemy);
        }
    }

    // ──────────────────────────────────
    // サーバー RPC：隠れる／出る をまとめてリクエスト
    // ──────────────────────────────────
    [ServerRpc]
    public void HideUnHideRayCastActionPerformedRequestServerRpc(
        ulong _TargetNetworkObjectId = 0)
    {
        if (!HidingPermit.Value) return;

        ulong myId = this.NetworkObject.NetworkObjectId;

        if (!player_status.HidingNow.Value)
        {
            // 隠れる処理
            GameObject hideObjGO = NetworkManager.Singleton.SpawnManager
                                   .SpawnedObjects[_TargetNetworkObjectId].gameObject;
            TargetHideObjectSet(hideObjGO, player_status);
            target_hide_object.HideRequestServerRpc(myId);
            HideClientRpc();
            player_status.HidingNow.Value = true;
        }
        else
        {
            // 出る処理
            target_hide_object.UnHideRequestServerRpc(myId);
            UnHideClientRpc();
            player_status.HidingNow.Value = false;
        }
    }

    private void TargetHideObjectSet(GameObject _TargetObject, PlayerStatus _player_status)
    {
        //初期化みたいなとこです 隠れる対象オブジェクト(押入れとか机とか)を取得し、
        //そのHideTypeを取得。
        target_hide_object      = _TargetObject.GetComponent<TargetHideObject>();
        hide_type_target_object.Value = target_hide_object.hide_type;
    }
}
