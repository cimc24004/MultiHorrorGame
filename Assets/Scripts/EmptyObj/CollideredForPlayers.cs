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
    void Start()
    {
        PlayerStatus.OnLocalPlayerDeaded += OnlocalPlayerDeaded;
    }
    private void OnlocalPlayerDeaded(PlayerStatus playerStatus) {
        if (CollideredObjes.Count != 0)
        {
            if (CollideredObjes.Contains(playerStatus.gameObject))
            {
                TriggerExitEvent(playerStatus.gameObject);
            }
        }

    }
    private void OnTriggerEnter(Collider other)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (TargetTags.Contains(other.gameObject.tag))
        {
            if (!CollideredObjes.Contains(other.gameObject))
            {
                TriggerEnterEvent(other.gameObject);
            }
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (TargetTags.Contains(other.gameObject.tag))
        {
            TriggerStayEvent();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (TargetTags.Contains(other.gameObject.tag))
        {
            if (CollideredObjes.Contains(other.gameObject))
            {
                TriggerExitEvent(other.gameObject);
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

    public void TriggerEnterEvent(GameObject obj)
    {
        CollideredObjesAdd(obj.gameObject);
        OnTriggerEnterEvent.Invoke();
    }
    public void TriggerStayEvent()
    {
        OnTriggerStayEvent.Invoke();
    }
    public void TriggerExitEvent(GameObject obj)
    {
        CollideredObjesRemove(obj);
        OnTriggerExitEvent.Invoke();
    }
}
