using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Sirenix.OdinInspector;
public class CameraShakeController : SerializedMonoBehaviour
{
    private CinemachineImpulseSource cinemachine_impulse_source;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [Button]
    public void Shake(){
        Debug.Log("Shaked-------------------------------Impusle!");
        cinemachine_impulse_source = this.gameObject.GetComponent<CinemachineImpulseSource>();
        cinemachine_impulse_source.GenerateImpulse();
    }
}
