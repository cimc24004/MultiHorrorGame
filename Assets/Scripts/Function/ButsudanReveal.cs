using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class ButsudansReveal : NetworkBehaviour
{
    public float ButsudansRevealTime;
    private HousesController houses_controller;
    private PlayerStatus player_status;
    private SubNoticedUIController sub_noticed_ui_controller;
    void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InitOnPlayerSpawn;
    }
    void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitOnPlayerSpawn;
    }
    private void InitOnPlayerSpawn(PlayerStatus _playerStatus){
        player_status = _playerStatus;
        sub_noticed_ui_controller = player_status.sub_noticed_ui_controller;
    }
    public override void OnNetworkSpawn()
    {
        houses_controller = HousesController.Instance;
    }

    public void RevealButsudansAction()
    {
        RevealButsudansServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RevealButsudansServerRpc(ServerRpcParams rpcParams = default)
    {
        RevealButsudansClientRpc();
    }

    [ClientRpc]
    private void RevealButsudansClientRpc(ClientRpcParams rpcParams = default)
    {
        sub_noticed_ui_controller?.Notice("仏壇の位置が明らかになった..!!");
        houses_controller?.RevealButusdans(ButsudansRevealTime);
        sub_noticed_ui_controller?.Notice(
            "仏壇の位置がわからなくなった....",
            ButsudansRevealTime
        );
    }
}
