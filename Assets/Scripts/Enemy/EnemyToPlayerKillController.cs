using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyToPlayerKillController : MonoBehaviour
{
    //Enemyにつけるスクリプト。 見つかっている状態でプレイヤーに触れたらプレイヤーを殺す。（プレイヤーを見ている状態で。）
    private DiscoveryPlayerController discovery_player_controller;
    //ObjectもRaycast判定にしないと壁を貫通して殺す判定になっちゃうのでObjectとかDoorとかも追加しておく。
    public LayerMask layer_mask;
    private GameObject Player;
    private PlayerStatus player_status;
    private RaycastHit raycast_hit;
    //Rayを飛ばすスタートPosition
    public Transform StartPos;
    public float RaycastDistance = 1;
    private Vector3 Direction;
    // Start is called before the first frame update
    void Start()
    {
        Player = PlayerStatus.instance.gameObject;
        player_status = PlayerStatus.instance;
        discovery_player_controller = this.gameObject.GetComponent<DiscoveryPlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        Direction = (player_status.HeadPos.position - StartPos.position).normalized;
        if (!player_status.Dead)
        {
            if (discovery_player_controller.DiscoveryPlayerNow)
            {
                //プレイヤーが死んでいないかつプレイヤーが見つかっていたら。
                if (Physics.Raycast(StartPos.position, Direction, out raycast_hit, RaycastDistance, layer_mask))
                {
                    //敵の目からのレイキャストを飛ばす。　プレイヤーの方向を見てるときだけ殺せるようにしておく。
                    if(raycast_hit.collider.gameObject.tag == "Player")
                    {
                        player_status.DeadEvent(this.gameObject);
                    }
                }
            }
        }
        Debug.DrawRay(StartPos.position, Direction * RaycastDistance, Color.red);

    }

}
