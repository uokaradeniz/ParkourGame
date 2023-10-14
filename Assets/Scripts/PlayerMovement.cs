using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")] public float moveSpeed;
    public float normalSpeed;
    public float sprintSpeed;
    public float acceleration;
    public float airMoveSpeed;
    public float duckSpeed;
    public float onPlatformSpeed;

    private Transform orientation;
    public bool isSprinting;
    [Header("Crouch")] public bool isDucked;
    public bool cantStandUp;
    public float maxCheckDistance;

    private float vertical;
    private float horizontal;

    private Rigidbody rb;
    [HideInInspector] public Vector3 dir;
    private Swing swing;

    [Header("Jumping")] public LayerMask groundMask;
    public float jumpForce;
    public bool isGrounded;
    private Transform feetPos;

    [Header("Drag Control")] public float groundDrag;
    public float airDrag;
    public float airControlForce;
    public float onPlatformDrag;

    [Header("Slope Handling")] private RaycastHit slopeHit;
    private Vector3 slopeMoveDir;

    [Header(" ")] public float vignetteTime;
    private ParticleSystem fastPaceFX;
    private PostProcessVolume postProcessVolume;
    private Vignette vignette;

    private Slide slide;
    private AudioSource playerAudioSource;

    private new CapsuleCollider collider;


    public bool onPlatform;

    // Start is called before the first frame update
    void Start()
    {
        playerAudioSource = GetComponent<AudioSource>();
        slide = GetComponent<Slide>();
        rb = GetComponent<Rigidbody>();
        orientation = transform.Find("Orientation");
        feetPos = transform.Find("Feet Position");
        swing = GetComponent<Swing>();
        fastPaceFX = Camera.main.GetComponentInChildren<ParticleSystem>();
        postProcessVolume = GameObject.Find("Game Handler").GetComponent<PostProcessVolume>();
        postProcessVolume.profile.TryGetSettings(out vignette);
        collider = GetComponent<CapsuleCollider>();
    }

    private void Update()
    {
        Inputs();
        if (!slide.isSliding)
            ControlMoveSpeed();
        else
            moveSpeed = Mathf.Lerp(moveSpeed, slide.slideSpeed, acceleration * Time.deltaTime);

        if (isDucked)
            Duck();
        else
        {
            isGrounded = Physics.CheckSphere(feetPos.position, 0.25f, groundMask);
            collider.height = 2;
        }

        if (slide.isSliding && rb.drag > 15)
            slide.hasSlided = true;

        DragControl();

        slopeMoveDir = Vector3.ProjectOnPlane(dir, slopeHit.normal);

        if (rb.velocity.magnitude > 14f && (!isGrounded || (slide.isSliding && OnSlope())))
        {
            vignette.color.Override(Color.cyan);
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, 0.6f, vignetteTime);
            if (!playerAudioSource.isPlaying)
                playerAudioSource.Play();
            fastPaceFX.Play();
        }
        else
        {
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, 0, vignetteTime);
            playerAudioSource.Stop();
            fastPaceFX.Stop();
        }

        if (isDucked && !slide.isSliding && !isSprinting && isGrounded)
        {
            vignette.color.Override(Color.black);
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, 0.6f, vignetteTime);
        }
        else
        {
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, 0, vignetteTime);
        }
    }

    void DragControl()
    {
        if (!isGrounded && !slide.isSliding && !onPlatform)
            rb.drag = airDrag;
        else if (isGrounded && !slide.isSliding && !onPlatform)
            rb.drag = groundDrag;
        else if (isGrounded && slide.isSliding && !onPlatform)
        {
            if (!OnSlope())
                rb.drag = Mathf.Lerp(rb.drag, 500, 0.03f * Time.deltaTime);
        }
        else if (isDucked && !isGrounded && swing.isSwinging && !onPlatform)
            rb.drag = airDrag;
        else if (onPlatform)
        {
            rb.drag = onPlatformDrag;
        }
    }

    void AirControl()
    {
        if (!isGrounded && !Physics.Raycast(feetPos.position, Vector3.down, 5) && !swing.isSwinging)
        {
            rb.AddForce(airControlForce * Time.fixedDeltaTime * horizontal * orientation.right, ForceMode.Acceleration);
        }
    }

    void Duck()
    {
        isGrounded = Physics.CheckSphere(feetPos.position + new Vector3(0, 0.5f, 0), 0.25f, groundMask);
        collider.height = 1;
        if (Physics.Raycast(transform.position, Vector3.up, maxCheckDistance))
            cantStandUp = true;
        else
            cantStandUp = false;
    }

    void ControlMoveSpeed()
    {
        if (isSprinting && !isDucked && !onPlatform)
        {
            moveSpeed = Mathf.Lerp(moveSpeed, sprintSpeed, acceleration * Time.deltaTime);
        }
        else if (isDucked && !isSprinting && isGrounded && !onPlatform)
        {
            moveSpeed = Mathf.Lerp(moveSpeed, duckSpeed, acceleration * Time.deltaTime);
        }
        else if (onPlatform)
        {
            moveSpeed = isDucked ? onPlatformSpeed * .5f : onPlatformSpeed;
        }
        else
            moveSpeed = Mathf.Lerp(moveSpeed, normalSpeed, acceleration * Time.deltaTime);
    }

    void Inputs()
    {
        if (!slide.isSliding)
            vertical = Input.GetAxis("Vertical");

        horizontal = Input.GetAxis("Horizontal");

        if (Input.GetButtonDown("Jump") && isGrounded)
            Jump();

        if (Input.GetButton("Sprint") && isGrounded && !isDucked && dir != Vector3.zero)
            isSprinting = true;
        else
            isSprinting = false;

        dir = orientation.forward * vertical + orientation.right * horizontal;

        if (Input.GetButton("Duck") || cantStandUp)
            isDucked = true;
        else
        {
            isDucked = false;
            slide.hasSlided = false;
        }
    }

    bool OnSlope()
    {
        if (isDucked)
        {
            if (Physics.Raycast(feetPos.position + new Vector3(0, 0.5f, 0), Vector3.down, out slopeHit, 0.4f,
                    groundMask))
            {
                if (slopeHit.normal != Vector3.up)
                    return true;
            }
        }
        else
        {
            if (Physics.Raycast(feetPos.position, Vector3.down, out slopeHit, 0.2f, groundMask))
            {
                if (slopeHit.normal != Vector3.up)
                    return true;
            }
        }

        return false;
    }

    void Jump()
    {
        if (isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            float jumpHeight = Mathf.Sqrt(jumpForce * -2f * Physics.gravity.y);
            rb.AddForce(jumpHeight * Time.fixedDeltaTime * transform.up, ForceMode.Impulse);
            onPlatform = false;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Movement();
        AirControl();
    }

    void Movement()
    {
        if (isGrounded && !OnSlope())
            rb.AddForce(moveSpeed * Time.fixedDeltaTime * dir.normalized, ForceMode.Acceleration);
        else if (isGrounded && OnSlope())
            rb.AddForce(moveSpeed * Time.fixedDeltaTime * slopeMoveDir.normalized, ForceMode.Acceleration);
        else if (!isGrounded)
            rb.AddForce(airMoveSpeed * Time.fixedDeltaTime * dir.normalized, ForceMode.Acceleration);
    }
}