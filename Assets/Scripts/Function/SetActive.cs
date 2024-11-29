using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetActive : MonoBehaviour
{
    public bool StartOnActiveFalse;
    // Start is called before the first frame update
    void Start()
    {
        if (StartOnActiveFalse)
        {
            SetAc(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetAc(bool active)
    {
        this.gameObject.SetActive(active);
    }
}
