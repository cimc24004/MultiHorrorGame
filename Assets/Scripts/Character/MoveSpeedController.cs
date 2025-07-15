using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MoveSpeedController : NetworkBehaviour
{
    public float WalkSpeed;
    public float RunSpeed;
    private EnemyAllController enemy_all_controller;
    private Animator m_Animator;
    // Start is called before the first frame update
    void Start()
    {
        // enemy_all_controller = EnemyAllController.Instance;
        // enemy_all_controller.enemy_move_speed_controllers.Add(this);
        // m_Animator = this.gameObject.GetComponent<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("MoveSpeeDController Obj: " + this.gameObject.name);
        PlayerStatus.OnLocalPlayerSpawned += InitWhenPlayerSpawned;
        if (PlayerStatus.LocalInstance != null)
        {
            Debug.Log("PlayerStatus.LocalInstance != null: " + this.gameObject.name);
            InitWhenPlayerSpawned(PlayerStatus.LocalInstance);
        }
    }
    private void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitWhenPlayerSpawned;
    }
    private void InitWhenPlayerSpawned(PlayerStatus _playerStatus){
        Debug.Log("InitWhenPlayerSpawned: " + this.gameObject.name);
        Debug.Log($"InitWhenPlayerSpawned IsServer: {this.gameObject.name}  IsSErver={IsServer}");
        if (IsServer)
        {
            enemy_all_controller = EnemyAllController.instance;
            Debug.Log("enemy_all_controller: " + enemy_all_controller + "this.gameObject: " + this.gameObject.name);

            enemy_all_controller.enemy_move_speed_controllers.Add(this);
            m_Animator = this.gameObject.GetComponent<Animator>();
        }
    }
    public void MoveSpeedUpAndAnimSpeedUp(float _runSpeed, float _walkSpeed, float _animationSpeed){
        //サーバー側で実行される
        if (IsServer)
        {
            this.RunSpeed += _runSpeed;
            this.WalkSpeed += _walkSpeed;
            m_Animator.speed += _animationSpeed;
        }

    }
    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            Debug.Log("enemy_all_controller??: " + enemy_all_controller);
            Debug.Log("enemy_all_controller.enemy_move_speed_controllers??: " + enemy_all_controller.enemy_move_speed_controllers);

            if (enemy_all_controller.enemy_move_speed_controllers.Contains(this))
            {
                enemy_all_controller.enemy_move_speed_controllers.Remove(this);
            }
        }
    }
}
