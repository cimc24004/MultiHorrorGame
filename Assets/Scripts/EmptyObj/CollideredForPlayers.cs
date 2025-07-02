using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class CollideredForPlayers : MonoBehaviour
{
    public List<string> TargetTags = new List<string>() { "Player" };
    [SerializeField]
    public UnityEvent OnTriggerEnterEvent;
    [SerializeField]
    public UnityEvent OnTriggerStayEvent;
    [SerializeField]
    public UnityEvent OnTriggerExitEvent;
    public List<GameObject> CollideredObjes = new List<GameObject>();

    public UnityEvent<GameObject> OnCollideredAddedObjectEvent;
    public UnityEvent<GameObject> OnCollideredRemovedObjectEvent;

    // void Start()
    // {

    // }
    private void OnTriggerEnter(Collider other)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (TargetTags.Contains(other.gameObject.tag))
        {
            if (!CollideredObjes.Contains(other.gameObject))
            {
                CollideredObjesAdd(other.gameObject);
                OnTriggerEnterEvent.Invoke();
            }
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (TargetTags.Contains(other.gameObject.tag))
        {
            OnTriggerStayEvent.Invoke();

        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (TargetTags.Contains(other.gameObject.tag))
        {
            if (CollideredObjes.Contains(other.gameObject))
            {
                CollideredObjesRemove(other.gameObject);
                OnTriggerExitEvent.Invoke();
            }
        }
    }
    private void CollideredObjesAdd(GameObject _obj)
    {
        OnCollideredAddedObjectEvent.Invoke(_obj);
        CollideredObjes.Add(_obj);
    }
    private void CollideredObjesRemove(GameObject _obj)
    {
        OnCollideredRemovedObjectEvent.Invoke(_obj);
        CollideredObjes.Remove(_obj);
    }
}
