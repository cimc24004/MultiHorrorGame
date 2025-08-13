using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Unity.Netcode;

public class TheDoorController : NetworkBehaviour
{

    //�q�v�f�ɑ��݂���h�A������List�Ƃ��ēo�^���A��C�ɊJ����B�@��C�ɕ߂�Ƃ�����������ׂ̂�B
    //������ł����Ȃ������ł������B
    [Header("�h�A���Ƃɂ��Ă���DoorOpenCloseController�X�N���v�g���Z�b�g�B")]
    [RequiredListLength(1, null)]
    public List<DoorOpenCloseController> door_open_close_controllers;


    public enum DoorStatus{
        Closed,
        Opened
    }

    public NetworkVariable<DoorStatus> door_status_network;
    [Header("�L�[���K�v�ȃh�A���ǂ����B")]
    public NetworkVariable<bool> NeededKey = new NetworkVariable<bool>();


    //�v���C���[���������b�N����


    public bool OpenedClosedCompletedAnimation = true;
    public bool OpenedClosedCompleted = true;
    private PlayerStatus player_status;
    private float CompletedWaitTimeNow;
    private NeedKeysController need_keys_controller;
    // [ShowIf("NeededKey")]
    [Header("�������������ꍇ�͂���������Ă�������")]
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
    private void InitOnPlayerSpawn(PlayerStatus _playerStatus){
        player_status = _playerStatus;
        need_keys_controller = NeedKeysController.instance;
    }

    private void Update()
    {
        if (OpenedClosedCompletedAnimation)
        {
            CompletedWaitTimeNow += Time.deltaTime;
            if(CompletedWaitTimeNow > 0.3f)
            {
                OpenedClosedCompleted = true;
                CompletedWaitTimeNow = 0;
            }
        }

    }
    
    public void DoorLock()
    {
        //�h�A�������I�Ƀ��b�N����֐�
        NeededKey.Value = true;
        if (open_permit_by_key_controller != null)
        {
            open_permit_by_key_controller.OpenPermit.Value = false;
        }
        else
        {
            Debug.Log("this.gameObjectはopen_permit_by_key_controllerがありません : " + this.gameObject.transform.parent.gameObject.transform.parent.gameObject);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void DoorOpenClosedChangeByStatusServerRPC(ulong _ActionedNetworkObjectId, ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        //EventActionMan: ���̊֐����s�������s�l��(�v���C���[��G�Ȃ�)
        //�h�A���X�e�[�^�X�ɂ���ĕω�������B
        //�󂢂Ă����ԂȂ�߂�B�@�J���Ă����ԂȂ�J����B
        GameObject EventActionMan = NetworkManager.Singleton.SpawnManager.SpawnedObjects[_ActionedNetworkObjectId].gameObject;
        //�����K�v�ȃh�A�Ȃ�܂��������̃h�A�Ɏg�������ǂ����𔻒�B �g���ĂȂ�������return; �J���Ȃ���B
        if (NeededKey.Value == true)
        {
            //NeededKey��true�����ł̓��b�N���ꂽ���Ƃɂ͂Ȃ�Ȃ��B
            if (!open_permit_by_key_controller.OpenPermit.Value == true)
            {

                //�����������Ă���
                if (EventActionMan.tag == "Player")
                {
                    //�����������Ă��邱�Ƃ�m�点��B
                    open_permit_by_key_controller.NeedKeyForPlayerClientRpc(new ClientRpcParams
                    {
                        Send = new ClientRpcSendParams
                        {
                            TargetClientIds = new ulong[] { clientId }
                        }
                    });
                    // open_permit_by_key_controller.NeedKeyForPlayerRequestServerRpc();
                }

                return;
            }
        }

        if (EventActionMan.tag == "Enemy")
        {
            //�G���v���C���[�𔭌����Ă�����A�߂�ꂽ���͉󂵂ĊJ����悤�ɂ���B
            DiscoveryPlayerController discovery_player_controller = EventActionMan.GetComponent<DiscoveryPlayerController>();
            EnemySoundSensor enemy_sound_sensor = EventActionMan.GetComponent<EnemySoundSensor>();
            if (discovery_player_controller.DiscoveryPlayerNowCheck())
            {
                return;
            }

        }

        Debug.Log("Door-----Open---------------------DoorOpenClosedChangeByStatusServerRPC");
        //�J���Ă�߂Ă�A�j���[�V�����Đ����Ȃ炱�̃A�N�V���������s���Ȃ��B
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

    public bool DoorOpenPermitCheck()
    {
        return open_permit_by_key_controller.OpenPermit.Value;
    }


    private void DoorsOpen()
    {
        this.OpenedClosedCompleted = false;
        this.OpenedClosedCompletedAnimation = false;
        foreach (DoorOpenCloseController door_open_close_controller in door_open_close_controllers)
        {
            door_open_close_controller.Open();
        }

        door_status_network.Value = DoorStatus.Opened;
    }
    private void DoorsClose()
    {
        this.OpenedClosedCompleted = false;
        this.OpenedClosedCompletedAnimation = false;
        foreach (DoorOpenCloseController door_open_close_controller in door_open_close_controllers)
        {
            door_open_close_controller.Close();
        }

        door_status_network.Value = DoorStatus.Closed;
    }

}
