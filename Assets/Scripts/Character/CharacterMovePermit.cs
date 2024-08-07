using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class CharacterMovePermit : MonoBehaviour
{
    public UnityEvent MoveEvent;
    public UnityEvent StopEvent;
    private CharacterController characterController;
    // Start is called before the first frame update
    void Start()
    {
        characterController = this.gameObject.GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Move()
    {
        MoveEvent.Invoke();
    }
    public void Stop()
    {
        StopEvent.Invoke();
        //rigid.velocity = Vector3.zero;
        characterController.Move(new Vector3(0, characterController.velocity.y, 0));
    }
}
