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
    private UnityEvent OnTriggerEvent;
    [SerializeField]
    private UnityEvent OnTriggerExitEvent;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerStay(Collider other)
    {
        if (TargetTags.Contains(other.gameObject.tag))
        {
            OnTriggerEvent.Invoke();
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
