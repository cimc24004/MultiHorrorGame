using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class DoorAttackedController : MonoBehaviour
{
    //扉が壊れる条件(3回で壊れる。)
    [SerializeField] private float need_break_count = 3.0f;
    [SerializeField] private float break_count_now = 0;

    public List<GameObject> Doors;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public bool DoorAttacked()
    {
        Debug.Log("扉を攻撃しています: " + break_count_now);
        break_count_now += 1;
        if (need_break_count <= break_count_now)
        {
            //扉が完全に壊れました。
            Debug.Log("扉が完全に壊れた");
            foreach(GameObject door in Doors)
            {
                Rigidbody rigid_door = door.gameObject.AddComponent<Rigidbody>();
                rigid_door.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                Collider door_collider = door.gameObject.GetComponent<Collider>();
                door.gameObject.layer = LayerMask.NameToLayer("WithoutCharacter");
                if(door != null && rigid_door != null)
                {
                    door.transform.Rotate(30 ,30 ,30 );
                }
            }
            //this.gameObject.SetActive(false);
            return true;
        }
        else
        {
            //ドアを攻撃している間
            return false;
        }
    }
}
