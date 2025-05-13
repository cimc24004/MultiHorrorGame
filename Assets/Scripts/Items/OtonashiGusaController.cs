using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtonashiGusaController : MonoBehaviour
{
    [Header("ƒvƒŒƒCƒ„[‚Ì‘«‰¹‚ğÁ‚·ŠÔ")]
    private InstantiateItemScript instantiate_item_script;
    private PlayerStatus player_status;
    private FootStepSoundController foot_step_sound_controller;

    private void Start()
    {
        instantiate_item_script = this.gameObject.GetComponent<InstantiateItemScript>();
        player_status = PlayerStatus.LocalInstance;
        foot_step_sound_controller = player_status.foot_step_sound_controller;
        OtonashiGusaUse();

    }
    public void OtonashiGusaUse()
    {
        Debug.Log("OtonashiGusaUse00------------");
        foot_step_sound_controller.FootStepCant(instantiate_item_script.item_status.ConsumableEffectTime);

    }
}
