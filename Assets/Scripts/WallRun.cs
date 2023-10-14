using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class WallRun : MonoBehaviour
{
    [Header("Wall Run Settings")] private Transform orientation;

    [SerializeField] private float wallDistance;
    [SerializeField] private float minJumpHeight;

    [FormerlySerializedAs("groundMask")] [SerializeField]
    private LayerMask wallMask;

    [SerializeField] private float wallRunGravity;
    [SerializeField] private float wallJumpForce;

    [Header("Camera Effects")] [SerializeField]
    private float fov;

    [SerializeField] private float wallRunFov;
    [SerializeField] private float camTilt;
    [SerializeField] private float camEffectsTime;
    public float tilt;
    private Camera cam;

    private bool wallLeft;
    private RaycastHit wallHitLeft;
    private bool wallRight;
    public bool wallRunning;
    private RaycastHit wallHitRight;
    private Rigidbody rb;

    private PlayerMovement playerMovement;
    [Header("Sprint Camera Bob")]
    public float bobDuration;
    private float bobTimer;
    private int bobDir = 1;
    public float tiltMax;

    bool CanWallRun()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, wallMask);
    }

    void CheckWall()
    {
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out wallHitLeft, wallDistance, wallMask);
        wallRight = Physics.Raycast(transform.position, orientation.right, out wallHitRight, wallDistance, wallMask);
    }

    // Start is called before the first frame update
    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody>();
        cam = Camera.main;
        orientation = transform.Find("Orientation");
    }

    // Update is called once per frame
    void Update()
    {
        CheckWall();
        if (CanWallRun())
        {
            if (wallLeft && !wallRight)
                StartWallRun();
            else if (!wallLeft && wallRight)
                StartWallRun();
            else
                StopWallRun();
        }
        else
            StopWallRun();
        
        if(!wallRunning && playerMovement.isSprinting)
            SprintHeadbobEffect(bobDuration);
    }
    
    void SprintHeadbobEffect(float seconds)
    {
        bobTimer += Time.deltaTime;
        tilt = Mathf.Lerp(tilt, bobDir*tiltMax, Time.deltaTime);
        if (bobTimer > seconds)
        {
            bobDir *= -1;
            bobTimer = 0;
        }
    }

    void StartWallRun()
    {
        wallRunning = true;
        rb.useGravity = false;
        rb.AddForce(Vector3.down * wallRunGravity, ForceMode.Force);

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, wallRunFov, camEffectsTime * Time.deltaTime);

        if (wallLeft)
            tilt = Mathf.Lerp(tilt, -camTilt, camEffectsTime * Time.deltaTime);
        else if (wallRight)
            tilt = Mathf.Lerp(tilt, camTilt, camEffectsTime * Time.deltaTime);

        if (Input.GetButtonDown("Jump"))
        {
            if (wallLeft)
            {
                Vector3 jumpDir = transform.up + wallHitLeft.normal;
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                rb.AddForce(wallJumpForce * Time.fixedDeltaTime * jumpDir, ForceMode.Impulse);
            }
            else if (wallRight)
            {
                Vector3 jumpDir = transform.up + wallHitRight.normal;
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                rb.AddForce(wallJumpForce * Time.fixedDeltaTime * jumpDir, ForceMode.Impulse);
            }
        }
    }

    void StopWallRun()
    {
        wallRunning = false;
        rb.useGravity = true;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fov, camEffectsTime * Time.deltaTime);
        if(!playerMovement.isSprinting)
        tilt = Mathf.Lerp(tilt, 0, camEffectsTime * Time.deltaTime);
    }
}