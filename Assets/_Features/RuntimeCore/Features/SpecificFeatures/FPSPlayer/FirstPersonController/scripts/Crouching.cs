using UnityEngine;
using UnityEngine.InputSystem;

public class Crouching: MonoBehaviour {

    [Header("References")]
    public Transform cameraTransform;   // the camera, or the transform that holds it

    [Header("Crouch")]
    [Range(0.3f, 0.95f)]
    public float crouchHeightRatio = 0.5f;  // crouched height as a fraction of the standing height
    public float crouchTime = 0.15f;        // seconds for a full stand <-> crouch transition

    [Header("Obstacle check")]
    public LayerMask obstacleLayers;    // must NOT include the player's own layer
    public float checkSkin = 0.05f;     // shrink the check radius a bit to avoid grazing walls

    /// <summary>Set by PlayerSetup. When false, crouching is ignored.</summary>
    public bool CrouchAllowed { get; set; } = true;

    CharacterController characterController;
    CapsuleCollider capsuleCollider;
    FirstPersonMovement firstPersonMovement;

    // Standing state
    float standingHeight;
    Vector3 standingCenter;
    Vector3 standingCameraLocalPos;
    float crouchHeight;
    bool sprintWasAllowed;

    bool wantsCrouch;
    float crouchAmount;                 // 0 = standing, 1 = fully crouched

    void Awake() {
        characterController = GetComponent<CharacterController>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        firstPersonMovement = GetComponent<FirstPersonMovement>();

        sprintWasAllowed = firstPersonMovement.sprintAllowed;
        CaptureStandingValues();
    }

    /// <summary>Reads the current (standing) dimensions as the new baseline. Call only while standing.</summary>
    public void CaptureStandingValues() {
        standingHeight = characterController.height;
        standingCenter = characterController.center;
        standingCameraLocalPos = cameraTransform.localPosition;

        // A capsule can't be shorter than its own diameter
        crouchHeight = Mathf.Clamp(standingHeight * crouchHeightRatio, characterController.radius * 2f, standingHeight);
    }

    /// <summary>Instantly returns to the standing state (and restores sprinting).</summary>
    public void ResetToStanding() {
        wantsCrouch = false;
        crouchAmount = 0f;
        Apply(0f);
        firstPersonMovement.sprintAllowed = sprintWasAllowed;
    }

    public void CrouchToggle() {
        if (!CrouchAllowed) return;

        if (wantsCrouch && !CanStandUp()) {
            Debug.Log("Cannot stand up, something is blocking the way!");
            return;
        }

        wantsCrouch = !wantsCrouch;

        if (sprintWasAllowed) {
            firstPersonMovement.sprintAllowed = !wantsCrouch;
            if (wantsCrouch) firstPersonMovement.isSprinting = false;
        }
    }

    // Is there room for the full-height standing capsule?
    bool CanStandUp() {
        float radius = characterController.radius - checkSkin;

        // Bottom of the standing capsule (feet), in world space
        Vector3 feet = transform.TransformPoint(standingCenter - Vector3.up * (standingHeight * 0.5f));
        Vector3 bottomSphere = feet + Vector3.up * characterController.radius;
        Vector3 topSphere = feet + Vector3.up * (standingHeight - characterController.radius);

        return !Physics.CheckCapsule(bottomSphere, topSphere, radius, obstacleLayers, QueryTriggerInteraction.Ignore);
    }

    void Update() {
        float target = wantsCrouch ? 1f : 0f;
        if (Mathf.Approximately(crouchAmount, target)) return;

        crouchAmount = Mathf.MoveTowards(crouchAmount, target, Time.deltaTime / Mathf.Max(crouchTime, 0.0001f));
        Apply(Mathf.SmoothStep(0f, 1f, crouchAmount));
    }

    void Apply(float t) {
        float height = Mathf.Lerp(standingHeight, crouchHeight, t);
        float heightLost = standingHeight - height;

        // Keep the feet where they are: the center moves down by half of what was lost
        Vector3 center = standingCenter - Vector3.up * (heightLost * 0.5f);

        characterController.height = height;
        characterController.center = center;

        capsuleCollider.height = height;
        capsuleCollider.center = center;

        // Camera keeps its distance from the top of the capsule
        Vector3 camPos = standingCameraLocalPos;
        camPos.y -= heightLost;
        cameraTransform.localPosition = camPos;
    }

    #region Input

    public void OnCrouch(InputAction.CallbackContext context) {
        if (context.performed) CrouchToggle();
    }

    #endregion
}
