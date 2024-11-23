using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationChase : MonoBehaviour
{
    public Transform Target;
    public bool X;
    public bool Y;
    public bool Z;
    [SerializeField]private Vector3 TargetVector;


    // Start is called before the first frame update
    void Start()
    {
        TargetVector = this.transform.rotation.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        if (X)
        {
            TargetVector = new Vector3(Target.transform.eulerAngles.x, TargetVector.y, TargetVector.z);
        }
        if (Y)
        {
            TargetVector = new Vector3(TargetVector.x, Target.transform.eulerAngles.y, TargetVector.z);

        }
        if (Z)
        {
            TargetVector = new Vector3(TargetVector.x, TargetVector.y, Target.transform.eulerAngles.z);
        }
        this.transform.rotation =Quaternion.Euler(TargetVector);
    }
}
