using UnityEngine;

public class Slide : MonoBehaviour
{
    [Header("Slide Properties")] public float slideSpeed;

    private PlayerMovement playerMovement;
    private Rigidbody rb;

    public bool dirCalculated;
    public bool isSliding;
    public bool hasSlided;

    Vector3 slideDir = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (playerMovement.isGrounded && Input.GetButton("Sprint") && playerMovement.isDucked && rb.velocity.y <= 0.1f && !hasSlided)
        {
            if (!dirCalculated)
            {
                slideDir = rb.velocity.normalized;
                dirCalculated = true;
            }

            SlideFunction(slideDir);
        }
        else
        {
            dirCalculated = false;
            isSliding = false;
        }
    }

    void SlideFunction(Vector3 slideDirection)
    {
        isSliding = true;
        rb.AddForce(playerMovement.moveSpeed * Time.fixedDeltaTime * slideDirection.normalized, ForceMode.Acceleration);
    }
}