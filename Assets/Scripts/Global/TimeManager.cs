using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : SingleTonDontDestory<TimeManager>
{
    public float deltaTime;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.deltaTime = Time.deltaTime;
    }


}
