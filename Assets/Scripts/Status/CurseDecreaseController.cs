using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurseDecreaseController : MonoBehaviour
{
    private PlayerStatus player_status;
    private GameObject CrossHairCanvas;
    private CurseController curse_controller;
    [SerializeField] private float CurseDecreaseAmount = 10;
    // Start is called before the first frame update
    void Start()
    {
        player_status = PlayerStatus.instance;
        CrossHairCanvas = player_status.cross_hair_event.gameObject;
        curse_controller = CurseController.instance;
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
