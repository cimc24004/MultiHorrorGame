using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Unity.Netcode;

public class TheDoorController : NetworkBehaviour
{
    // 複数枚のドア（左右や両開きなど）をまとめて管理する。
    // ここに DoorOpenCloseController を登録しておくと、開閉時に全てへ伝播する。
    [Header("このドアを構成する DoorOpenCloseController をセット（1つ以上）")]
    [RequiredListLength(1, null)]
    public List<DoorOpenCloseController> door_open_close_controllers;

    public enum DoorStatus
    {
        Closed,
        Opened
    }

    // ネットワーク同期用のドア状態
    public NetworkVariable<DoorStatus> door_status_network =
        new NetworkVariable<DoorStatus>(DoorStatus.Closed, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("鍵が必要なドアかどうか")]
    public NetworkVariable<bool> NeededKey = new NetworkVariable<bool>();

    // アニメ完了フラグ（多重入力を防ぐためのガード）
    public bool OpenedClosedCompletedAnimation = true;
    public bool OpenedClosedCompleted = true;

    private PlayerStatus player_status;
    private float CompletedWaitTimeNow;
    private NeedKeysController need_keys_controller;

    [Header("鍵が必要な場合は必ず設定してね")]
    public OpenPermitByKeyController open_permit_by_key_controller;

    public DoorAttackedController door_attacked_controller;

    void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InitOnPlayerSpawn;
    }

    void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitOnPlayerSpawn;
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (NeededKey.Value && open_permit_by_key_controller == null)
        {
            Debug.LogError("[TheDoorController] 鍵が必要だけど open_permit_by_key_controller が未設定です。", this);
        }
        // if (door_attacked_controller == null && this.gameObject.tag == "Door")
        // {
        //         // Debug.LogError("[DoorAttackedController]が存在しません！！！！ : " + this.gameObject.name, this);
        // }
    }
    private void InitOnPlayerSpawn(PlayerStatus _playerStatus)
    {
        player_status = _playerStatus;
        need_keys_controller = NeedKeysController.instance;
    }

    private void Update()
    {
        // 開閉アニメ後のディレイ。一定時間で入力受付を再開
        if (OpenedClosedCompletedAnimation)
        {
            CompletedWaitTimeNow += Time.deltaTime;
            if (CompletedWaitTimeNow > 0.3f)
            {
                OpenedClosedCompleted = true;
                CompletedWaitTimeNow = 0;
            }
        }
    }

    /// <summary>ドアを強制的にロックする（鍵必須にする）</summary>
    public void DoorLock()
    {
        NeededKey.Value = true;
        if (open_permit_by_key_controller != null)
        {
            open_permit_by_key_controller.OpenPermit.Value = false;
        }
        else
        {
            Debug.LogError("open_permit_by_key_controller が未設定です: " +
                      this.gameObject.transform.parent.gameObject.transform.parent.gameObject, this);
        }
    }

    /// <summary>
    /// ドアの開閉トグル要求（サーバ実行）
    /// _ActionedNetworkObjectId: 実行者（プレイヤー/敵）の NetworkObjectId
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void DoorOpenClosedChangeByStatusServerRPC(ulong _ActionedNetworkObjectId, ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;

        // このRPCを投げた実行者（EventActionMan）を取得（プレイヤー or 敵）
        GameObject EventActionMan = NetworkManager.Singleton.SpawnManager.SpawnedObjects[_ActionedNetworkObjectId].gameObject;

        // 鍵が必要なドアなら、開ける権限があるか確認。なければプレイヤーに通知して中断
        if (NeededKey.Value == true)
        {
            // OpenPermit が true でなければ開けられない
            if (!open_permit_by_key_controller.OpenPermit.Value)
            {
                if (EventActionMan.tag == "Player")
                {
                    // このプレイヤーに「鍵が必要」を通知
                    open_permit_by_key_controller.NeedKeyForPlayerClientRpc(new ClientRpcParams
                    {
                        Send = new ClientRpcSendParams
                        {
                            TargetClientIds = new ulong[] { clientId }
                        }
                    });
                }
                return;
            }
        }

        // 実行者が敵の場合、追跡中など開閉を抑制したい条件があれば中断
        if (EventActionMan.tag == "Enemy")
        {
            var discovery_player_controller = EventActionMan.GetComponent<DiscoveryPlayerController>();
            var enemy_sound_sensor = EventActionMan.GetComponent<EnemySoundSensor>();
            if (discovery_player_controller.DiscoveryPlayerNowCheck())
            {
                // 追跡中などは開閉しない
                return;
            }
        }

        Debug.Log("Door-----Open/Close Toggle-----DoorOpenClosedChangeByStatusServerRPC");

        // 多重実行防止（アニメ中は無視）
        if (!OpenedClosedCompleted) return;

        if (door_status_network.Value == DoorStatus.Opened)
        {
            DoorsClose();
        }
        else if (door_status_network.Value == DoorStatus.Closed)
        {
            DoorsOpen();
        }
    }

    /// <summary>鍵での開放許可があるか</summary>
    public bool DoorOpenPermitCheck()
    {
        return open_permit_by_key_controller.OpenPermit.Value;
    }

    private void DoorsOpen()
    {
        OpenedClosedCompleted = false;
        OpenedClosedCompletedAnimation = false;

        foreach (var door_open_close_controller in door_open_close_controllers)
        {
            door_open_close_controller.Open();
        }

        door_status_network.Value = DoorStatus.Opened;
    }

    private void DoorsClose()
    {
        OpenedClosedCompleted = false;
        OpenedClosedCompletedAnimation = false;

        foreach (var door_open_close_controller in door_open_close_controllers)
        {
            door_open_close_controller.Close();
        }

        door_status_network.Value = DoorStatus.Closed;
    }
}
