using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Crouching : MonoBehaviour {

    CharacterController characterController;
    CapsuleCollider capsuleCollider;
    FirstPersonMovement firstPersonMovement;
    public Transform cameraTransform;

    public float crouchHeight = 1.0f;
    public float standingHeight = 2.0f;
    public float crouchSpeed = 0.1f;
    public float cameraCrouchOffset = -0.5f;
    float cameraOriginalHeight;

    bool isCrouching = false;
    bool isTransitioning = false;
    bool wasSprintingAllowed = false;

    // LayerMask to define what objects can block standing up
    public LayerMask obstacleLayers;

    // Length of the ray that checks for obstacles above the player's head
    public float raycastBuffer = 0.2f;

    void Start() {
        characterController = GetComponent<CharacterController>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        firstPersonMovement = GetComponent<FirstPersonMovement>();
        cameraOriginalHeight = cameraTransform.localPosition.y;

        wasSprintingAllowed = firstPersonMovement.sprintAllowed;
    }

    public void CrouchToggle() {
        if (!isTransitioning) {
            if (isCrouching && !CanStandUp()) {
                Debug.Log("Cannot stand up, something is blocking the way!");
                return;
            }

            isCrouching = !isCrouching;
            if (wasSprintingAllowed) {
                firstPersonMovement.sprintAllowed = !isCrouching;
                if(isCrouching) firstPersonMovement.isSprinting = false;
            }

            StartCoroutine(CrouchRoutine(isCrouching ? crouchHeight : standingHeight));
        }
    }

    // This function checks if there's space above the player to stand up
    bool CanStandUp() {
        // Calculate the distance to check based on the difference between standing and crouch height
        float rayLength = standingHeight - crouchHeight + raycastBuffer;

        // Perform the raycast starting from the player's current position, casting upwards
        Ray ray = new Ray(transform.position, Vector3.up);
        return !Physics.Raycast(ray, rayLength, obstacleLayers);
    }

    private IEnumerator CrouchRoutine(float targetHeight) {
        isTransitioning = true;

        float initialHeight = characterController.height;
        float initialCameraHeight = cameraTransform.localPosition.y;
        float targetCameraHeight = isCrouching ? cameraOriginalHeight + cameraCrouchOffset : cameraOriginalHeight;
        float elapsed = 0f;

        while (elapsed < 1f) {
            elapsed += Time.deltaTime / crouchSpeed;
            characterController.height = Mathf.Lerp(initialHeight, targetHeight, elapsed);
            characterController.center = new Vector3(0, characterController.height / 2, 0);
            capsuleCollider.height = Mathf.Lerp(initialHeight, targetHeight, elapsed);
            capsuleCollider.center = new Vector3(0, capsuleCollider.height / 2, 0);
            cameraTransform.localPosition = new Vector3(cameraTransform.localPosition.x, Mathf.Lerp(initialCameraHeight, targetCameraHeight, elapsed), cameraTransform.localPosition.z);
            yield return null;
        }

        isTransitioning = false;
    }

    #region Input

    void OnCrouch(InputAction.CallbackContext context) {
        CrouchToggle();
    }

    #endregion
}
