using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibledEnemyController : MonoBehaviour
{
    private FollowObjectByNav follow_object_by_nav;
    private CharacterController character_controller;
    private PlayerStatus player_status;
    private Camera MainCamera;

    public bool isVisible;
    // Start is called before the first frame update
    void Start()
    {
        player_status = PlayerStatus.instance;
        MainCamera = Camera.main;
        follow_object_by_nav = this.gameObject.GetComponent<FollowObjectByNav>();
        character_controller = this.gameObject.GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (IsVisibleFromCamera(MainCamera))
        {
            isVisible = true;
        }
        else
        {
            isVisible = false;
        }
        if (isVisible)
        {
            follow_object_by_nav.enabled = false;
            follow_object_by_nav.speed = 0;
            character_controller.Move(Vector3.zero);
        }
        else
        {
            follow_object_by_nav.enabled = true;
        }
    }
    bool IsVisibleFromCamera(Camera cam)
    {
        Vector3 viewportPos = cam.WorldToViewportPoint(transform.position);
        bool isInFront = viewportPos.z > 0;
        bool isInsideX = viewportPos.x > 0.05f && viewportPos.x < 0.95f;
        bool isInsideY = viewportPos.y > 0.05f && viewportPos.y < 0.95f;

        return isInFront && isInsideX && isInsideY;
    }



}
