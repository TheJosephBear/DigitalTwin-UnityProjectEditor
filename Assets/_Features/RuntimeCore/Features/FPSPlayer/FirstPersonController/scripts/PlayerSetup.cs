using System;
using UnityEngine;

/// <summary>
/// Plain data holder for everything the user can configure about the player.
/// </summary>
[Serializable]
public class PlayerSettings {
    public const float MinHeight = 0.5f;

    public float speed = 5f;
    public bool jumpAllowed = true;
    public float jumpStrength = 1.5f;   // jump height in meters
    public bool crouchAllowed = true;
    public float playerHeight = 2f;     // meters

    public PlayerSettings Clone() => (PlayerSettings)MemberwiseClone();

    public void Sanitize(float minHeight = MinHeight) {
        speed = Mathf.Max(0f, speed);
        jumpStrength = Mathf.Max(0f, jumpStrength);
        playerHeight = Mathf.Max(minHeight, playerHeight);
    }

    public bool ApproximatelyEquals(PlayerSettings other) {
        if (other == null) return false;
        return Mathf.Approximately(speed, other.speed)
            && jumpAllowed == other.jumpAllowed
            && Mathf.Approximately(jumpStrength, other.jumpStrength)
            && crouchAllowed == other.crouchAllowed
            && Mathf.Approximately(playerHeight, other.playerHeight);
    }
}

/// <summary>
/// Lives on the player prefab. Reads the prefab's own values once as "defaults",
/// and applies a PlayerSettings object to the movement / jump / crouch scripts,
/// the character controller, the capsule collider and the camera.
/// </summary>
public class PlayerSetup: MonoBehaviour {

    [Tooltip("Camera / vcam transform that sits at eye level (child of the player). " +
             "Falls back to the Crouching script's camera if empty.")]
    [SerializeField] private Transform cameraTransform;

    CharacterController controller;
    CapsuleCollider capsule;
    FirstPersonMovement movement;
    Jumping jumping;
    Crouching crouching;

    bool captured;
    PlayerSettings defaults;
    PlayerSettings current;

    // Original geometry, captured once, everything height related is derived from this
    float baseHeight;
    Vector3 baseCenter;
    Vector3 baseCameraLocalPos;

    /// <summary>The values the prefab had before anything was applied.</summary>
    public PlayerSettings Defaults { get { EnsureCaptured(); return defaults.Clone(); } }

    /// <summary>The settings that are currently applied (sanitized).</summary>
    public PlayerSettings Current { get { EnsureCaptured(); return current.Clone(); } }

    void Awake() {
        EnsureCaptured();
    }

    void EnsureCaptured() {
        if (captured) return;
        captured = true;

        controller = GetComponent<CharacterController>();
        capsule = GetComponent<CapsuleCollider>();
        movement = GetComponent<FirstPersonMovement>();
        jumping = GetComponent<Jumping>();
        crouching = GetComponent<Crouching>();

        if (cameraTransform == null && crouching != null) cameraTransform = crouching.cameraTransform;

        defaults = new PlayerSettings();

        if (controller != null) {
            baseHeight = controller.height;
            baseCenter = controller.center;
            defaults.playerHeight = baseHeight;
        }
        if (cameraTransform != null) baseCameraLocalPos = cameraTransform.localPosition;

        if (movement != null) defaults.speed = movement.maxBaseMovementSpeed;
        if (jumping != null) {
            defaults.jumpAllowed = jumping.JumpAllowed;
            defaults.jumpStrength = jumping.JumpHeight;
        }
        if (crouching != null) defaults.crouchAllowed = crouching.CrouchAllowed;

        current = defaults.Clone();
    }

    /// <summary>Returns a clamped copy (e.g. height can't be below the capsule diameter). Applies nothing.</summary>
    public PlayerSettings Sanitize(PlayerSettings settings) {
        EnsureCaptured();
        PlayerSettings s = settings.Clone();
        float minHeight = controller != null
            ? Mathf.Max(PlayerSettings.MinHeight, controller.radius * 2f)
            : PlayerSettings.MinHeight;
        s.Sanitize(minHeight);
        return s;
    }

    public void Apply(PlayerSettings settings) {
        EnsureCaptured();
        if (settings == null) return;

        PlayerSettings s = Sanitize(settings);
        current = s;

        // Make sure we're standing (and the crouch script has its original values) before resizing
        if (crouching != null) crouching.ResetToStanding();

        ApplyHeight(s.playerHeight);

        if (movement != null) movement.maxBaseMovementSpeed = s.speed;

        if (jumping != null) {
            jumping.JumpAllowed = s.jumpAllowed;
            jumping.JumpHeight = s.jumpStrength;
        }

        if (crouching != null) {
            crouching.CrouchAllowed = s.crouchAllowed;
            crouching.CaptureStandingValues();   // new standing height becomes the crouch baseline
        }
    }

    void ApplyHeight(float height) {
        if (controller != null) {
            // Keep the feet where they were in the original setup, grow/shrink upwards
            float feetOffset = baseCenter.y - baseHeight * 0.5f;
            Vector3 center = baseCenter;
            center.y = feetOffset + height * 0.5f;

            controller.height = height;
            controller.center = center;

            if (capsule != null) {
                capsule.height = height;
                capsule.center = center;
            }
        }

        // Camera keeps its original distance from the top of the capsule
        if (cameraTransform != null) {
            cameraTransform.localPosition = baseCameraLocalPos + Vector3.up * (height - baseHeight);
        }
    }
}
