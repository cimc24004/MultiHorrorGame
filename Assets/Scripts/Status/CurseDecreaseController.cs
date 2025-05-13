using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Netcode;
public class CurseDecreaseController : NetworkBehaviour
{
    private PlayerStatus player_status;
    private GameObject CrossHairCanvas;
    private CurseController curse_controller;
    [SerializeField] private float CurseDecreaseAmount = 10;
    // Start is called before the first frame update
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
        CrossHairCanvas = player_status.cross_hair_event.gameObject;
    }
    public override void OnNetworkSpawn()
    {
        curse_controller = CurseController.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CurseDecrease()
    {
        Debug.Log("CurseDecrease!!!!!!!!!!!!!!!!!!!!!!");
        if(curse_controller.CurseMaxedEventNow){
            curse_controller.CurseMaxedReset();
        }
        player_status.sub_noticed_ui_controller.Notice("呪い値が下がった");
        curse_controller.CurseDecrease(CurseDecreaseAmount);
    }
}
