using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableClickListener : Singleton<MovableClickListener> {
    public LayerMask movableLayer;
    bool isListenerActive = true;

    void Update() {
        // Check for left mouse button click using the old input system
        if (Input.GetMouseButtonDown(0)) {
            HandleClick();
        }
    }

    private void HandleClick() {
        // Only proceed if the listener is active
        if (!isListenerActive) return;

        // Use the old input system to get the mouse position
        Vector3 pointerPosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(pointerPosition);
        RaycastHit hit;

        // Check if the ray hits an object on the movable layer
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, movableLayer)) {
            GameObject clickedObject = hit.collider.gameObject;
            GizmoManager.Instance.SetTargetGameObject(clickedObject);
            GizmoManager.Instance.ShowUniversalGizmo();
        }
    }

    public void ToggleListener(bool active) {
        isListenerActive = active;
    }
}
