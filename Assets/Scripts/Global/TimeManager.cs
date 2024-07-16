using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : SingleTonDontDestory<TimeManager>
{
    private float time_now;
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

    public bool TimeCountDown(float time)
    {
        time_now += Time.deltaTime;
        if (time_now > time)
        {
            time_now = 0;
            return true;
        }
        return false;
    }
}
