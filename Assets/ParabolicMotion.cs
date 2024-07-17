using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParabolicMotion : MonoBehaviour
{
    private Rigidbody rigid;
    public float ForwardDistance = 10;

    public float FirstSpeed = 1;
    public float Angle = 30;
    // Start is called before the first frame update
    void Start()
    {
        rigid = this.gameObject.GetComponent<Rigidbody>();
        rigid.velocity = new Vector3(FirstSpeed * Mathf.Cos(Angle * Mathf.Deg2Rad) * this.transform.forward.x,FirstSpeed * Mathf.Sin(Angle * Mathf.Deg2Rad), FirstSpeed * Mathf.Cos(Angle * Mathf.Deg2Rad) * this.transform.forward.z);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void FixedUpdate()
    {
        //this.rigid.velocity = new Vector3(this.transform.forward.x * _speed, this.transform.forward.y * _speed, this.transform.forward.z * _speed);
    }
}
