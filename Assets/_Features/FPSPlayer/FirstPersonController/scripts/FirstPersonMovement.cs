using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonMovement: MonoBehaviour {

    #region Variables

    [Header("Movement")]
    public float maxBaseMovementSpeed = 5.0f;
    public float gravity = -9.81f;
    public float movementAcceleration = 1f;
    public float movementDeceleration = 1f;
    public float movementSmoothing = 1f;
    public float slopeSlideDownSpeed = 1f;
    public float slopeSlideDownRaycastLength = 3f;

    [Header("Sprinting and Stamina")]
    public bool sprintAllowed = true;
    public float sprintMultiplier = 2.0f;
    public float maximumStamina = 100f;
    public float staminaDrainRate = 10f;
    public float staminaRecoveryRate = 5f;

    CharacterController controller;
    Vector2 moveInput;
    public bool isSprinting = false;
    float currentSpeed = 0f;
    float currentStamina;
    float verticalVelocity = 0f;
    Vector3 lastDirection = Vector3.zero;

    #endregion

    void Start() {
        controller = GetComponent<CharacterController>();
        currentStamina = maximumStamina;
    }

    void Update() {
        HandleMovement(moveInput);
        HandleSprinting();
        HandleSliding();
    }

    void HandleMovement(Vector2 moveInput) {
        float targetSpeed = maxBaseMovementSpeed * (isSprinting ? sprintMultiplier : 1f);

        Camera mainCamera = Camera.main;
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;
        cameraForward.y = 0;
        cameraForward.Normalize();
        cameraRight.y = 0;
        cameraRight.Normalize();
        Vector3 desiredDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;

        lastDirection = Vector3.Lerp(lastDirection, desiredDirection, movementSmoothing * Time.deltaTime);
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, movementAcceleration * Time.deltaTime);


        if (controller.isGrounded) {
            if (Vector3.Angle(Vector3.up, lastDirection) > controller.slopeLimit) {
                verticalVelocity = -5f;
            } else {
                verticalVelocity = -0.5f;
            }
        } else {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 movement = lastDirection * currentSpeed + Vector3.up * verticalVelocity;
        controller.Move(movement * Time.deltaTime);
    }

    void HandleSprinting() {
        if (!sprintAllowed)
            return;

        if (isSprinting && currentStamina > 0) {
            currentStamina -= staminaDrainRate * Time.deltaTime;
        } else {
            currentStamina += staminaRecoveryRate * Time.deltaTime;
        }
        currentStamina = Mathf.Clamp(currentStamina, 0, maximumStamina);
    }

    void HandleSliding() {
        if (!controller.isGrounded) return;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, slopeSlideDownRaycastLength)) {
            Vector3 surfaceNormal = hit.normal;
            float slopeAngle = Vector3.Angle(Vector3.up, surfaceNormal);

            if (slopeAngle > controller.slopeLimit) {
                Vector3 slideDirection = Vector3.ProjectOnPlane(Vector3.down, surfaceNormal).normalized;
                controller.Move(slideDirection * (slopeSlideDownSpeed * Time.deltaTime));
            }
        }
    }

    #region Input Public Setters

    public void SetMoveInput(Vector2 input) {
        moveInput = input;
    }

    public void SetSprinting(bool sprinting) {
        if (!sprintAllowed) {
            isSprinting = false;
            return;
        }

        isSprinting = sprinting;
    }

    #endregion

    #region GetterSetters

    public float GetCurrentSpeed() {
        return currentSpeed;
    }

    public bool IsSprinting() {
        return isSprinting;
    }

    #endregion
}
