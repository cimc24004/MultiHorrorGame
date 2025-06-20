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
        item_effect_controller = GameController.instance.gameObject.GetComponent<ItemEffectController>();
        instantiate_item_script = this.gameObject.GetComponent<InstantiateItemScript>();
        //クライアントから実行される。
        item_effect_controller.SenkouUseRequestServerRpc(instantiate_item_script.item_status.ConsumableEffectTime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
