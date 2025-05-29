using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movable : MonoBehaviour, IClickable {

    public GizmoType MovableType;
    public List<GizmoAxis> ShownAxis;

    public void OnClick() {

    }

    public virtual void OnClickDown() {
        if (GizmoManager.Instance.GetTargetObject() != this && !GizmoManager.Instance.IsShowingGizmo()) {
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