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
    //投げてから敵に触れたときのUnityイベント
    public UnityEvent HitEnemyEvent;
    //初速度
    public float FirstSpeed = 1;

    public ItemThrowController item_throw_controller;
    [Header("対称のレイヤーを指定")]
    public LayerMask col_layer;

    private Vector3 normal;

    private GameObject hited_obj;
    // Start is called before the first frame update
    //void Start()
    //{
    //}

    // Update is called once per frame
    void Update()
    {
        //投げたときにしか実行されないようにしておく。
        if (OnGround && throw_now)
        {
            //地面に到着したときはオブジェクトが滑らないように静止したいのでisKinematic=trueにする。
            Debug.DrawRay(this.transform.position,normal * 10, Color.blue);
            if ((col_layer.value & (1 << hited_obj.gameObject.layer)) != 0)
            {
                if (Vector3.Dot(Vector3.up, normal) >= 0.5f)
                {

                    //地面に衝突した際はiskinematic = trueにする。
                    OnGroundEvent.Invoke();
                    rigid.isKinematic = true;
                    OnGround = false;
                }
            }

        }

    }

    public void Throw(float _FirstSpeed, Vector3 _forward)
    {
        //物を水平投射で投げる関数。 ItemThrowControllerなどから実行する。
        this.gameObject.isStatic = false;
        Debug.Log("throwwwwwwwwwwwwwwwwwwwwwwwwwwwww");
        rigid = this.gameObject.GetComponent<Rigidbody>();
        rigid.isKinematic = false;
        this.FirstSpeed = _FirstSpeed;

        //初速度と_forward設定して最初だけ速度つけてあげれば水平投射になる.
        throw_now = true;
        rigid.velocity = _forward * _FirstSpeed;
    }
    private void OnCollisionEnter(Collision other)
    {
        Debug.DrawRay(this.gameObject.transform.position, other.contacts[0].normal * 10, Color.green);
        hited_obj = other.gameObject;
        if (throw_now)
        {
            if ((col_layer.value & (1 << hited_obj.gameObject.layer)) != 0 && hited_obj.gameObject.tag != "WindowBorder")
            {
                OnGround = true;
            }

            if (other.gameObject.tag == "Enemy")
            {
                item_throw_controller.EnemyThrowCollisionEvent(other.gameObject);
                HitEnemyEvent.Invoke();
            }
        }

    }
    private void OnCollisionStay(Collision collision)
    {
        if ((col_layer.value & (1 << collision.gameObject.layer)) != 0)
        {
            normal = collision.contacts[0].normal;
            hited_obj = collision.gameObject;
        }
    }
    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.layer == col_layer && throw_now)
        {
            hited_obj = null;
            OnGround = false;
        }
        if(other.gameObject.tag == "Enemy")
        {

        }
    }
}
