using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SisenAsobiDousiController : MonoBehaviour
{
    private DiscoveryPlayerController discovery_player_controller;
    private FollowObjectByNav follow_object_by_nav;
    private EnemyRoutingController enemy_routing_controller;
    private HeadLookatTargetController head_look_at_target_controller;
    [SerializeField]
    private SkinnedMeshRenderer HumanKaradaMeshRenderer;
    [SerializeField]
    private AudioSource ScreamAudioSource;
    [SerializeField]
    private bool MouthOpenNow;
    [SerializeField]
    private float LookNowTimeNow = 0;
    [SerializeField]
    private float MouthOpenTimeNow = 0;
    private Vector3 BeforePosition;
    private EnemyStatus enemy_status;
    private EnemyToPlayerKillController enemy_to_player_kill_controller;
    private CharacterController character_controller;
    private bool _once_look;
    // Start is called before the first frame update
    void Start()
    {
        discovery_player_controller = this.gameObject.GetComponent<DiscoveryPlayerController>();
        head_look_at_target_controller = this.gameObject.GetComponent<HeadLookatTargetController>();
        enemy_routing_controller = this.gameObject.GetComponent<EnemyRoutingController>();
        BeforePosition = this.transform.position;
        enemy_to_player_kill_controller = this.gameObject.GetComponent<EnemyToPlayerKillController>();
        enemy_status = this.gameObject.GetComponent<EnemyStatus>();
        character_controller = this.gameObject.GetComponent<CharacterController>();
        follow_object_by_nav = this.gameObject.GetComponent<FollowObjectByNav>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!discovery_player_controller.DiscoveryPlayerNow)
        {
            if (!MouthOpenNow)
            {
                if (head_look_at_target_controller.LookAtTargetNow)
                {
                    //‹ß‚­‚É“G‚ª‚¢‚é‚È‚ç ‚Æ‚¢‚¤”»’è‚Íhead_look_at_target_controller‚ªtarget‚ðŒ©‚Ä‚é‚©‚Ç‚¤‚©‚Å”»’è‚·‚éB
                    //follow_object_by_nav.ChaseToTargetSet(true, BeforePosition, 0);
                    enemy_routing_controller.PermitFunction(false);
                    LookNowTimeNow += Time.deltaTime;
                    enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Stop;
                    enemy_to_player_kill_controller.enabled = false;
                    character_controller.Move(Vector3.zero);
                    follow_object_by_nav.MovePermit = false;
                    //Œ©‚Ä‚¢‚éŠÔ‚ÌŽžŠÔ‚ª‰½•b‚©Œo‚Á‚½‚ç
                    if (LookNowTimeNow > 1)
                    {
                        //Œû‚ðŠJ‚¯‚é Œû‚ðŠJ‚¯‚étrue‚É
                        MouthOpenNow = true;
                        HumanKaradaMeshRenderer.SetBlendShapeWeight(0, 200);
                        ScreamAudioSource.Play();
                        LookNowTimeNow = 0;
                    }
                    _once_look = false;
                }
                else
                {
                    if (!_once_look)
                    {
                        enemy_routing_controller.PermitFunction(true);
                        follow_object_by_nav.MovePermit = true;
                        LookNowTimeNow = 0;
                        _once_look = true;
                    }

                }
            }
        }


        //Œû‚ªŠJ‚¢‚Ä‚¢‚étrue‚È‚ç ‰½•b‚©Œo‚Á‚½Œã‚ÉP‚Á‚Ä‚­‚éB
        if (MouthOpenNow)
        {
            MouthOpenTimeNow += Time.deltaTime;
            if(MouthOpenTimeNow > 1)
            {
                enemy_routing_controller.PermitFunction(true);
                HumanKaradaMeshRenderer.SetBlendShapeWeight(0, 0);
                enemy_to_player_kill_controller.enabled = true;
                follow_object_by_nav.MovePermit = true;
                enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Walk;

                discovery_player_controller.DiscoveryPlayerNow = true;

                MouthOpenNow = false;
                MouthOpenTimeNow = 0;
            }else
            {
                follow_object_by_nav.MovePermit = false;
                enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Stop;
            }

        }
    }
}
