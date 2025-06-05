using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
public class HorizontalProjectile : SerializedMonoBehaviour
{
    // 投射物を投げている間 true
    public bool throw_now;

    private Rigidbody rigid;
    // 投射物が地面に付いているかどうか
    public bool OnGround;
    // 地面に着いたときに呼ばれるイベント
    public UnityEvent OnGroundEvent;

    // 敵にヒットした際にオブジェクトを生成するか
    [SerializeField] private bool InstantiateOnEnemyHit;
    [ShowIf("InstantiateOnEnemyHit")]
    [SerializeField] private GameObject InstantiatePrefabWhenEnemyHit;
    // 初速
    public float FirstSpeed = 1;

    [Header("対象レイヤー")]
    public LayerMask col_layer;

    private Vector3 normal;
    [SerializeField]
    private GameObject hited_obj;

    private GameObject InstantiateObjWhenHitedEnemy;
    private ItemObject item_object;
    // Start は最初のフレーム前に呼ばれる
    void Start()
    {
        item_object = this.gameObject.GetComponent<ItemObject>();


    }

    // 毎フレーム呼び出される
    void Update()
    {
        // 着地後は移動処理を止める
        if (OnGround && throw_now)
        {
            // 地面に刺さったときは物理挙動を止める
            Debug.DrawRay(this.transform.position,normal * 10, Color.blue);
            if ((col_layer.value & (1 << hited_obj.gameObject.layer)) != 0)
            {
                if (Vector3.Dot(Vector3.up, normal) >= 0.5f)
                {

                    // 着地したら isKinematic を true にする
                    OnGroundEvent.Invoke();
                    rigid.isKinematic = true;
                    OnGround = false;
                }
            }

        }

    }

    public void Throw(float _FirstSpeed, Vector3 _forward)
    {
        // ItemThrowController から呼ばれて投げる
        this.gameObject.isStatic = false;
        Debug.Log("throwwwwwwwwwwwwwwwwwwwwwwwwwwwww");
        rigid = this.gameObject.GetComponent<Rigidbody>();
        rigid.isKinematic = false;
        this.FirstSpeed = _FirstSpeed;

        // 指定方向へ速度を与える
        throw_now = true;
        rigid.linearVelocity = _forward * _FirstSpeed;
    }
    private void OnCollisionEnter(Collision other)
    {
        Debug.DrawRay(this.gameObject.transform.position, other.contacts[0].normal * 10, Color.green);
        hited_obj = other.gameObject;
        if (throw_now)
        {
            // 地面に当たったか確認する
            if ((col_layer.value & (1 << hited_obj.gameObject.layer)) != 0)
            {
                OnGround = true;
            }

            if(hited_obj.tag == "Enemy"){
                if(InstantiateOnEnemyHit){
                    InstantiateObjWhenHitedEnemy = Instantiate(InstantiatePrefabWhenEnemyHit, this.transform.position, Quaternion.identity);
                    ThrowedEnemyHitController throwed_enemy_hit_controller = InstantiateObjWhenHitedEnemy.GetComponent<ThrowedEnemyHitController>();
                    Debug.Log("ThrowedEnemyHitController? " + throwed_enemy_hit_controller);
                    throwed_enemy_hit_controller.HitedEnemy = hited_obj;
                    throwed_enemy_hit_controller.item_status = item_object.item_status;
                    throwed_enemy_hit_controller.EnemyThrowCollisionEvent();
                }
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
