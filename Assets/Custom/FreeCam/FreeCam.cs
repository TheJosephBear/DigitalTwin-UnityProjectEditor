using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class FreeCam : MonoBehaviour {

    GameObject activeVCam;
    public float movementSpeed = 10f;
    public float mouseSensitivity = 2f;
    public float slowdownSpeed = 5f;

    float yaw = 0f;
    float pitch = 0f;
    Vector3 currentVelocity;

    bool canMove = false;

    void Update() {
        if (canMove) {
            MouseLook();
            Movement();
        }
    }

    public void ToggleFreeCam(bool enable) {
        canMove = enable;
        activeVCam = FindAnyObjectByType<CinemachineBrain>().ActiveVirtualCamera?.VirtualCameraGameObject;
        if (activeVCam!= null && enable) {
            // Initialize yaw and pitch to match the current camera rotation
            Vector3 currentRotation = activeVCam.transform.eulerAngles;
            yaw = currentRotation.y;
            pitch = currentRotation.x;

            // Hide cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        } else if(activeVCam != null) {
            // Show cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void MouseLook() {
        if (activeVCam == null)
            return;
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, -90f, 90f);
        // Apply the rotation to the camera
        activeVCam.transform.eulerAngles = new Vector3(pitch, yaw, 0f);
    }

    void Movement() {
        if (activeVCam == null) 
            return;
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 forwardMovement = activeVCam.transform.forward * verticalInput;
        Vector3 rightMovement = activeVCam.transform.right * horizontalInput;
        Vector3 movement = (forwardMovement + rightMovement).normalized * movementSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.E)) {
            movement.y += movementSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.Q)) {
            movement.y -= movementSpeed * Time.deltaTime;
        }
        // Slow down smoothly if not moving forward
        if (verticalInput == 0) {
            movement = Vector3.SmoothDamp(movement, Vector3.zero, ref currentVelocity, 1f / slowdownSpeed);
        }
        // Apply the movement to the camera
        activeVCam.transform.position += movement;
    }
}
