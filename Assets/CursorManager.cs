using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class CursorManager : MonoBehaviour {
    [SerializeField] private Texture2D clickCursor;
    [SerializeField] private Vector2 hotspot = Vector2.zero;

    private Texture2D defaultCursor;

    void Update() {
        if (EventSystem.current == null)
            return;

        PointerEventData pointerData = new PointerEventData(EventSystem.current) {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        bool hoveringButton = false;

        // 1. Check if we hit absolutely anything at all
        if (results.Count > 0) {
            // 2. Get the closest/topmost object
            GameObject topmostObject = results[0].gameObject;

            // 3. Check if this topmost object (or its parents) is a Button
            if (topmostObject.GetComponentInParent<Button>() != null) {
                // Only change the cursor if the button is actually interactable
                if (topmostObject.GetComponentInParent<Button>().interactable) {
                    hoveringButton = true;
                }
            }
        }

        Cursor.SetCursor(
            hoveringButton ? clickCursor : null,
            hotspot,
            CursorMode.ForceSoftware
        );
    }
}