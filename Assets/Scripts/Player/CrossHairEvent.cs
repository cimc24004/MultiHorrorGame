using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CrossHairEvent : NetworkBehaviour
{
    [Header("画面中央からのレイキャストで調べる対象レイヤーマスク")]
    public LayerMask TargetLayerMaskViewToPoint;

    public ItemBelongingsController item_belonging_controller;
    public AudioDatas KeyAudioDatas;
    public PlayerHideObjectAction player_hide_object_action;
    public AudioSource ItemGetSource;
    // 入力
    private PlayerInput player_input;

    // 自分プレイヤー関連
    private PlayerStatus player_status;
    private bool initPlayer;
    private GameObject playerGO;
    private NetworkObject playerNetObj;

    [Header("レイキャスト到達距離")]
    public float RaycastDistance = 3f;

    // クロスヘア状態の公開構造体（別コンポーネントで参照）
    public CrossHairStatus cross_hair_status;

    // 許可フラグ（ネット経由で外から無効にできる）
    public NetworkVariable<bool> CrossHairEventRaycastPermit = new NetworkVariable<bool>(true);
    public NetworkVariable<bool> CrossHairEventPermitForDeath = new NetworkVariable<bool>(true);
    // 内部
    private Camera mainCam;
    private RaycastHit hit;
    public GameObject RaycasstedObj { get; private set; }

    // 祈り対象の押下状態を保持（押し始めにロックして、離す/外れるまで維持）
    private PrayController currentPray;

    // いくつかの他要素に触るため（必要なら）
    private TheDoorController the_door_controller;
    private MainNoticedUIController noticedUI;
    private PlayerAllController playerAllController;

    /*━━━━━━━━ ライフサイクル ━━━━━━━━*/
    private void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InitOnPlayerSpawn;
    }

    private void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitOnPlayerSpawn;
        if (initPlayer && IsOwner && player_input != null)
        {
            player_input.actions["RaycastAction"].started  -= OnRaycastActionStarted;
            player_input.actions["RaycastAction"].canceled -= OnRaycastActionCanceled;
        }
    }

    private void InitOnPlayerSpawn(PlayerStatus _playerStatus)
    {
        playerAllController = PlayerAllController.instance; // （存在する前提のシングルトン）
        player_status = _playerStatus;
        player_input  = player_status.player_input;
        noticedUI     = player_status.main_noticed_ui_controller;
        playerGO      = player_status.gameObject;
        playerNetObj  = playerGO.GetComponent<NetworkObject>();
        mainCam       = Camera.main;

        if (IsOwner && player_input != null)
        {
            player_input.actions["RaycastAction"].started  += OnRaycastActionStarted;
            player_input.actions["RaycastAction"].canceled += OnRaycastActionCanceled;
        }

        initPlayer = true;
    }

    /*━━━━━━━━ Update ━━━━━━━━*/
    private void Update()
    {
        if (!initPlayer || !IsOwner) return;

        // 画面中心からレイ
        var ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Debug.DrawRay(mainCam.transform.position, mainCam.transform.forward * RaycastDistance, Color.yellow);

        if (Physics.Raycast(ray, out hit, RaycastDistance, TargetLayerMaskViewToPoint))
        {
            RaycasstedObj = hit.collider.gameObject;

            // タグごとの軽いUI通知（任意）
            if (CrossHairEventPermitForDeath.Value)
            {
                if (RaycasstedObj.CompareTag("HideActionObject"))
                {
                    noticedUI?.Notice("隠れる", 1);
                }
                else if (RaycasstedObj.CompareTag("CantFirstAidKit"))
                {
                    // 死亡中の他プレイヤーに救急箱が必要、などの文言
                    var targetPS = playerAllController?.SpawnedPlayersWithPlayerStatuses[RaycasstedObj.transform.parent.gameObject];
                    if (targetPS != null && targetPS.Dead.Value && !targetPS.firstAidKitForPlayer.CanFirstAid)
                    {
                        noticedUI?.Notice("救急箱が必要だ");
                    }
                }
            }

            // クロスヘア状態を共有
            if (CrossHairEventPermitForDeath.Value &&
               (RaycasstedObj.layer == LayerMask.NameToLayer("Item") ||
                RaycasstedObj.CompareTag("HideActionObject") ||
                RaycasstedObj.CompareTag("Door") ||
                RaycasstedObj.CompareTag("Drawer") ||
                RaycasstedObj.CompareTag("Light") ||
                RaycasstedObj.CompareTag("Pray") ||
                RaycasstedObj.CompareTag("DeliverPlace") ||
                RaycasstedObj.CompareTag("ReadEvent") ||
                RaycasstedObj.CompareTag("Window") ||
                RaycasstedObj.CompareTag("Cabinet") ||
                RaycasstedObj.CompareTag("GameClear")))
            {
                cross_hair_status.HitedTag = RaycasstedObj.tag;
                cross_hair_status.HitedObj = RaycasstedObj;
                cross_hair_status.RaycastHitedExistsByCrossHairEvent = true;
            }
            else
            {
                cross_hair_status.RaycastHitedExistsByCrossHairEvent = false;
            }

            // クリック（トリガー）系の単発アクション
            if (!CrossHairEventRaycastPermit.Value) return;
            if (player_input.actions["RaycastAction"].triggered)
            {
                // アイテム
                if (RaycasstedObj.layer == LayerMask.NameToLayer("Item"))
                {
                    if (CrossHairEventPermitForDeath.Value)
                    {
                        var itemObj = RaycasstedObj.GetComponent<ItemObject>();
                        itemObj?.TryPickup();
                    }
                    return;
                }

                // 納品場所
                if (RaycasstedObj.CompareTag("DeliverPlace"))
                {
                    if (CrossHairEventPermitForDeath.Value)
                    {
                        var place = RaycasstedObj.GetComponent<DeliverMemoryPlace>();
                        place?.Deliver(item_belonging_controller);
                    }
                    return;
                }

                // 隠れる
                if (RaycasstedObj.CompareTag("HideActionObject"))
                {
                    if (CrossHairEventPermitForDeath.Value && !player_status.HidingNow.Value)
                    {
                        var netObj = RaycasstedObj.GetComponent<NetworkObject>();
                        if (netObj != null)
                            player_hide_object_action.HideUnHideRayCastActionPerformedRequestServerRpc(netObj.NetworkObjectId);
                    }
                    return;
                }

                // ドア/窓/キャビネット
                if (RaycasstedObj.CompareTag("Door") || RaycasstedObj.CompareTag("Cabinet") || RaycasstedObj.CompareTag("Window"))
                {
                    var doorCtrl = RaycasstedObj.GetComponent<DoorOpenCloseController>();
                    the_door_controller = doorCtrl?.the_door_controller;
                    the_door_controller?.DoorOpenClosedChangeByStatusServerRPC(playerNetObj.NetworkObjectId);
                    return;
                }

                // 引き出し
                if (RaycasstedObj.CompareTag("Drawer"))
                {
                    var drawer = RaycasstedObj.GetComponent<ItemBoxOpenCloseController>();
                    drawer?.RequestToggle();
                    return;
                }

                // ライト
                if (RaycasstedObj.CompareTag("Light"))
                {
                    var lightCtrl = RaycasstedObj.GetComponent<TurningOnOffLightController>();
                    lightCtrl?.ToggleLight();
                    return;
                }

                // 読む
                if (RaycasstedObj.CompareTag("ReadEvent"))
                {
                    var read = RaycasstedObj.GetComponent<ReadEventController>();
                    read?.ReadEventStart();
                    return;
                }

                // クリア
                if (RaycasstedObj.CompareTag("GameClear"))
                {
                    SceneManager.LoadScene("GameClear");
                    return;
                }
            }
        }
        else
        {
            cross_hair_status.RaycastHitedExistsByCrossHairEvent = false;
            RaycasstedObj = null;
        }

        // ★長押し中の「祈り」管理（毎フレームRPCは送らない）
        bool pressed = player_input.actions["RaycastAction"].IsPressed();

        if (pressed)
        {
            // 1) 狙ってた祈り対象から外れたら、一回だけFalseを送る
            if (currentPray != null &&
                (RaycasstedObj == null || RaycasstedObj != currentPray.gameObject))
            {
                currentPray.PrayNowFalseRequestServerRpc(this.OwnerClientId);
                currentPray = null;
            }

            // 2) まだロックしていない & いま祈り対象を見ている なら新規にTrueを一回だけ送る
            if (currentPray == null && RaycasstedObj != null && RaycasstedObj.CompareTag("Pray"))
            {
                var pc = RaycasstedObj.GetComponent<PrayController>();
                if (pc != null)
                {
                    currentPray = pc;
                    currentPray.PrayNowTrueRequestServerRpc(this.OwnerClientId);
                }
            }
        }
        else
        {
            // 離した瞬間：ロック中の対象へFalseを1回だけ送って解除
            if (currentPray != null)
            {
                currentPray.PrayNowFalseRequestServerRpc(this.OwnerClientId);
                currentPray = null;
            }
        }
    }

    /*━━━━━━━━ 入力イベント（押し始め/離した） ━━━━━━━━*/
    private void OnRaycastActionStarted(InputAction.CallbackContext ctx)
    {
        // 押し始め時点で祈り対象を見ていれば即ロックしてTrue
        if (RaycasstedObj != null && RaycasstedObj.CompareTag("Pray"))
        {
            var pc = RaycasstedObj.GetComponent<PrayController>();
            if (pc != null)
            {
                currentPray = pc;
                currentPray.PrayNowTrueRequestServerRpc(this.OwnerClientId);
            }
        }
    }

    private void OnRaycastActionCanceled(InputAction.CallbackContext ctx)
    {
        // 離したらFalseを一回だけ送って解除
        if (currentPray != null)
        {
            currentPray.PrayNowFalseRequestServerRpc(this.OwnerClientId);
            currentPray = null;
        }
    }
}
