using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomMovement : MonoBehaviour {

    public bool RightClickLookEnabled = true;
    public float moveSpeed = 5f;
    public float lookSensitivity = 2f;

    private GameObject _movedObject;
    private float _rotationX = 0f;
    private float _rotationY = 0f;

    private Vector2 _moveInput = Vector2.zero;
    private float _ascendInput = 0f;

    void Update() {
        if (_movedObject == null) return;

        if (RightClickLookEnabled) {
            RightClickLook();
        } else {
            NoPressLook();
        }

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

    public void SetTarget(GameObject target) {
        _movedObject = target;
        if (target == null) return;
        Vector3 currentEuler = _movedObject.transform.rotation.eulerAngles;
        // Smooth out the X rotation so it doesn't break the Mathf.Clamp bounds
        _rotationX = currentEuler.x;
        if (_rotationX > 180f) _rotationX -= 360f;
        _rotationY = currentEuler.y;
    }
}