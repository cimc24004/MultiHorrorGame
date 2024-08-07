using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemThrowController : MonoBehaviour
{
    //なにか物を投げる為のスクリプト。 

    //↓仮に投げるアイテムをセットしてるだけ。
    public GameObject itemPrefab;
    public GameObject camera;
    public PlayerInput player_input;
    public Transform StartFiringPos;
    public float ThrowFirstSpeed = 1;
    public ItemStatus item_status;

   
    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        //プレイヤーのコライダーにぶつかって上手く投げることができないので
        //このItemThrowConrollerの子要素にあるオブジェクトから発射することで上手く放てる
        
        //Cameraの向いてる方向に向かって 自分も向くようにする。
        this.transform.forward = new Vector3(camera.transform.forward.x, camera.transform.forward.y, camera.transform.forward.z);
        

    }
    public void ThrowStart(GameObject throw_obj)
    {
        //ItmeMenuController.csから実行する。
        //ThrowForward => 投げる方向のこと。　今回は自分の方向を設定している。(Cameraの向きを参照している。)

        //投げるためのオブジェクト(Prefab)を生成。
        GameObject obj = Instantiate(throw_obj, StartFiringPos.position, Quaternion.identity);
        //水平投射を行う為スクリプトを参照しの初速度を設定して発射。
        HorizontalProjectile horizontal_projectile = obj.GetComponent<HorizontalProjectile>();
        horizontal_projectile.item_throw_controller = this;
        horizontal_projectile.Throw(ThrowFirstSpeed, this.transform.forward);

    }
    public void EnemyThrowCollisionEvent(GameObject HitedEnemy)
    {
        //敵にアイテムが当たったときのイベント
        if (item_status.MakingStopEnemyItem)
        {
            //敵を止めることができるアイテムだったら？
            HitedEnemy.GetComponent<EnemyStatus>().StopEnemyByPlayer(item_status.status_of_making_stop_enemy.MakingStopTime);
            
        }
    }
}
