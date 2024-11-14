using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectClickListener : MonoBehaviour {

    bool isListenerActive = true;

    void Update() {
        // Check for left mouse button click using the old input system
        if (Input.GetMouseButtonDown(0)) {
            HandleClick();
        }
    }

    void HandleClick() {
        if (!isListenerActive) return;
        if (Camera.main == null) return;

        Vector3 pointerPosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(pointerPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
            GameObject clickedObject = hit.collider.gameObject;

            // Check if the clicked object has an IClickable component
            IClickable clickable = clickedObject.GetComponent<IClickable>();
            if (clickable != null) {
                clickable.OnClick();
            }
        }
    }

    public void ToggleListener(bool active) {
        isListenerActive = active;
    }
}
