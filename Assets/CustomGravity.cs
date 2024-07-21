using UnityEngine;

public class CustomGravity : MonoBehaviour
{
    public float gravityMultiplier = 10.0f; // èdóÕÇÃî{ó¶
    private Rigidbody rb;
    public Vector3 gravity;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        rb.AddForce(Vector3.down * gravityMultiplier, ForceMode.Acceleration);
    }
}