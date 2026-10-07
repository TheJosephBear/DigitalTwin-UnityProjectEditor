using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class CursorManager : MonoBehaviour {
    [SerializeField] private Texture2D clickCursor;
    [SerializeField] private Vector2 hotspot = Vector2.zero;

    private Texture2D defaultCursor;
    private Texture2D lastAppliedCursor = null;
    private bool wasOverUIToolkit = false;

    void Update() {
        if (EventSystem.current == null)
            return;

        PointerEventData pointerData = new PointerEventData(EventSystem.current) {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
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
                // 4. Check if this topmost object (or its parents) is a Button
                if (topmostObject.GetComponentInParent<Button>() != null) {
                    // Only change the cursor if the button is actually interactable
                    if (topmostObject.GetComponentInParent<Button>().interactable) {
                        hoveringButton = true;
                    }
                }
            }
        }

        // Determine the target cursor we want to show
        Texture2D targetCursor = hoveringButton ? clickCursor : null;

        if (isOverUIToolkit) {
            // If we transitioned to UI Toolkit, we clear any cursor set by CursorManager
            // to allow UI Toolkit and its styles to control the cursor.
            if (lastAppliedCursor != null || !wasOverUIToolkit) {
                Cursor.SetCursor(null, hotspot, CursorMode.ForceSoftware);
                lastAppliedCursor = null;
            }
        } else {
            // Only update the hardware/software cursor if there is a change to avoid overrides
            if (lastAppliedCursor != targetCursor || wasOverUIToolkit) {
                Cursor.SetCursor(targetCursor, hotspot, CursorMode.ForceSoftware);
                lastAppliedCursor = targetCursor;
            }
        }

        wasOverUIToolkit = isOverUIToolkit;
    }
}