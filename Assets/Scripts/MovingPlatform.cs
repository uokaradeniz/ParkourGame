using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public float platformSpeed;

    private Rigidbody rb;
    private Vector3 moveDir;

    public int direction = 1;
    private GameObject player;
    private PlayerMovement playerMovement;
    private Rigidbody playerRb;
    public float playerSpeedMultiplier;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerMovement = player.GetComponent<PlayerMovement>();
        playerRb = player.GetComponent<Rigidbody>();
        rb = GetComponentInChildren<Rigidbody>();
    }

    Vector3 currentTarget()
    {
        if (direction == 1)
            return startPoint.position;

        return endPoint.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 targetPos = currentTarget();
        if (Vector3.Distance(transform.position, targetPos) < 1)
            direction *= -1;

        moveDir = targetPos - transform.position;
        rb.AddForce(Time.fixedDeltaTime * platformSpeed * moveDir, ForceMode.VelocityChange);
        if (playerMovement.onPlatform)
            playerRb.velocity = playerMovement.dir.normalized + rb.velocity * playerSpeedMultiplier;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerMovement.onPlatform = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerMovement.onPlatform = false;
    }
}