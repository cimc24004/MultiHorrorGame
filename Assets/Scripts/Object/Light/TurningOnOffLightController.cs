using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public class TurningOnOffLightController : MonoBehaviour
{
    public enum TurnStatus
    {
        TurnOff,
        TurnOn
        
    }
    public TurnStatus turn_status;
    public GameObject LightObj;
    private Animator animator;
    private void Start()
    {
        animator = this.gameObject.GetComponent<Animator>();
    }
    [Button]
    public void TurnOnOffByStatus()
    {
        if(turn_status == TurnStatus.TurnOff)
        {
            animator.SetTrigger("TurnOn");

            turn_status = TurnStatus.TurnOn;
        }
        else
        {
            animator.SetTrigger("TurnOff");
            turn_status = TurnStatus.TurnOff;
        }
    }
}
