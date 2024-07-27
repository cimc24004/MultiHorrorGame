using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class CharactorMovePermit : MonoBehaviour
{
    public UnityEvent MoveEvent;
    public UnityEvent StopEvent;
    private Rigidbody rigid;
    // Start is called before the first frame update
    void Start()
    {
        rigid = this.gameObject.GetComponent<Rigidbody>();
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
        rigid.velocity = Vector3.zero;
    }
}
