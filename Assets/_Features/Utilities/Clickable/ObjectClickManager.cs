using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectClickManager : Singleton<ObjectClickManager> {
    private List<IClickable> currentHovered = new List<IClickable>();
    private List<IClickable> currentClicked = new List<IClickable>();

    private Camera mainCamera;

    protected override void Awake() {
        mainCamera = Camera.main;
    }

    void Update() {
        HandleHover();
        HandleClick();
    }

    void HandleHover() {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        List<IClickable> newHovered = new List<IClickable>();

        if (Physics.Raycast(ray, out hit)) {
            var clickables = hit.collider.GetComponents<IClickable>();
            newHovered.AddRange(clickables);
        }

        foreach (var oldHover in currentHovered) {
            if (!newHovered.Contains(oldHover))
                oldHover.OnUnhover();
        }

        foreach (var newHover in newHovered) {
            if (!currentHovered.Contains(newHover))
                newHover.OnHover();
        }

        currentHovered = newHovered;
    }

    void HandleClick() {
        if (Input.GetMouseButtonDown(0)) {
            if (currentHovered.Count > 0) {
                currentClicked = new List<IClickable>(currentHovered);
                foreach (var c in currentClicked)
                    c.OnClickDown();
            }
        }

        if (Input.GetMouseButton(0)) {
            foreach (var c in currentClicked)
                c.OnClick();
        }

        if (Input.GetMouseButtonUp(0)) {
            foreach (var c in currentClicked)
                c.OnClickUp();
            currentClicked.Clear();
        }
    }
}