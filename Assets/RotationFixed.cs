using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationFixed : MonoBehaviour
{
    private Quaternion DefaultRotation;
    // Start is called before the first frame update
    void Start()
    {
        DefaultRotation = this.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.rotation = DefaultRotation;
    }
}
