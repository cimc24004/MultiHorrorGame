using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class LookAtPlayer : MonoBehaviour
{
    PlayerStatus player_status;
    private void Start()
    {
        player_status = PlayerStatus.instance;
    }
    private void LateUpdate()
    {
        this.gameObject.transform.DODynamicLookAt(player_status.HeadPos.transform.position, 0.3f);
    }
}
