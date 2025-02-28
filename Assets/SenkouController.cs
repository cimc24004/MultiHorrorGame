using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SenkouController : MonoBehaviour
{
    private ItemEffectController item_effect_controller;
    private InstantiateItemScript instantiate_item_script;
    // Start is called before the first frame update
    void Start()
    {
        item_effect_controller = ItemEffectController.instance;
        instantiate_item_script = this.gameObject.GetComponent<InstantiateItemScript>();
        item_effect_controller.SenkouUse(instantiate_item_script.item_status.ConsumableEffectTime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
