using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class LookAtPlayer : MonoBehaviour
{
    //プレイヤーを見る為だけのスクリプト。 
    PlayerStatus player_status;
    public enum PlayMode
    {
        Update,
        Start
    }
    public enum LookAt
    {
        Player,
        CameraRoot
    }
    public LookAt look_at;
    public PlayMode play_mode;
    private void Start()
    {
        player_status = PlayerStatus.instance;
        if(play_mode == PlayMode.Start)
        {
            if(look_at == LookAt.Player)
            {
                this.gameObject.transform.DODynamicLookAt(player_status.HeadPos.transform.position, 0f).SetLink(this.gameObject);
            }else if(look_at == LookAt.CameraRoot)
            {
                this.gameObject.transform.DODynamicLookAt(player_status.CameraRoot.transform.position, 0f).SetLink(this.gameObject);
            }

        }

    }
    private void LateUpdate()
    {
        //DotWeenで少しフェードしてLookAtする
        if(play_mode == PlayMode.Update)
        {
            if(look_at == LookAt.Player)
            {
                this.gameObject.transform.DODynamicLookAt(player_status.HeadPos.transform.position, 0.3f).SetLink(this.gameObject);
            }
            else if (look_at == LookAt.CameraRoot)
            {
                this.gameObject.transform.DODynamicLookAt(player_status.CameraRoot.transform.position, 0.3f).SetLink(this.gameObject);
            }
        }
    }
}
