using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurseDecreaseController : MonoBehaviour
{
    private PlayerStatus player_status;
    private GameObject CrossHairCanvas;
    private ChargeCircleGageController charge_circle_gage_controller;
    private CurseController curse_controller;
    [SerializeField] private float CurseDecreaseAmount = 10;
    // Start is called before the first frame update
    void Start()
    {
        player_status = PlayerStatus.instance;
        charge_circle_gage_controller = this.gameObject.GetComponent<ChargeCircleGageController>();
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
        curse_controller.CurseDecrease(CurseDecreaseAmount);
    }
}
