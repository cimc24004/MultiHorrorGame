using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscoveryPlayerController : MonoBehaviour
{
    public bool DiscoveryPlayerMode;

    public List<string> ObstacleLayers;
    public Transform EyePosition;
    [Header("視野角")]
    public float VisionFieldAngle;
    [Header("Playerを見つけられる範囲")]
    public float PlayerDiscoveryRange;

    //特定のレイヤーオブジェクトのみで視野判定を行っていくための設定
    public List<string> layer_mask_lists = new List<string>{ "Player", "Object" };
    private int layerMask;
    private GameObject Player;
    private Transform PlayersHeadTransform;
    private RaycastHit raycast_hit;
    //プレイヤーが見つけられる範囲以内にいるかどうか。
    private bool InsidePlayer;
    private Vector3 direction;
    // Start is called before the first frame update
    void Start()
    {
        Player = PlayerController.instance.gameObject;
        PlayersHeadTransform = PlayerController.instance.Head;
        layerMask = LayerMask.GetMask(layer_mask_lists.ToArray());
    }

    // Update is called once per frame
    void Update()
    {
        direction = (PlayersHeadTransform.transform.position - EyePosition.transform.position).normalized;

        if (Vector3.Distance(Player.transform.position, this.transform.transform.position) <= PlayerDiscoveryRange)
            InsidePlayer = true;
        else
            InsidePlayer = false;

        if (Vector3.Angle(direction, EyePosition.forward) <= VisionFieldAngle && InsidePlayer)
        {
            //プレイヤーが敵の視界以内にいるかどうか && プレイヤーが範囲以内にいるかどうか。
            if(Physics.Raycast(EyePosition.transform.position, direction, out raycast_hit,layerMask))
            {
                //Raycastをプレイヤーにとりあえず飛ばす。

                if (!ObstacleLayers.Contains(LayerMask.LayerToName(raycast_hit.collider.gameObject.layer)))
                {
                    //プレイヤーにRaycastが当たっているが、まずその前に障害物が存在する場合は見つからなかった判定にしておく。
                    if (raycast_hit.collider.gameObject.tag == "Player")
                    {
                        Debug.Log("Playerが敵に見つかりました！");
                    }
                }
            }
            Debug.DrawRay(EyePosition.transform.position, direction * 1000, Color.red);
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(EyePosition.transform.position, (Quaternion.Euler(0, -VisionFieldAngle, 0) * EyePosition.forward) * 10);
        Gizmos.DrawRay(EyePosition.transform.position, (Quaternion.Euler(0, VisionFieldAngle, 0) * EyePosition.forward) * 10);
        Gizmos.DrawCube(this.transform.position + (Vector3.up * 0.1f), new Vector3(PlayerDiscoveryRange, 0.01f, PlayerDiscoveryRange));
    }
}
