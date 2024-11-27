using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionChase : MonoBehaviour
{
    public Transform Target;
    public bool X;
    public bool Y;
    public bool Z;

    [SerializeField] private Vector3 TargetVector;
    // Start is called before the first frame update
    void Start()
    {
        TargetVector = this.transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        if (X)
        {
            TargetVector = new Vector3(Target.transform.position.x, TargetVector.y, TargetVector.z);
        }
        if (Y)
        {
            TargetVector = new Vector3(TargetVector.x, Target.transform.position.y, TargetVector.z);

        }
        if (Z)
        {
            TargetVector = new Vector3(TargetVector.x, TargetVector.y, Target.transform.position.z);
        }
        this.transform.position = TargetVector;
    }
}
