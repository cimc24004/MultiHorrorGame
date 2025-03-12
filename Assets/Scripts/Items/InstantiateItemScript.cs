using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class InstantiateItemScript : MonoBehaviour
{
    [SerializeField]
    private float StartEventDelayTime = 0;
    [SerializeField]
    private float DestroyTime;
    [SerializeField]
    private UnityEvent2 StartEvent;

    [SerializeField]
    private UnityEvent2 UpdateEvent;

    public ItemStatus item_status;

    // Start is called before the first frame update
    void Start()
    {
        Invoke("StartEventInvoke", StartEventDelayTime);
        Destroy(this.gameObject,DestroyTime);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateEvent?.Invoke();
    }

    private void StartEventInvoke()
    {
        StartEvent.Invoke();
    }
}
