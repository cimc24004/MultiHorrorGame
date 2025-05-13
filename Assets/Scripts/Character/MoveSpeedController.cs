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
    
    // public override void OnNetworkSpawn()
    // {

    // }
    private void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InitWhenPlayerSpawned;
    }
    private void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitWhenPlayerSpawned;
    }
    private void InitWhenPlayerSpawned(PlayerStatus _playerStatus){
        enemy_all_controller = EnemyAllController.Instance;
        Debug.Log("enemy_all_controller: " + enemy_all_controller);
        enemy_all_controller.enemy_move_speed_controllers.Add(this);
        m_Animator = this.gameObject.GetComponent<Animator>();    
    }
    public void MoveSpeedUpAndAnimSpeedUp(float _runSpeed, float _walkSpeed, float _animationSpeed){
        this.RunSpeed += _runSpeed;
        this.WalkSpeed += _walkSpeed;
        m_Animator.speed += _animationSpeed;
    }
    public override void OnNetworkDespawn()
    {
        enemy_all_controller.enemy_move_speed_controllers.Remove(this);
    }
}
