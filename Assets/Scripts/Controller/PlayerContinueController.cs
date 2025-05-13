using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;

public class PlayerContinueController : NetworkSingleton<PlayerContinueController>
{
    [Header("ÉvÉåÉCÉÑÅ[prefab")]
    private PlayerStatus player_status;
    private GameObject Player;
    [SerializeField]private Vector3 PlayerContinuePos;
    [SerializeField]private CinemachineVirtualCamera cinemachine_virtualcamera;
    [SerializeField] private int ContinueCount;
    private Animator player_anim;
    private CharacterMovePermit player_move_permit;
    private CharacterController player_character_controller;
    [SerializeField] Transform ContinueTransform;
    void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InitOnPlayerSpawn;
    }
    void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitOnPlayerSpawn;
    }

    private void InitOnPlayerSpawn(PlayerStatus _playerStatus){
        this.player_status = _playerStatus;
        Player = player_status.gameObject;
        PlayerContinuePos = Player.transform.position;
        player_move_permit = Player.GetComponent<CharacterMovePermit>();
        player_character_controller = Player.GetComponent<CharacterController>();
        player_anim = Player.GetComponent<Animator>();
    }
    public void Continue()
    {
        Player.transform.position = ContinueTransform.position;
        player_move_permit.Move();
        player_anim.SetBool("AppearentDeath", false);
        player_character_controller.enabled = true;
        Player.gameObject.layer = LayerMask.NameToLayer("Player");
        ContinueCount++;
        GameOverCheck();
    }

    private void GameOverCheck()
    {
        if(ContinueCount >= 4)
        {
            SceneManager.LoadScene("GameOver");
        }
    }
}
