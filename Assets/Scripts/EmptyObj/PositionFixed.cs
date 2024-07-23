using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionFixed : MonoBehaviour
{
    private Vector3 DefaultPosition;
    // Start is called before the first frame update
    void Start()
    {
        DefaultPosition = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = DefaultPosition;
    }
}
