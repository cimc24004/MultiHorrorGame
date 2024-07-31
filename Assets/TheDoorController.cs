using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public class TheDoorController : SerializedMonoBehaviour
{
    [Header("ドアごとについているDoorOpenCloseControllerスクリプトをセット。")]
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
    public bool OpenedClosedCompletedAnimation = true;
    public bool OpenedClosedCompleted = true;

    private float CompletedWaitTimeNow;
    private void Start()
    {
    }
    private void Update()
    {
        if (OpenedClosedCompletedAnimation)
        {
            CompletedWaitTimeNow += TimeManager.instance.deltaTime;
            if(CompletedWaitTimeNow > 0.3f)
            {
                OpenedClosedCompleted = true;
                CompletedWaitTimeNow = 0;
            }
        }

    }
    public void DoorOpenClosedChangeByStatus()
    {
        //ドアをステータスによって変化させる。
        //空いている状態なら閉める。　開いている状態なら開ける。
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
