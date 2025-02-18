using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SisenAsobiDousiController : MonoBehaviour
{
    private DiscoveryPlayerController discovery_player_controller;
    private HeadLookatTargetController head_look_at_target_controller;
    [SerializeField]
    private SkinnedMeshRenderer HumanKaradaMeshRenderer;
    [SerializeField]
    private AudioSource ScreamAudioSource;
    private bool MouthOpenNow;
    private float LookNowTimeNow = 0;
    private float MouthOpenTimeNow = 0;
    // Start is called before the first frame update
    void Start()
    {
        discovery_player_controller = this.gameObject.GetComponent<DiscoveryPlayerController>();
        head_look_at_target_controller = this.gameObject.GetComponent<HeadLookatTargetController>();
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
                    LookNowTimeNow += Time.deltaTime;
                    //Œ©‚Ä‚¢‚éŠÔ‚ÌŽžŠÔ‚ª‰½•b‚©Œo‚Á‚½‚ç
                    if (LookNowTimeNow > 1)
                    {
                        //Œû‚ðŠJ‚¯‚é Œû‚ðŠJ‚¯‚étrue‚É
                        MouthOpenNow = true;
                        HumanKaradaMeshRenderer.SetBlendShapeWeight(0, 200);
                        ScreamAudioSource.Play();
                        LookNowTimeNow = 0;
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
                HumanKaradaMeshRenderer.SetBlendShapeWeight(0, 0);
                discovery_player_controller.DiscoveryPlayerNow = true;
                MouthOpenNow = false;
                MouthOpenTimeNow = 0;
            }
        }
    }
}
