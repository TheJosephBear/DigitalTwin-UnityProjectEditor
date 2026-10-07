using UnityEngine;

public class PlayerInputController: MonoBehaviour {

    [Header("Input Settings")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.C;

    private FirstPersonMovement movement;
    private Crouching crouching;

    void Awake() {
        movement = GetComponent<FirstPersonMovement>();
        crouching = GetComponent<Crouching>();
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
    }
}
