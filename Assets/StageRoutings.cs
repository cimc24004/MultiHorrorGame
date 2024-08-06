using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageRoutings : MonoBehaviour
{
    public List<GameObject> Routings;

    // Start is called before the first frame update
    void Start()
    {
        Routings = new List<GameObject>(GameObject.FindGameObjectsWithTag("Route"));
        Routings.Reverse();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
