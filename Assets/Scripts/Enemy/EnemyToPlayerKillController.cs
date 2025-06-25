using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Sirenix.OdinInspector;
public class EnemyToPlayerKillController : NetworkBehaviour
{
    public Transform boxCenter;                  // 範囲の中心（敵の目の前に置く）
    public Vector3   boxHalfExtents = new(0.3f, 0.6f, 0.4f);
    public LayerMask layerMask;    // Player レイヤーだけにしておく
    
    private DiscoveryPlayerController discovery_player_controller;
    private Collider[] hitBuffer = new Collider[4];   // 最大ヒット数を決め打ちで確保
    private GameNetworkController game_network_controller;
    private PlayerAllController player_all_controller;
    public override void OnNetworkSpawn()
    {
        game_network_controller = GameNetworkController.instance;
        player_all_controller = PlayerAllController.instance;
        discovery_player_controller = this.gameObject.GetComponent<DiscoveryPlayerController>();
    }

    void Update()
    {
        if(!IsServer) return;

        // Box の向きは敵の forward に合わせる
        Quaternion orientation = boxCenter.rotation;

        int hitCount = Physics.OverlapBoxNonAlloc(
            boxCenter.position,
            boxHalfExtents,
            hitBuffer,
            orientation,
            layerMask,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hitCount; i++)
        {
            if (hitBuffer[i].CompareTag("Player"))
            {
                //プレイヤーに触れたら
                GameObject player = hitBuffer[i].gameObject;
                PlayerStatus playerStatus = player_all_controller.SpawnedPlayersWithPlayerStatuses[player];
                if (playerStatus.Dead.Value || playerStatus.KillSequenceRunning.Value) continue;  // 既に死んでたらスキップ
                if (!playerStatus.Dead.Value && playerStatus.HidingNow.Value == false && discovery_player_controller.PlayersVisibilityMapStatuesWithKey[player].DiscoveryPlayerNow)
                {
                    //死にます。
                    discovery_player_controller.PlayersVisibilityMapStatuesWithKey[player].DiscoveryPlayerNow = false;
                    // discovery_player_controller.FirstKeyOfDiscoveriedPlayers = 
                    discovery_player_controller.RemovePlayerOfDiscoveryPlayerLists(player);

                    player_all_controller.SpawnedPlayersWithPlayerStatuses[player].DeadEvent(this.gameObject);
                }
                break;
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!boxCenter) return;
        Gizmos.color = Color.cyan;
        Matrix4x4 m = Matrix4x4.TRS(boxCenter.position, boxCenter.rotation, Vector3.one);
        Gizmos.matrix = m;
        Gizmos.DrawWireCube(Vector3.zero, boxHalfExtents * 2);
    }
#endif
}
