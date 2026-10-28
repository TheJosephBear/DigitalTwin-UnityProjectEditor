using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class CursorManager : MonoBehaviour {
    // Keep the texture at the size of the OS arrow (32x32 canvas, hand ~17x23) so swapping cursors doesn't look like a jump.
    [SerializeField] private Texture2D clickCursor;
    // Pixel of the texture that sits on the mouse position - the fingertip, so it lines up with the arrow tip.
    [SerializeField] private Vector2 hotspot = new Vector2(6, 0);

    private Texture2D lastAppliedCursor = null;
    private bool wasOverUIToolkit = false;

    private PointerEventData pointerData;
    private EventSystem pointerDataEventSystem;
    private readonly List<RaycastResult> results = new List<RaycastResult>();

    void Update() {
        if (EventSystem.current == null)
            return;

        if (pointerData == null || pointerDataEventSystem != EventSystem.current) {
            pointerData = new PointerEventData(EventSystem.current);
            pointerDataEventSystem = EventSystem.current;
        }
        pointerData.position = Input.mousePosition;

        results.Clear();
        EventSystem.current.RaycastAll(pointerData, results);

        bool hoveringButton = false;
        bool isOverUIToolkit = false;

        // 1. Check if we hit absolutely anything at all
        if (results.Count > 0) {
            // 2. Get the closest/topmost object
            GameObject topmostObject = results[0].gameObject;
            BaseRaycaster module = results[0].module;

            // 3. Check if the raycast hit a UI Toolkit panel
            if (module is UnityEngine.UIElements.PanelRaycaster) {
                isOverUIToolkit = true;
            } else {
                // 4. Check if this topmost object (or its parents) is an interactable Button
                Button button = topmostObject.GetComponentInParent<Button>();
                hoveringButton = button != null && button.IsInteractable();
            }
        }

        // Determine the target cursor we want to show
        Texture2D targetCursor = hoveringButton ? clickCursor : null;

        if (isOverUIToolkit) {
            // If we transitioned to UI Toolkit, we clear any cursor set by CursorManager
            // to allow UI Toolkit and its styles to control the cursor.
            if (lastAppliedCursor != null || !wasOverUIToolkit) {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                lastAppliedCursor = null;
            }
        } else {
            // Only update the cursor if there is a change to avoid overrides
            if (lastAppliedCursor != targetCursor || wasOverUIToolkit) {
                // Auto = hardware cursor: drawn by the OS without the one-frame lag of a software cursor,
                // and the same mode UI Toolkit uses for USS cursors, so both UIs behave alike.
                Cursor.SetCursor(targetCursor, targetCursor != null ? hotspot : Vector2.zero, CursorMode.Auto);
                lastAppliedCursor = targetCursor;
            }
        }

        wasOverUIToolkit = isOverUIToolkit;
    }

    void OnDisable() {
        // Don't leave the hand stuck on screen if the manager gets disabled while hovering a button
        if (lastAppliedCursor != null) {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            lastAppliedCursor = null;
        }
    }
}
