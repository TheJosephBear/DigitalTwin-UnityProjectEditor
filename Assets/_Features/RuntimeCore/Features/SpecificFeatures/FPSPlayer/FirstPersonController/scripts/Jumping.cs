using UnityEngine;
using UnityEngine.InputSystem;

public class Jumping: MonoBehaviour {
    [Header("Jump Settings")]
    [SerializeField] private float _jumpHeight = 1.5f;

    /// <summary>Set by PlayerSetup. When false, jumping is ignored.</summary>
    public bool JumpAllowed { get; set; } = true;

    /// <summary>Jump height in meters. Set by PlayerSetup.</summary>
    public float JumpHeight {
        get => _jumpHeight;
        set => _jumpHeight = Mathf.Max(0f, value);
    }

    private FirstPersonMovement _movementScript;

    private void Start() {
        _movementScript = GetComponent<FirstPersonMovement>();
    }

    // Takes the callback context so it only jumps once per press
    // (a parameterless method would also fire on started/canceled).
    public void OnJump(InputAction.CallbackContext context) {
        if (!context.performed) return;
        Jump();
    }

    public void Jump() {
        if (!JumpAllowed) return;

        if (_movementScript != null && _movementScript.IsGrounded()) {
            float gravity = _movementScript.GetGravity();
            float jumpVelocity = Mathf.Sqrt(_jumpHeight * -2f * gravity);

            _movementScript.ApplyJump(jumpVelocity);
        }
    }
}
