using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibleThroughTheWallController : MonoBehaviour
{
    public bool Visible;
    public bool VisibleOnce;
    private MaterialChangeController material_change_controller;
    private bool Wall;
    private GameObject MainCamera;
    private Ray ray;
    private Vector3 direction;
    private RaycastHit raycast_hit;
    private GameObject CameraRoot;
    private PlayerStatus player_status;
    // Start is called before the first frame update
    void Start()
    {
        material_change_controller = this.gameObject.GetComponent<MaterialChangeController>();
        MainCamera = Camera.main.gameObject;
        player_status = PlayerStatus.instance;
        CameraRoot = player_status.CameraRoot.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        direction = CameraRoot.transform.position - this.transform.position;
        if (Visible)
        {
            if (Physics.Raycast(this.gameObject.transform.position, direction, out raycast_hit, Mathf.Infinity))
            {
                if(raycast_hit.collider.gameObject.tag == "Player")
                {
                    material_change_controller.MaterialChange = true;
                }
            }
        }

        if (VisibleOnce)
        {
            if (Physics.Raycast(this.gameObject.transform.position, direction, out raycast_hit, Mathf.Infinity))
            {
                if (raycast_hit.collider.gameObject.tag == "Player")
                {
                }
                else
                {
                    material_change_controller.MaterialChange = true;
                }
            }
            VisibleOnce = false;

        }
    }
}
