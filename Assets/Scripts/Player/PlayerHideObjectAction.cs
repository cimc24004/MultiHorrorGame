using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHideObjectAction : NetworkBehaviour
{
    //プレイヤーが隠れるアクション
    private TargetHideObject target_hide_object; // 隠れる対象オブジェクトの参照
    private PlayerStatus player_status;
    private CharacterMovePermit charactor_move_permit;
    public NetworkVariable<TargetHideObject.HideType> hide_type_target_object = new NetworkVariable<TargetHideObject.HideType>();
    private Animator player_anim;
    private CharacterController player_character_controller;
    private PlayerInput player_input;
    private PlayerMoveScript player_move_script;
    private CrossHairEvent cross_hair_event; // CrossHairEvent を使う場合

    public NetworkVariable<bool> HidingPermit = new NetworkVariable<bool>(true);
    public bool layLookDownTriggered = false;

    public override void OnNetworkSpawn()
    {
        player_status = this.gameObject.GetComponent<PlayerStatus>();
        player_anim = this.gameObject.GetComponent<Animator>();
        player_character_controller = this.gameObject.GetComponent<CharacterController>();
        charactor_move_permit = this.gameObject.GetComponent<CharacterMovePermit>();
        player_input = player_status.player_input;
        player_move_script = this.gameObject.GetComponent<PlayerMoveScript>();
        cross_hair_event = player_status.cross_hair_event; // PlayerStatusから取得

        hide_type_target_object.OnValueChanged += OnHideTypeChanged; // 値変更時のコールバック登録

        if (IsOwner)
        {
            player_input.actions["RaycastAction"].performed += PerfomedHideRaycastAction;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (hide_type_target_object != null)
        {
            hide_type_target_object.OnValueChanged -= OnHideTypeChanged;
        }
        if (IsOwner && player_input != null)
        {
            player_input.actions["RaycastAction"].performed -= PerfomedHideRaycastAction;
        }
    }

    private void OnHideTypeChanged(TargetHideObject.HideType previousValue, TargetHideObject.HideType newValue)
    {
        if (IsOwner)
        {
            Debug.Log($"[Client] hide_type_target_object changed from {previousValue} to {newValue}");
        }
    }

    // ──────────────────────────────────
    // クライアント RPC：隠れる際の自分側処理
    // ──────────────────────────────────
    [ClientRpc]
    public void HideClientRpc(TargetHideObject.HideType actualHideType)
    {
        PlayerDeformedByHideType(actualHideType); // 引数で渡されたHideTypeを使用

        // ★ ClientNetworkTransform 環境では CC の有効/無効はオーナーだけで触る
        if (IsOwner && player_character_controller != null)
        {
            player_character_controller.enabled = false;
        }

        this.gameObject.layer = LayerMask.NameToLayer("HidePlayer");

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
        if (IsOwner && player_character_controller != null)
        {
            player_character_controller.enabled = true;
        }

        // カメラ位置やアニメーションのリセット（オーナーのみ）
        if (IsOwner)
        {
            player_status.HeadPos.position = player_status.StandHeadPos.position;
            player_status.CameraRoot.position = player_status.StandHeadPos.position;
        }

        player_anim.SetBool("FaceDown", false);
        player_anim.SetBool("Crouch", false);
        this.gameObject.layer = LayerMask.NameToLayer("Player");
        charactor_move_permit.Move(); // 移動許可

        if (IsOwner)
        {
            HidingNowChangeServerRpc(false);
        }
    }

    private void PlayerDeformedByHideType(TargetHideObject.HideType typeToDeform)
    {
        if (IsOwner) // カメラやローカルな見た目の変更はオーナーのみ
        {
            Debug.Log($"[Client Owner] PlayerDeformedByHideType called with type: {typeToDeform}");
            if (typeToDeform == TargetHideObject.HideType.Stand)
            {
                player_status.HeadPos.position = player_status.StandHeadPos.position;
                player_status.CameraRoot.position = player_status.StandHeadPos.position;
            }
            else if (typeToDeform == TargetHideObject.HideType.Crouch)
            {
                player_status.HeadPos.position = player_status.CrouchHeadPos.position;
                player_status.CameraRoot.position = player_status.CrouchHeadPos.position;
            }
            else if (typeToDeform == TargetHideObject.HideType.FaceDown)
            {
                player_status.HeadPos.position = player_status.FaceDownHeadPos.position;
                player_status.CameraRoot.position = player_status.FaceDownHeadPos.position;
            }
        }
        if (IsOwner)
        {
           PlayerDeformedByHideTypeRequestServerRpc();
        }
    }

    [ServerRpc]
    private void PlayerDeformedByHideTypeRequestServerRpc()
    {
        player_move_script.UpdateCrouchNetworkChangeServerRpc(false);
        if (hide_type_target_object.Value == TargetHideObject.HideType.Crouch)
        {
            player_anim.SetBool("Crouch", true);
            player_anim.SetBool("FaceDown", false);
        }
        else if (hide_type_target_object.Value == TargetHideObject.HideType.FaceDown)
        {
            player_anim.SetBool("FaceDown", true);
            player_anim.SetBool("Crouch", false);
        }
        else
        {
            player_anim.SetBool("Crouch", false);
            player_anim.SetBool("FaceDown", false);
        }
    }

    [ServerRpc]
    private void HidingNowChangeServerRpc(bool _HidingNow)
    {
        player_status.HidingNow.Value = _HidingNow; // NetworkVariableの更新
        player_status.CrouchPermit.Value = !_HidingNow;        
        if (_HidingNow)
        {
            charactor_move_permit.Stop();
            player_status.player_move_state_network.Value = PlayerStatus.PlayerMoveState.Stop;
            player_status.foot_step_sound_controller.move_status.Value = FootStepSoundController.MoveStatus.Stop;
        }
        else
        {
            player_move_script.NowSpeed = player_move_script.WalkSpeed; // 通常速度に戻す
            charactor_move_permit.Move();
        }
    }

    private void PerfomedHideRaycastAction(InputAction.CallbackContext ctx)
    {
        if (!IsOwner || !HidingPermit.Value) return;

        if (player_status.HidingNow.Value) // すでに隠れている場合 -> 出る
        {
            HideUnHideRayCastActionPerformedRequestServerRpc(0); // 0 はターゲットなしを示す
        }
        else // 隠れていない場合 -> 隠れる
        {
            if (cross_hair_event != null && cross_hair_event.cross_hair_status.RaycastHitedExistsByCrossHairEvent && cross_hair_event.cross_hair_status.HitedTag == "Hide")
            {
                GameObject hitedObject = cross_hair_event.cross_hair_status.HitedObj;
                if (hitedObject != null)
                {
                    NetworkObject targetNetworkObject = hitedObject.GetComponent<NetworkObject>();
                    if (targetNetworkObject != null)
                    {
                        HideUnHideRayCastActionPerformedRequestServerRpc(targetNetworkObject.NetworkObjectId);
                    }
                    else
                    {
                        Debug.LogWarning("Raycast hit 'Hide' tag, but no NetworkObject found on the hited object.");
                    }
                }
            }
            else
            {
                Debug.Log("Cannot hide: No valid 'Hide' object targeted by crosshair.");
            }
        }
    }

    public void GetCoughtWhenHidingByEnemy(GameObject _SightningEnemy)
    {
        if (!IsServer) return;

        Animator enemy_anim = _SightningEnemy.GetComponent<Animator>();
        if (player_status.HidingNow.Value && !player_status.Dead.Value && !layLookDownTriggered)
        {
            if (hide_type_target_object.Value == TargetHideObject.HideType.FaceDown && enemy_anim.parameters.Any(p => p.name == "LayLookDown"))
            {
                Debug.Log("Enemy found player hiding (FaceDown). Triggering enemy animation.");
                enemy_anim.SetTrigger("LayLookDown");
                layLookDownTriggered = true;
            }
            else
            {
                Debug.Log("Enemy found player hiding (Not FaceDown). Player unhides and dies.");
                target_hide_object.UnHideRequestServerRpc(this.NetworkObjectId);
                UnHideClientRpc(); // 見た目/CCはクライアント側で処理
                player_status.DeadEvent(_SightningEnemy);
            }
            player_status.cross_hair_event.CrossHairEventRaycastPermit.Value = true;
        }
    }

    public void LayLookDownAnimationEndDeathOnServer()
    {
        target_hide_object.UnHideRequestServerRpc(this.NetworkObjectId);
        player_status.player_hide_object_action.UnHideClientRpc();
    }

    [ServerRpc]
    public void HideUnHideRayCastActionPerformedRequestServerRpc(ulong _TargetNetworkObjectId, ServerRpcParams rpcParams = default)
    {
        if (!HidingPermit.Value) return;

        ulong requestingClientId = rpcParams.Receive.SenderClientId;
        if (this.OwnerClientId != requestingClientId)
        {
            Debug.LogError($"Security: Client {requestingClientId} tried to call Hide/UnHide for player {this.OwnerClientId}. Denied.");
            return;
        }

        if (!player_status.HidingNow.Value) // まだ隠れていない -> 隠れる処理
        {
            if (_TargetNetworkObjectId == 0) {
                Debug.LogError("Hide action initiated without a target object ID on server.");
                return;
            }

            if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(_TargetNetworkObjectId, out NetworkObject hideNetworkObj))
            {
                Debug.LogError($"Could not find NetworkObject with ID: {_TargetNetworkObjectId} on server.");
                return;
            }
            GameObject hideObjGO = hideNetworkObj.gameObject;

            TargetHideObject.HideType typeOfHideObject = TargetHideObjectSet(hideObjGO); // サーバー側で設定

            if (target_hide_object.SomeoneHided.Value == false)
            {
                player_status.cross_hair_event.CrossHairEventRaycastPermit.Value = false;
                target_hide_object.HideRequestServerRpc(this.NetworkObject.NetworkObjectId); // 対象オブジェクトに通知
                HideClientRpc(typeOfHideObject); // 見た目/CC(オーナーのみ)等
            }
            else
            {
                ClientRpcParams clientRpcParams = new ClientRpcParams {
                    Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { OwnerClientId } }
                };
                NotifyClientActionFailedClientRpc("誰かが隠れている...", clientRpcParams);
            }
        }
        else // すでに隠れている -> 出る処理
        {
            if (target_hide_object != null)
            {
                player_status.cross_hair_event.CrossHairEventRaycastPermit.Value = true;
                target_hide_object.UnHideRequestServerRpc(this.NetworkObject.NetworkObjectId);
            }
            else
            {
                Debug.LogWarning($"[Server] Player {OwnerClientId} is trying to unhide, but target_hide_object is not set on server.");
            }
            UnHideClientRpc();
        }
    }

    // サーバーでのみ実行される。隠れる対象オブジェクトの情報を設定。
    private TargetHideObject.HideType TargetHideObjectSet(GameObject _TargetObject)
    {
        target_hide_object = _TargetObject.GetComponent<TargetHideObject>();
        if (target_hide_object != null)
        {
            hide_type_target_object.Value = target_hide_object.hide_type; // サーバーでNetworkVariableを更新
            return target_hide_object.hide_type;
        }
        Debug.LogError("TargetHideObject component not found on the target object.");
        return TargetHideObject.HideType.Stand;
    }

    [ClientRpc]
    private void NotifyClientActionFailedClientRpc(string message, ClientRpcParams clientRpcParams = default)
    {
        if (player_status != null && player_status.sub_noticed_ui_controller != null)
        {
            player_status.sub_noticed_ui_controller.Notice(message);
        }
        else
        {
            Debug.LogWarning($"[Client] Could not show notice '{message}' because player_status or sub_noticed_ui_controller is null.");
        }
    }
}
