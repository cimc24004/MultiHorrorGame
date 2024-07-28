using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyToPlayerKillController : MonoBehaviour
{
    private DiscoveryPlayerController discovery_player_controller;
    public LayerMask layer_mask;
    private GameObject Player;
    private PlayerStatus player_status;
    public Transform StartPos;
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
        if (!player_status.Dead)
        {
            if (discovery_player_controller.DiscoveryPlayerNow)
            {
                if (Physics.Raycast(StartPos.position, discovery_player_controller.direction, 0.3f, layer_mask))
                {
                    player_status.DeadEvent(this.gameObject);
                }
            }
        }
        Debug.DrawRay(StartPos.position, discovery_player_controller.direction * 0.3f, Color.red);

    }

}
