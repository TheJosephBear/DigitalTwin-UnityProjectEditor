using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movable : MonoBehaviour, IClickable {

    public void OnClick() {
        if (GizmoManager.Instance.IsShowingGizmo()) {
            GizmoManager.Instance.HideGizmo();
        } else {
            GizmoManager.Instance.SetTargetGameObject(gameObject);
            GizmoManager.Instance.ShowUniversalGizmo();
        }
    }
}
