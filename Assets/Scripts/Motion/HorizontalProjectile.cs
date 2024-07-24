using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class HorizontalProjectile : MonoBehaviour
{
    //水平投射を行うためのスクリプト。
    public bool throw_now;

    private Rigidbody rigid;
    //投げてから地面についた時。
    public bool OnGround;
    //投げてから地面についたときのUnityイベント
    public UnityEvent OnGroundEvent;
    //初速度
    public float FirstSpeed = 1;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        //投げたときにしか実行されないようにしておく。
        if (OnGround && throw_now)
        {
            //地面に到着したときはオブジェクトが滑らないように静止したいのでisKinematic=trueにする。
            rigid.isKinematic = true;
        }
    }
    private void FixedUpdate()
    {
    }
    public void Throw(float _FirstSpeed, Vector3 _forward)
    {
        //物を水平投射で投げる関数。 ItemThrowControllerなどから実行する。

        rigid = this.gameObject.GetComponent<Rigidbody>();
        this.FirstSpeed = _FirstSpeed;

        //初速度と_forward設定して最初だけ速度つけてあげれば水平投射になる.
        throw_now = true;
        rigid.velocity = _forward * _FirstSpeed;
    }
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Ground" && throw_now)
        {
            OnGroundEvent.Invoke();
            OnGround = true;
        }
    }
    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.tag == "Ground" && throw_now)
        {
            OnGround = false;
        }
    }
}
