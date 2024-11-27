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

    [SerializeField] private AudioSource HimoAudioSource;
    [SerializeField] private AudioSource LightOnAudioSource;
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
            HimoAudioSource.Play();
            LightOnAudioSource.Play();
            turn_status = TurnStatus.TurnOn;
        }
        else
        {
            HimoAudioSource.Play();

            animator.SetTrigger("TurnOff");
            turn_status = TurnStatus.TurnOff;
        }
    }
}
