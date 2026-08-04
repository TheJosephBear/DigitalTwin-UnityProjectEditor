using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomMovement : MonoBehaviour {
    [Header("Look & Movement Settings")]
    public bool RightClickLookEnabled = true;
    public float moveSpeed = 5f;
    public float lookSensitivity = 2f;

    [Header("Pan Settings")]
    public float panSensitivity = 0.5f;

    [Header("Zoom / Scroll Settings")]
    public float zoomSensitivity = 10f;
    public float ScrollSlowDistance = 50f;
    public float MinScrollSpeed = 2f;
    public float MaxScrollSpeed = 20f;
    public LayerMask GroundGeometryLayers = ~0; // Default to 'Everything'

    private GameObject _movedObject;
    private float _rotationX = 0f;
    private float _rotationY = 0f;

    private Vector2 _moveInput = Vector2.zero;
    private float _ascendInput = 0f;

    void Update() {
        if (_movedObject == null) return;

        // Rotation Controls
        if (RightClickLookEnabled) {
            RightClickLook();
        } else {
            NoPressLook();
        }

        // Pan Controls (Middle Mouse Click)
        HandlePan();

        // Zoom / Scroll Controls
        HandleZoom();
        UpdateCameraSpeed();

        // Standard Movement
        HandleWASDInput();
        ApplyMovement();
    }

    void HandleWASDInput() {
        _moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        _ascendInput = 0f;

        if (Input.GetKey(KeyCode.E)) _ascendInput += 1f;
        if (Input.GetKey(KeyCode.Q)) _ascendInput -= 1f;
    }

    void ApplyMovement() {
        Vector3 inputDir = new Vector3(_moveInput.x, _ascendInput, _moveInput.y);
        Vector3 moveDir = _movedObject.transform.TransformDirection(inputDir.normalized);
        _movedObject.transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    void RightClickLook() {
        if (Input.GetMouseButton(1)) {
            CameraRotating();
        }
    }

    void NoPressLook() {
        CameraRotating();
    }

    void CameraRotating() {
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        _rotationY += mouseX;
        _rotationX -= mouseY;
        _rotationX = Mathf.Clamp(_rotationX, -90f, 90f);

        _movedObject.transform.rotation = Quaternion.Euler(_rotationX, _rotationY, 0f);
    }

    void HandlePan() {
        // Middle Mouse Button (Button 2)
        if (Input.GetMouseButton(2)) {
            float mouseX = Input.GetAxis("Mouse X") * panSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * panSensitivity;

            // Move relative to current orientation (Left/Right, Up/Down)
            Vector3 panDirection = (_movedObject.transform.right * -mouseX) + (_movedObject.transform.up * -mouseY);
            _movedObject.transform.position += panDirection;
        }
    }

    void HandleZoom() {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollInput) > 0.01f) {
            // Move forward/backward along the target's forward view vector based on scroll direction
            Vector3 zoomDirection = _movedObject.transform.forward * (scrollInput * zoomSensitivity);
            _movedObject.transform.position += zoomDirection;
        }
    }

    void UpdateCameraSpeed() {
        float currentDistance = ScrollSlowDistance; // Default to max distance if nothing is hit

        // Cast a ray forward from target position to find geometry in view
        Ray ray = new Ray(_movedObject.transform.position, _movedObject.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, ScrollSlowDistance, GroundGeometryLayers)) {
            currentDistance = hit.distance;
        }

        // Map distance (0 to ScrollSlowDistance) to t (0 to 1)
        float t = Mathf.Clamp01(currentDistance / ScrollSlowDistance);

        // Smoothly transition from MinScrollSpeed (close) to MaxScrollSpeed (far)
        float newScrollSpeed = Mathf.Lerp(MinScrollSpeed, MaxScrollSpeed, t);

        // Apply dynamically calculated speed if GizmoManager exists
        if (GizmoManager.Instance != null) {
            GizmoManager.Instance.SetFreecamScrollSpeed(newScrollSpeed);
        }
    }

    public void SetTarget(GameObject target) {
        _movedObject = target;
        if (target == null) return;
        Vector3 currentEuler = _movedObject.transform.rotation.eulerAngles;

        // Smooth out X rotation to avoid breaking Mathf.Clamp bounds
        _rotationX = currentEuler.x;
        if (_rotationX > 180f) _rotationX -= 360f;
        _rotationY = currentEuler.y;
    }
}
