using UnityEngine;

public class PlayerInputController: MonoBehaviour {

    [Header("Input Settings")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.C;
    public KeyCode jumpKey = KeyCode.Space;

    private FirstPersonMovement movement;
    private Crouching crouching;
    private Jumping jumping;

    void Awake() {
        movement = GetComponent<FirstPersonMovement>();
        crouching = GetComponent<Crouching>();
        jumping = GetComponent<Jumping>();
    }

    void Update() {
        // Read movement axes (WASD / Left Stick)
        Vector2 moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );
        movement.SetMoveInput(moveInput);

        // Sprinting
        movement.SetSprinting(Input.GetKey(sprintKey));

        // Crouching (Toggle on key press)
        if (Input.GetKeyDown(crouchKey)) {
            if (crouching != null) {
                crouching.CrouchToggle();
            }
        }

        if (Input.GetKeyDown(jumpKey)) {
            jumping.Jump();
        }
    }
}
