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
using Autodesk.Fbx;
using System.Linq;
using System.Security.Cryptography;
using Steamworks;
using Unity.Collections;
using TMPro;
public class PlayerStatus : NetworkBehaviour
{
    public NetworkVariable<FixedString64Bytes> SteamUserName = new NetworkVariable<FixedString64Bytes>();
    // ローカルの人を取得するためのinstance
    public static PlayerStatus LocalInstance { get; private set; }
    public static event Action<PlayerStatus> OnLocalPlayerSpawned;
    public static event Action<PlayerStatus> OnLocalPlayerDeaded;
    [SerializeField] private TextMeshProUGUI steamUserNameTextUI;
    // 値の受け渡しやスタミナなどの管理。プレイヤーの状態などを保存
    public Transform CameraRoot;
    public Transform HeadPos;
    public Transform CrouchHeadPos;
    public Transform StandHeadPos;
    public Transform DownHeadPos;
    public Transform FaceDownHeadPos;
    public CrossHairEvent cross_hair_event;
    public PlayerHideObjectAction player_hide_object_action;
    public PlayerInput player_input;
    public FlashLightController flash_light_controller;
    public SubNoticedUIController sub_noticed_ui_controller;
    public MainNoticedUIController main_noticed_ui_controller;
    public GameObject GageCanvas;
    public Image GageImage;
    public NetworkVariable<bool> Dead = new NetworkVariable<bool>();

    // プレイヤーの移動状態 (走る 歩く しゃがむ ダウン状態 停止)
    public enum PlayerMoveState { Walk, Run, Crouch,Down, Stop }

    // サーバー書き込み・全員読取
    public NetworkVariable<PlayerMoveState> player_move_state_network = new NetworkVariable<PlayerMoveState>();

    [Header("移動速度")]
    public float MoveSpeed;

    public NetworkVariable<bool> HidingNow = new NetworkVariable<bool>();
    public float MoveX;
    public float MoveY;
    [Header("スタミナコスト")]
    public float StaminaCost = 0.3f;
    [Header("現在のスタミナ値")]
    public NetworkVariable<float> StaminaValueNow = new NetworkVariable<float>();
    public NetworkVariable<bool> StaminaValueAutoIncrease = new NetworkVariable<bool>(true);
    [Header("スタミナ1fにつきの回復力")]
    public float StaminaRetakePower;
    [Header("走れる最小スタミナ")]
    public float CanMinStaminaRun = 30;

    public AudioDatas HideActionAudioDatas;
    public bool PlayerKilledScene;

    [SerializeField] private GameObject FlashLightBatteryUI;
    private float offsetY;
    private CharacterMovePermit player_move_permit;
    private PlayerStaminaController player_stamina_controller;
    private PlayerMoveScript player_move_script;
    private Animator anim;
    private bool InitPlayer;
    public NetworkVariable<bool> Crouch = new NetworkVariable<bool>();
    public NetworkVariable<bool> CrouchPermit = new NetworkVariable<bool>(true);
    public CinemachineVirtualCamera virtual_camera;
    public GameObject ItemController;
    public FootStepSoundController foot_step_sound_controller;
    public ItemMenuController item_menu_controller;
    public ItemEquipController item_equip_controller;
    public CharacterController playerCharacterController;
    public FirstAidKitForPlayer firstAidKitForPlayer;
    private ItemBelongingsController item_belonging_controller;
    private PlayerContinueController player_continue_controller;
    private HorrorDirector horror_director;
    private Tween crouchTween;
    private PlayerToEnemyDistanceController player_to_enemy_distance_controller;
    private EnemyStatus killed_enemy_status;
    private ItemDatabases item_databases;
    private EnemyFlinchController killed_enemy_flinch_controller;
    public NetworkVariable<bool> KillSequenceRunning = new(false);
    public override void OnNetworkDespawn()
    {
        if (IsOwner && LocalInstance == this)
            LocalInstance = null;

    }

    public override void OnNetworkSpawn()
    {

        if (IsOwner)
        {
            FixedString64Bytes personaName = SteamFriends.GetPersonaName();
            PlayerNameSetRequestServerRpc(personaName.ToString());
            virtual_camera = CMFPSController.instance.GetComponent<CinemachineVirtualCamera>();
            steamUserNameTextUI.enabled = false;
        }
        player_stamina_controller = GetComponent<PlayerStaminaController>();
        player_continue_controller = GameController.instance.GetComponent<PlayerContinueController>();
        player_move_permit = GetComponent<CharacterMovePermit>();
        anim = GetComponent<Animator>();
        player_hide_object_action = GetComponent<PlayerHideObjectAction>();
        item_belonging_controller = ItemController.GetComponent<ItemBelongingsController>();
        item_menu_controller = ItemController.GetComponent<ItemMenuController>();
        horror_director = HorrorDirector.instance;
        player_to_enemy_distance_controller = GetComponent<PlayerToEnemyDistanceController>();
        player_move_script = GetComponent<PlayerMoveScript>();
        item_databases = ItemDatabases.instance;
        InitPlayer = true;

        if (IsOwner)
        {
            LocalInstance = this;
            OnLocalPlayerSpawned?.Invoke(this);
        }
        if (!IsOwner)
        {
            item_equip_controller.gameObject.SetActive(false);
            sub_noticed_ui_controller.gameObject.SetActive(false);
            item_belonging_controller.gameObject.SetActive(false);
            main_noticed_ui_controller.gameObject.SetActive(false);
            player_stamina_controller.StaminaCanvas.gameObject.SetActive(false);
            cross_hair_event.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!InitPlayer) return;

        if (IsServer)
        {
            UpdateMoveStatusFromPlayerStatus();
            // 自分以外のキャラのアニメだけ同期
            if (!IsOwner)
            {
                anim.SetFloat("MoveY", MoveY);
                anim.SetFloat("MoveX", MoveX);
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
    [ServerRpc]
    private void PlayerNameSetRequestServerRpc(string _SteamUserName)
    {
        SteamUserName.Value = _SteamUserName;
        PlayerNameTextUISetClientRpc(_SteamUserName);
    }
    [ClientRpc]
    private void PlayerNameTextUISetClientRpc(string _SteamUserName)
    {
        steamUserNameTextUI.SetText(_SteamUserName);
    }
    [Button]
    public void Revival()
    {
        Dead.Value = false;
        anim.SetBool("AppearentDeath", false);
        player_hide_object_action.HidingPermit.Value = true;
        this.CrouchPermit.Value = true;
        cross_hair_event.CrossHairEventPermitForDeath.Value = true;
        var rpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { this.OwnerClientId }
            }
        };
        //Client
        this.gameObject.tag = "Player";
        RevivalClientRpc(rpcParams);
    }
    [ClientRpc]
    private void RevivalClientRpc(ClientRpcParams rpcParams)
    {
        Vector3 src = CameraRoot.localPosition;
        Transform tgt = HeadPos;
        float tgtY = tgt.localPosition.y;
        Tween StandTween = DOTween.To(
            () => src.y,
            y => CameraRoot.localPosition = new Vector3(src.x, y, src.z),
            tgtY,
            0.2f
        ).SetEase(Ease.OutQuad);
    }
    public void DeadEvent(GameObject KilledMeEnemy)
    {
        if (this.Dead.Value) return;
        //サーバーから実行される。
        this.KillSequenceRunning.Value = true;
        Debug.Log("あなたは死にました！" + "DeadEvent DeadPlayerClientRpcへ向かいたい");
        var rpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { this.OwnerClientId }
            }
        };
        if (KilledMeEnemy.tag == "Enemy")
        {
            killed_enemy_status = KilledMeEnemy.GetComponent<EnemyStatus>();
            ulong enemyNetId = killed_enemy_status.NetworkObjectId;
            killed_enemy_flinch_controller = killed_enemy_status.enemy_flinch_controller;
            DeadPlayerClientRpc(enemyNetId, rpcParams);
        }

    }
    [ClientRpc]
    private void DeadPlayerClientRpc(ulong enemyNetworkObjectId, ClientRpcParams clientRpcParams = default)
    {
        if (!IsOwner) return;
        // collider.enabled = false;
        // collider.enabled = true;

        Debug.Log("mitti この関数はDeadPlayerClientRpcです。");
        // アイテム所持チェック
        var items = item_belonging_controller.ItemFindByItemType(ItemStatus.ItemType.EmergencyAttack);
        Debug.Log($"mitti DeadPlayerClientRpc--- items.Count == {items.Count}");
        if (items.Count > 0)
        {
            Debug.Log("mitti お札アイテムを使用するため、死ぬことはない！items.Count>0 DeadPlayerClientRpc内です");
            var item = items[0];
            item_belonging_controller.ItemUseFromBelonging(item);
            sub_noticed_ui_controller.Notice($"{item.ItemName}を使用");
            // サーバーにお札生成依頼
            ItemUserForDeadPlayerRequestServerRpc(item.GlobalId);
        }
        else
        {
            // enemyNetworkObjectId を使ってクライアント側で演出
            Debug.Log("HorrorDirectorの演出がされるはず！ あなたのOwnerClientId: " + this.OwnerClientId + "DeadPlayerClientRpc内です");
            var enemyGO = NetworkManager.Singleton.SpawnManager
                            .SpawnedObjects[enemyNetworkObjectId].gameObject;
            Vector3 src = CameraRoot.localPosition;
            Transform tgt = DownHeadPos;
            float tgtY = tgt.localPosition.y;
            Tween DownTween = DOTween.To(
                () => src.y,
                y => CameraRoot.localPosition = new Vector3(src.x, y, src.z),
                tgtY,
                0.2f
            ).SetEase(Ease.OutQuad);
            Debug.Log("enemyGO: " + enemyGO);
            var cameraExpander = enemyGO.GetComponent<CameraEnemyFaceExpanding>();
            // flash_light_controller.Light.enabled = false;
            horror_director.nearEnemyCount = 0;
            horror_director.discoveredEnemyCount = 0;
            horror_director.ChangeHorrorState(HorrorState.PlayerKilled);
            Debug.Log("cameraExpander: " + cameraExpander);
            cameraExpander.FaceExpandingActive(this.gameObject);
            DOVirtual.DelayedCall(4, () =>
            {
                cameraExpander.FaceExpandingUnActive();
                horror_director.ChangeHorrorState(HorrorState.None);
            });
            NotHaveItemForDeadPlayerRequestServerRpc();
        }
    }

    [ServerRpc]
    private void ItemUserForDeadPlayerRequestServerRpc(uint _itemGlobalId)
    {
        //アイテムを使うから死なない処理
        ItemStatus item = item_databases.ItemFindByGlobalId(_itemGlobalId);
        GameObject g = Instantiate(item.ItemThrowPrefab, killed_enemy_status.EyePosition.transform.position, Quaternion.identity, killed_enemy_status.EyePosition.transform);
        NetworkObject g_NetworkObject = g.GetComponent<NetworkObject>();
        g_NetworkObject.Spawn();
        Destroy(g, item.StopEnemyTime);
        killed_enemy_flinch_controller.FlinchStart(item.StopEnemyTime);
        KillSequenceRunning.Value = false;
    }
    [ServerRpc]
    private void NotHaveItemForDeadPlayerRequestServerRpc()
    {
        //普通に死ぬ処理
        this.Dead.Value = true;
        player_move_script.UpdateCrouchNetworkChangeServerRpc(false);
        anim.SetBool("AppearentDeath", true);
        this.gameObject.tag = "DeadPlayer";
        OnLocalPlayerDeaded.Invoke(this);
        PlayerKilledScene = true;
        DOVirtual.DelayedCall(4, () =>
        {
            PlayerKilledScene = false;
        });
        player_hide_object_action.HidingPermit.Value = false;
        KillSequenceRunning.Value = false;
        this.CrouchPermit.Value = false;
        cross_hair_event.CrossHairEventPermitForDeath.Value = false;
        
    }
    public void CrouchStandCameraPosChange(bool _Crouch)
    {
        if (!this.CrouchPermit.Value) return;
        Transform tgt = _Crouch ? CrouchHeadPos : StandHeadPos;
        crouchTween?.Kill();
        this.HeadPos.position = tgt.position;
        Vector3 src = CameraRoot.localPosition;
        float tgtY = tgt.localPosition.y;
        crouchTween = DOTween.To(
            () => src.y,
            y => CameraRoot.localPosition = new Vector3(src.x, y, src.z),
            tgtY,
            0.2f
        ).SetEase(Ease.OutQuad);
    }
    // PlayerStatusから移動状態を取得してmove_statusを更新
    private void UpdateMoveStatusFromPlayerStatus()
    {
        switch (this.player_move_state_network.Value)
        {
            case PlayerStatus.PlayerMoveState.Run:
                foot_step_sound_controller.move_status.Value = FootStepSoundController.MoveStatus.Run;
                break;
            case PlayerStatus.PlayerMoveState.Crouch:
                foot_step_sound_controller.move_status.Value = FootStepSoundController.MoveStatus.Crouch;
                break;
            case PlayerStatus.PlayerMoveState.Stop:
                foot_step_sound_controller.move_status.Value = FootStepSoundController.MoveStatus.Stop;
                break;
            case PlayerStatus.PlayerMoveState.Down:
                foot_step_sound_controller.move_status.Value = FootStepSoundController.MoveStatus.Stop;
                break;
            default:
                foot_step_sound_controller.move_status.Value = FootStepSoundController.MoveStatus.Walk;
                break;
        }
    }
}
