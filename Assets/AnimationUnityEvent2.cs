using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
public class AnimationUnityEvent2 : SerializedMonoBehaviour
{
    public Dictionary<string,UnityEvent2> AnimEvent;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Invoke(string event_name)
    {
        AnimEvent[event_name].Invoke();
    }
}
