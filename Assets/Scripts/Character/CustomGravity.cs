using UnityEngine;

public class CustomGravity : MonoBehaviour
{
    public float gravityMultiplier = 10.0f; // èdóÕÇÃî{ó¶
    private Rigidbody rb;
    public bool UseCharacterController;
    private CharacterController character_controller;
    public Vector3 gravity;
    void Start()
    {
        if (UseCharacterController)
        {
            character_controller = this.gameObject.GetComponent<CharacterController>();
        }
        else
        {
            rb = GetComponent<Rigidbody>();
        }
    }

    void FixedUpdate()
    {
        gravity = Vector3.down * gravityMultiplier;
        if (UseCharacterController)
        {
            if(character_controller.enabled)
            character_controller.Move(new Vector3(character_controller.velocity.x * Time.deltaTime, gravity.y * Time.deltaTime, character_controller.velocity.z * Time.deltaTime));
        }
        else
        {
            rb.AddForce(gravity, ForceMode.Acceleration);
        }
    }
}