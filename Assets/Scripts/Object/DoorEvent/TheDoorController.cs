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
    public enum DoorType
    {
        Husuma,
        Shoji
    }

    public DoorType door_type;

    public enum DoorStatus{
        Closed,
        Opened
    }
    public DoorStatus door_status;
    [Header("キーが必要なドアかどうか。")]
    public bool NeededKey;

    [HideInInspector]
    public bool OpenedClosedCompletedAnimation = true;
    private bool OpenedClosedCompleted = true;
    private PlayerStatus player_status;
    private TimeManager time_manager;
    private NoticedUIController noticed_ui_controller;
    private float CompletedWaitTimeNow;
    [ShowIf("NeededKey")]
    [Required] public OpenPermitByKeyController open_permit_by_key_controller;
    private void Start()
    {
        player_status = PlayerStatus.instance;
        time_manager = TimeManager.instance;
        noticed_ui_controller = player_status.noticed_ui_controller;
    }
    private void Update()
    {
        if (OpenedClosedCompletedAnimation)
        {
            CompletedWaitTimeNow += time_manager.deltaTime;
            if(CompletedWaitTimeNow > 0.3f)
            {
                OpenedClosedCompleted = true;
                CompletedWaitTimeNow = 0;
            }
        }
    }
    public void DoorOpenClosedChangeByStatus(GameObject EventActionMan)
    {
        //EventActionMan: この関数を行った実行人物(プレイヤーや敵など)
        //ドアをステータスによって変化させる。
        //空いている状態なら閉める。　開いている状態なら開ける。

        //鍵が必要なドアならまず鍵をこのドアに使ったかどうかを判定。 使ってなかったらreturn; 開けないよ。
        if (NeededKey)
        {

            if (!open_permit_by_key_controller.OpenPermit) {
                //鍵がかかっている
                if(EventActionMan.tag == "Player")
                {
                    noticed_ui_controller.Notice("鍵がかかっている", NoticedUIController.NoticeStatus.NeedKey);
                }
                else if(EventActionMan.tag == "Enemy")
                {
                    //ドアを破壊するとかを作ればいい。
                }
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
    public void DoorOpenDOPermit()
    {
        open_permit_by_key_controller.OpenPermit = true;
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
