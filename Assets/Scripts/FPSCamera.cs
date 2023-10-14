using System;
using UnityEngine;

public class FPSCamera : MonoBehaviour
{
    [Range(0,1000)] [SerializeField] private float sensitivity;

    private WallRun wallRun;

    private float mouseX;
    private float mouseY;

    private float xRotation;
    private float yRotation;

    private Transform cameraPos;
    private Transform orientation;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        wallRun = GameObject.FindGameObjectWithTag("Player").GetComponent<WallRun>();
        cameraPos = wallRun.transform.Find("Camera Position");
        orientation = wallRun.transform.Find("Orientation");
    }

    private void Update()
    {
        transform.position = cameraPos.position;
    }

    void LateUpdate()
    {
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
        yRotation += mouseX * sensitivity * Time.deltaTime;
        xRotation -= mouseY * sensitivity * Time.deltaTime;
        xRotation = Math.Clamp(xRotation, -90, 80);
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, wallRun.tilt);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
