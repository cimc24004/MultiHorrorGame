using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectLayerControl : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ChangeLayerByName(string layer_name)
    {
        this.gameObject.layer = LayerMask.NameToLayer(layer_name);
    }
}
