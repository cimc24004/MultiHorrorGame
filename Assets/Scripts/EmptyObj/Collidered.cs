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


    private void OnTriggerEnter(Collider other)
    {
        if (TargetTags.Contains(other.gameObject.tag))
        {
            OnTriggerEnterEvent.Invoke();
            CollideredObj = other.gameObject;
            Collider = true;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (TargetTags.Contains(other.gameObject.tag))
        {
            OnTriggerStayEvent.Invoke();
            CollideredObj = other.gameObject;
            Collider = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (TargetTags.Contains(other.gameObject.tag))
        {
            OnTriggerExitEvent.Invoke();
            CollideredObj = null;
            Collider = false;
        }
    }
}
