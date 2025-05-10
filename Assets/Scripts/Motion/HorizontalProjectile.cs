using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
public class HorizontalProjectile : SerializedMonoBehaviour
{
    //�������˂��s�����߂̃X�N���v�g�B
    public bool throw_now;

    private Rigidbody rigid;
    //�����Ă���n�ʂɂ������B
    public bool OnGround;
    //�����Ă���n�ʂɂ����Ƃ���Unity�C�x���g
    public UnityEvent OnGroundEvent;

    //Enemy�Ƀq�b�g�����Ƃ��ɉ����������邩�ǂ���
    [SerializeField] private bool InstantiateOnEnemyHit;
    [ShowIf("InstantiateOnEnemyHit")]
    [SerializeField] private GameObject InstantiatePrefabWhenEnemyHit;
    //�����x
    public float FirstSpeed = 1;

    [Header("�Ώ̂̃��C���[���w��")]
    public LayerMask col_layer;

    private Vector3 normal;
    [SerializeField]
    private GameObject hited_obj;

    private GameObject InstantiateObjWhenHitedEnemy;
    private ItemObject item_object;
    // Start is called before the first frame update
    void Start()
    {
        item_object = this.gameObject.GetComponent<ItemObject>();


    }

    // Update is called once per frame
    void Update()
    {
        //�������Ƃ��ɂ������s����Ȃ��悤�ɂ��Ă����B
        if (OnGround && throw_now)
        {
            //�n�ʂɓ��������Ƃ��̓I�u�W�F�N�g������Ȃ��悤�ɐÎ~�������̂�isKinematic=true�ɂ���B
            Debug.DrawRay(this.transform.position,normal * 10, Color.blue);
            if ((col_layer.value & (1 << hited_obj.gameObject.layer)) != 0)
            {
                if (Vector3.Dot(Vector3.up, normal) >= 0.5f)
                {

                    //�n�ʂɏՓ˂����ۂ�iskinematic = true�ɂ���B
                    OnGroundEvent.Invoke();
                    rigid.isKinematic = true;
                    OnGround = false;
                }
            }

        }

    }

    public void Throw(float _FirstSpeed, Vector3 _forward)
    {
        //���𐅕����˂œ�����֐��B ItemThrowController�Ȃǂ�����s����B
        this.gameObject.isStatic = false;
        Debug.Log("throwwwwwwwwwwwwwwwwwwwwwwwwwwwww");
        rigid = this.gameObject.GetComponent<Rigidbody>();
        rigid.isKinematic = false;
        this.FirstSpeed = _FirstSpeed;

        //�����x��_forward�ݒ肵�čŏ��������x���Ă�����ΐ������˂ɂȂ�.
        throw_now = true;
        rigid.linearVelocity = _forward * _FirstSpeed;
    }
    private void OnCollisionEnter(Collision other)
    {
        Debug.DrawRay(this.gameObject.transform.position, other.contacts[0].normal * 10, Color.green);
        hited_obj = other.gameObject;
        if (throw_now)
        {
            //�n�ʂɂԂ�������
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
