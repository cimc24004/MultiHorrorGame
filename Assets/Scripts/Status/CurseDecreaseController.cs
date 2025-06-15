using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
public class CurseDecreaseController : MonoBehaviour
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
    private void InitOnPlayerSpawn(PlayerStatus _playerStatus)
    {
        player_status = _playerStatus;
        CrossHairCanvas = player_status.cross_hair_event.gameObject;
        curse_controller = CurseController.Instance 
                    ?? FindObjectOfType<CurseController>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CurseDecrease()
    {

        if (curse_controller.CurseMaxedEventNow)
        {
            //呪い値がマックスならば、CurseMaxedEventをリセット！
            curse_controller.CurseMaxedReset();
        }
        player_status.sub_noticed_ui_controller.Notice("呪い値が下がった");
        curse_controller.CurseDecrease(CurseDecreaseAmount);
    }
}
