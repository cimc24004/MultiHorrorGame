using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemThrowController : MonoBehaviour
{
    public GameObject itemPrefab;
    public GameObject Camera;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //(itemPrefab, 10, Camera.transform.rotation);
    }
    public void ThrowStart(GameObject throw_obj, float speed, Quaternion ThrowForward)
    {
        Instantiate(throw_obj, this.transform.position, ThrowForward);
    }
}
