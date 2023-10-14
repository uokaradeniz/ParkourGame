using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Swing : MonoBehaviour
{
    public KeyCode swingKey = KeyCode.Mouse0;
    private LineRenderer lr;
    private Transform orientation;
    private Transform cam;

    [Header("Swinging")] public LayerMask grappleable;
    [SerializeField] private float maxGrappleDistance = 25f;
    private Vector3 swingPoint;
    private SpringJoint joint;
    private Rigidbody rb;
    [SerializeField] private float grappleForce;
    private Vector3 grappleDir;
    [HideInInspector] public bool isSwinging;
    private bool grappled;
    public float grappleUpVectorStrength;
    public float grappleAnimTime;
    private Vector3 currentGrapplePos;
    private Vector3 startPos;

    [Header("Joint Properties")] public float jointSpring;
    public float jointDamper;
    public float jointMassScale;

    RaycastHit hit;

    private PlayerMovement playerMovement;
    private GameHandler gameHandler;

    // Start is called before the first frame update
    void Start()
    {
        gameHandler = GameObject.Find("Game Handler").GetComponent<GameHandler>();
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody>();
        cam = Camera.main.transform;
        lr = GetComponentInChildren<LineRenderer>();
        orientation = transform.Find("Orientation");
    }

    // Update is called once per frame
    void Update()
    {
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance, grappleable))
        {
            gameHandler.grappleCrosshair.color = Color.green;
            if (Input.GetKeyDown(swingKey))
                StartSwing();
        }
        else
            gameHandler.grappleCrosshair.color = Color.red;


        if (Input.GetKeyUp(swingKey))
            StopSwing();

        if (Input.GetKeyDown(KeyCode.Mouse1) && isSwinging && !grappled)
        {
            Vector3 grappleDir = (swingPoint - cam.position) + new Vector3(0, grappleUpVectorStrength, 0);
            rb.AddForce(grappleDir * grappleForce, ForceMode.Acceleration);
            grappled = true;
        }

        if (playerMovement.isGrounded)
            grappled = false;

        if (!grappled)
            gameHandler.grappleImage.color = new Color(0, 1, 0, .7f);
        else
            gameHandler.grappleImage.color = new Color(1, 0, 0, .7f);
    }

    private void LateUpdate()
    {
        startPos = orientation.position - new Vector3(-0.1f, 0.5f, 0);
        if (isSwinging)
            DrawLineRenderer();
        else
            currentGrapplePos = startPos;
    }

    void StartSwing()
    {
        swingPoint = hit.point;
        joint = transform.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = swingPoint;

        float distanceFromPoint = Vector3.Distance(transform.position, swingPoint);
        joint.maxDistance = distanceFromPoint * 0.5f;
        joint.minDistance = distanceFromPoint * 0.1f;

        joint.spring = jointSpring;
        joint.damper = jointDamper;
        joint.massScale = jointMassScale;

        lr.positionCount = 2;
        isSwinging = true;
    }

    void StopSwing()
    {
        lr.positionCount = 0;
        isSwinging = false;
        Destroy(joint);
    }

    void DrawLineRenderer()
    {
        if (!joint)
            return;

        currentGrapplePos = Vector3.Lerp(currentGrapplePos, swingPoint, Time.deltaTime * grappleAnimTime);

        lr.SetPosition(0, startPos);
        lr.SetPosition(1, currentGrapplePos);
    }
}