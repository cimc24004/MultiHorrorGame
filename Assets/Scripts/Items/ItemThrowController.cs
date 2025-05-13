using System.Collections;
using System.Collections.Generic;
using MathNet.Numerics.Optimization.TrustRegion;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemThrowController : MonoBehaviour
{
    //なにか物を投げる為のスクリプト。 

    //↓仮に投げるアイテムをセットしてるだけ。
    public GameObject MainCamera;
    public PlayerInput player_input;
    public Transform StartFiringPos;
    public float ThrowFirstSpeed = 1;
    public ItemStatus item_status;
    private PlayerStatus player_status;
    private bool InitPlayer;
    void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InitOnPlayerSpawn;
    }
    void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitOnPlayerSpawn;
    }
    private void InitOnPlayerSpawn(PlayerStatus _playerStatus){
        player_status = _playerStatus;
        player_input = player_status.player_input;
        MainCamera = player_status.virtual_camera.VirtualCameraGameObject;
        InitPlayer = true;
    }
    // Update is called once per frame
    void Update()
    {
        if(!InitPlayer) return;
        //プレイヤーのコライダーにぶつかって上手く投げることができないので
        //このItemThrowConrollerの子要素にあるオブジェクトから発射することで上手く放てる
        
        //Cameraの向いてる方向に向かって 自分も向くようにする。
        this.transform.forward = new Vector3(MainCamera.transform.forward.x, MainCamera.transform.forward.y, MainCamera.transform.forward.z);
        

    }
    public void ThrowStart(GameObject throw_obj)
    {
        //ItmeMenuController.csから実行する。
        //ThrowForward => 投げる方向のこと。　今回は自分の方向を設定している。(Cameraの向きを参照している。)

        //投げるためのオブジェクト(Prefab)を生成。
        GameObject obj = Instantiate(throw_obj, StartFiringPos.position, Quaternion.identity);
        //水平投射を行う為スクリプトを参照しの初速度を設定して発射。
        HorizontalProjectile horizontal_projectile = obj.GetComponent<HorizontalProjectile>();
        horizontal_projectile.Throw(ThrowFirstSpeed, this.transform.forward);

    }

}
