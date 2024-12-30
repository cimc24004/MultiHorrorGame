using UnityEngine;

public class CustomGravity : MonoBehaviour
{
    public float gravityMultiplier = 10.0f; // èdóÕÇÃî{ó¶
    private Rigidbody rb;
    public bool UseCharacterController;
    private CharacterController character_controller;
    [SerializeField]private GroundCheck ground_check;
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
    private void Update()
    {
        if (UseCharacterController)
        {
            gravity = Vector3.down * gravityMultiplier;
            if (character_controller.enabled )
            {
                character_controller.Move(new Vector3(0, gravity.y * Time.deltaTime, 0));
            }
        }

    }
    void FixedUpdate()
    {

        if (!UseCharacterController)
        {
            gravity = Vector3.down * gravityMultiplier;
            if (!ground_check.OnGround)
            {
                rb.AddForce(gravity, ForceMode.Acceleration);
            }
        }
    }
}