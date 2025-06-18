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
    private Material NormalBodyMaterial;
    [SerializeField]
    private Material ChasePlayerBodyMaterial;
    [SerializeField]
    private PlayerNearSensor player_near_sensor;
    [SerializeField]
    private SkinnedMeshRenderer HumanKaradaMeshRenderer;
    [SerializeField]
    private AudioSource ScreamAudioSource;
    [SerializeField]
    private bool MouthOpenNow;
    [SerializeField]
    private float LookNowTime = 2;
    [SerializeField]
    private float LookNowTimeNow = 0;
    [SerializeField]
    private float MouthOpenTimeNow = 0;

    private Vector3 BeforePosition;
    private EnemyStatus enemy_status;
    private EnemyToPlayerKillController enemy_to_player_kill_controller;
    private CharacterController character_controller;
    private bool _once_look;
    [SerializeField]
    private bool previousFlinchNow = false;
    [SerializeField]
    private bool PermitMouthOpen = true;

    private URPFxController urp_fx_controller;
    private EnemyFlinchController enemy_flinch_controller;

    private EnemySoundSensor enemy_sound_sensor;
    private Material BeforeHumanKaradaMaterial;
    // Start is called before the first frame update
    void Start()
    {
        discovery_player_controller = this.gameObject.GetComponent<DiscoveryPlayerController>();
        head_look_at_target_controller = this.gameObject.GetComponent<HeadLookatTargetController>();
        enemy_routing_controller = this.gameObject.GetComponent<EnemyRoutingController>();
        BeforePosition = this.transform.position;
        enemy_sound_sensor = this.gameObject.GetComponent<EnemySoundSensor>();
        enemy_to_player_kill_controller = this.gameObject.GetComponent<EnemyToPlayerKillController>();
        enemy_status = this.gameObject.GetComponent<EnemyStatus>();
        character_controller = this.gameObject.GetComponent<CharacterController>();
        follow_object_by_nav = this.gameObject.GetComponent<FollowObjectByNav>();
        urp_fx_controller = GameControllerOnAllScene.instance.gameObject.GetComponent<URPFxController>();
        enemy_flinch_controller = this.gameObject.GetComponent<EnemyFlinchController>();
        player_near_sensor.IsNearToKillEffect = false;
    }

    // Update is called once per frame
    void Update()
    {

        if (!previousFlinchNow && enemy_flinch_controller.FlinchNow)
        {
            ResetValue();
            PermitMouthOpen = false;
            head_look_at_target_controller.enabled = false;
        }else if(previousFlinchNow && !enemy_flinch_controller.FlinchNow)
        {
            PermitMouthOpen = true;
            enemy_routing_controller.PermitFunction(true);
            head_look_at_target_controller.enabled = true;
        }
        // ƒtƒŒ[ƒ€‚ÌÅŒã‚ÉA¡‰ñ‚Ì’l‚ð previousFlinchNow ‚ÉŠi”[‚µ‚ÄŽŸ‰ñ”äŠr‚ÉŽg‚¤
        previousFlinchNow = enemy_flinch_controller.FlinchNow;


        if (discovery_player_controller.PlayersVisibilityMapStatuesWithKey[discovery_player_controller.FirstKeyOfDiscoveriedPlayers].DiscoveryPlayerNow && !MouthOpenNow)
        {

            HumanKaradaMeshRenderer.SetBlendShapeWeight(0, 200);
        }
        if(MouthOpenNow || discovery_player_controller.PlayersVisibilityMapStatuesWithKey[discovery_player_controller.FirstKeyOfDiscoveriedPlayers].DiscoveryPlayerNow){
            if(BeforeHumanKaradaMaterial != ChasePlayerBodyMaterial){
                HumanKaradaMeshRenderer.material = ChasePlayerBodyMaterial;
            }
            BeforeHumanKaradaMaterial = ChasePlayerBodyMaterial;
        }else{
            if(!BeforeHumanKaradaMaterial != NormalBodyMaterial){
                HumanKaradaMeshRenderer.material = NormalBodyMaterial;
            }
            BeforeHumanKaradaMaterial = NormalBodyMaterial;
        }
        if (!PermitMouthOpen)
        {
            return;
        }
        if (!discovery_player_controller.PlayersVisibilityMapStatuesWithKey[discovery_player_controller.FirstKeyOfDiscoveriedPlayers].DiscoveryPlayerNow)
        {
            player_near_sensor.IsNearToKillEffect = false;
            if (!MouthOpenNow)
            {
                if (head_look_at_target_controller.LookAtTargetNow && discovery_player_controller.PlayersVisibilityMapStatuesWithKey[discovery_player_controller.FirstKeyOfDiscoveriedPlayers].DiscoveryPlayerNow)
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
                    if (LookNowTimeNow > LookNowTime)
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
                    HumanKaradaMeshRenderer.SetBlendShapeWeight(0, 0);
                    if (!_once_look)
                    {
                        enemy_routing_controller.PermitFunction(true);
                        follow_object_by_nav.MovePermit = true;
                        LookNowTimeNow = 0;
                        _once_look = true;
                    }

                }
            }
        }else{
        }


        //Œû‚ªŠJ‚¢‚Ä‚¢‚étrue‚È‚ç ‰½•b‚©Œo‚Á‚½Œã‚ÉP‚Á‚Ä‚­‚éB
        if (MouthOpenNow)
        {
            MouthOpenTimeNow += Time.deltaTime;
            if(MouthOpenTimeNow > 1)
            {
                enemy_routing_controller.PermitFunction(true);
                enemy_to_player_kill_controller.enabled = true;
                follow_object_by_nav.MovePermit = true;
                enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Walk;
                discovery_player_controller.PlayersVisibilityMapStatuesWithKey[discovery_player_controller.FirstKeyOfDiscoveriedPlayers].DiscoveryPlayerNow = true;
                urp_fx_controller.InteferecesChange(false);
                player_near_sensor.IsNearToKillEffect = true;
                MouthOpenNow = false;
                MouthOpenTimeNow = 0;
            }else
            {
                follow_object_by_nav.MovePermit = false;
                urp_fx_controller.InteferecesChange(true);
                enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Stop;
            }

        }
    }

    private void ResetValue()
    {
        MouthOpenNow = false;
        urp_fx_controller.InteferecesChange(false);
       // head_look_at_target_controller.LookAtTargetNow = false;
        enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Stop;
        LookNowTimeNow = 0;

        MouthOpenTimeNow = 0;
    }
}
