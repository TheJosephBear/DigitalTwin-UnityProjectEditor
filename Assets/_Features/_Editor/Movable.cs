using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movable : MonoBehaviour, IClickable {

    public MovableType MovableType;
    public GizmoAxis ShownAxis;

    public void OnClick() {

    }

    public void OnClickDown() {
        if (GizmoManager.Instance.IsShowingGizmo()) {
            GizmoManager.Instance.HideGizmo();
        } else {
            GizmoManager.Instance.SetTargetGameObject(gameObject);
            GizmoManager.Instance.ShowGizmo(MovableType, ShownAxis);
        }
    }

    public void OnClickUp() {

    }

    public void OnHover() {

    }

    public void OnUnhover() {

    }
}