using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class TheDoorController : SerializedMonoBehaviour
{

    //子要素に存在するドアたちをListとして登録し、一気に開ける。　一気に閉めるとかを実装する為のやつ。
    //一つだけでいいなら一つだけでも入れる。
    [Header("ドアごとについているDoorOpenCloseControllerスクリプトをセット。")]
    [RequiredListLength(1, null)]
    public List<DoorOpenCloseController> door_open_close_controllers;


    public enum DoorStatus{
        Closed,
        Opened
    }
    public DoorStatus door_status;
    [Header("キーが必要なドアかどうか。")]
    public bool NeededKey;


    //プレイヤーが扉をロックする


    public bool OpenedClosedCompletedAnimation = true;
    public bool OpenedClosedCompleted = true;
    private PlayerStatus player_status;
    private TimeManager time_manager;
    private float CompletedWaitTimeNow;

    // [ShowIf("NeededKey")]
    [Header("鍵をかけたい場合はこちらを入れてください")]
    public OpenPermitByKeyController open_permit_by_key_controller;

    public DoorAttackedController door_attacked_controller;
    private void Start()
    {
        player_status = PlayerStatus.instance;
        time_manager = TimeManager.instance;
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
    public void DoorLock(){
        //ドアを強制的にロックする関数
        NeededKey = true;
        open_permit_by_key_controller.OpenPermit = false;
    }
    public void DoorOpenClosedChangeByStatus(GameObject EventActionMan)
    {
        //EventActionMan: この関数を行った実行人物(プレイヤーや敵など)
        //ドアをステータスによって変化させる。
        //空いている状態なら閉める。　開いている状態なら開ける。

        //鍵が必要なドアならまず鍵をこのドアに使ったかどうかを判定。 使ってなかったらreturn; 開けないよ。
        if (NeededKey)
        {
            //NeededKeyがtrueだけではロックされたことにはならない。
            if (!open_permit_by_key_controller.OpenPermit) {

                //鍵がかかっている
                if(EventActionMan.tag == "Player")
                {
                    open_permit_by_key_controller.NeedKeyForPlayer();
                }

                return;
            }
        }

        if(EventActionMan.tag == "Enemy")
        {
            //敵がプレイヤーを発見していたら、閉められた扉は壊して開けるようにする。
            DiscoveryPlayerController discovery_player_controller = EventActionMan.GetComponent<DiscoveryPlayerController>();
            EnemySoundSensor enemy_sound_sensor = EventActionMan.GetComponent<EnemySoundSensor>();
            if (discovery_player_controller.DiscoveryPlayerNow)
            {
                return;
            }

        }


        //開けてる閉めてるアニメーション再生中ならこのアクションを実行しない。
        if (!OpenedClosedCompleted) return;
        if (door_status == DoorStatus.Opened)
        {
            DoorsClose();
        }else if(door_status == DoorStatus.Closed)
        {
            DoorsOpen();
        }   
    }

    public bool DoorOpenPermitCheck()
    {
        return open_permit_by_key_controller.OpenPermit;
    }


    private void DoorsOpen()
    {
        this.OpenedClosedCompleted = false;
        this.OpenedClosedCompletedAnimation = false;
        foreach (DoorOpenCloseController door_open_close_controller in door_open_close_controllers)
        {
            door_open_close_controller.Open();
        }

        door_status = DoorStatus.Opened;
    }
    private void DoorsClose()
    {
        this.OpenedClosedCompleted = false;
        this.OpenedClosedCompletedAnimation = false;
        foreach (DoorOpenCloseController door_open_close_controller in door_open_close_controllers)
        {
            door_open_close_controller.Close();
        }

        door_status = DoorStatus.Closed;
    }

}
