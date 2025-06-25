using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollideredForPlayers : MonoBehaviour
{
    public List<string> TargetTags = new List<string>(){"Player"};
    [SerializeField]
    public UnityEvent OnTriggerEnterEvent;
    [SerializeField]
    public UnityEvent OnTriggerStayEvent;
    [SerializeField]
    public UnityEvent OnTriggerExitEvent;
    public List<GameObject> CollideredObjes = new List<GameObject>();
    // void Start()
    // {

    // }
    private void OnTriggerEnter(Collider other)
    {
        if (TargetTags.Contains(other.gameObject.tag))
        {
            OnTriggerEnterEvent.Invoke();
            if (!CollideredObjes.Contains(other.gameObject))
            {
                CollideredObjes.Add(other.gameObject);
            }
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (TargetTags.Contains(other.gameObject.tag))
        {
            OnTriggerStayEvent.Invoke();

        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (TargetTags.Contains(other.gameObject.tag))
        {
            OnTriggerExitEvent.Invoke();
            if (CollideredObjes.Contains(other.gameObject))
            {
                CollideredObjes.Remove(other.gameObject);
            }
        }
    }
}
