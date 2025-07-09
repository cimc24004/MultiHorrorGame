using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Collidered : MonoBehaviour
{
    public List<string> TargetTags;
    public bool Collider;
    public GameObject CollideredObj;
    [SerializeField]
    public UnityEvent OnTriggerEnterEvent;
    [SerializeField]
    public UnityEvent OnTriggerStayEvent;
    [SerializeField]
    public UnityEvent OnTriggerExitEvent;

    void Start()
    {
        PlayerStatus.OnLocalPlayerDeaded += OnlocalPlayerDeaded;
    }
    private void OnlocalPlayerDeaded(PlayerStatus playerStatus)
    {
        if (CollideredObj == playerStatus.gameObject)
        {
            TriggerExitEvent(playerStatus.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (TargetTags.Contains(other.gameObject.tag))
        {
            TriggerEnterEvent(other.gameObject);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (TargetTags.Contains(other.gameObject.tag))
        {
            TriggerStayEvent(other.gameObject);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (TargetTags.Contains(other.gameObject.tag))
        {
            TriggerExitEvent(other.gameObject);
        }
    }
    private void TriggerEnterEvent(GameObject obj)
    {
        OnTriggerEnterEvent.Invoke();
        CollideredObj = obj;
        Collider = true;
    }
    private void TriggerStayEvent(GameObject obj)
    {
        OnTriggerStayEvent.Invoke();
        CollideredObj = obj;
        Collider = true;
    }
    private void TriggerExitEvent(GameObject obj)
    {
        OnTriggerExitEvent.Invoke();
        CollideredObj = null;
        Collider = false;
    }
}
