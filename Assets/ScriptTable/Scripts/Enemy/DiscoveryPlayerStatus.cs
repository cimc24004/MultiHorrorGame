using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DiscoveryPlayerStatus", menuName = "ScriptableObjects/Enemy/DiscoveryPlayerStatus")]
public class DiscoveryPlayerStatus : ScriptableObject
{
    [Header("視野角")]
    public float VisionFieldAngle;
    [Header("Playerを見つけられる範囲")]
    public float PlayerDiscoveryRange;
    [Header("視認できるオブジェクトのレイヤー")]
    public LayerMask layerMask;
    //特定のレイヤーオブジェクトのみで視野判定を行っていくための設定
    //public List<string> layer_mask_lists = new List<string>{ "Player", "Object" };
    //プレイヤーが自分の視界から外れている間の時間
    [Header("プレイヤーが敵の視界から外れている間の追いかけるのを諦めるまでの時間")]
    public float SightOutPlayerTime = 1;
}
