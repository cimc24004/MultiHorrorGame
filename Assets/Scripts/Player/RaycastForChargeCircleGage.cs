using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastForChargeCircleGage : MonoBehaviour
{
    private RaycastHit raycast_hit;
    private GameObject RaycasstedObj;
    private ChargeCircleGageController charge_circle_gage_controller;
    private Ray main_ray;
    public LayerMask TargetLayerMaskViewToPoint;
    public float RaycastDistance = 3;

    Camera MainCamera;

    // Start is called before the first frame update
    void Start()
    {
        MainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        main_ray = MainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(main_ray, out raycast_hit, RaycastDistance, TargetLayerMaskViewToPoint))
        {
            if (raycast_hit.collider.gameObject.tag == "ChargeCircle")
            {
                if (RaycasstedObj != raycast_hit.collider.gameObject)
                {
                    charge_circle_gage_controller = raycast_hit.collider.gameObject.GetComponent<ChargeCircleGageController>();
                }
                Debug.Log("raycast_hit.collider.gameObject.tag == 'ChargeCircle'");
                charge_circle_gage_controller.UIShowTrigger();

            }
        }


    }
}
