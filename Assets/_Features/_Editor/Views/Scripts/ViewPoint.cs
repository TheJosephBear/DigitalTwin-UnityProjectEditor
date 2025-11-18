using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewPoint : EditorObjectBase, IClickable {

    public GameObject vcam;

    public void OnClickDown() {
        EditorManager.Instance.ViewManager.SetActiveViewPoint(this);
        /*
        if (EditorManager.Instance.EditorModeCurrent != EditorMode.View) {
            GizmoManager.Instance.SetTargetGameObject(gameObject);
            GizmoManager.Instance.ShowGizmo(GizmoType.Universal, UniversalGizmoScaleDisabled: true);
            ViewManager.Instance.SetActiveViewPoint(this);
        }
        */
    }

    public void Activate() {
        GizmoManager.Instance.HideGizmo();
        vcam.SetActive(true);
    }

    public void Deactivate() {
        vcam.SetActive(false);
    }

    public SerializableInterestPoint Serialize() {
        return new SerializableInterestPoint {
            position = transform.position,
            eulerRotation = transform.eulerAngles 
        };
    }

    public void Deserialize(SerializableInterestPoint interestPoint) {
        transform.position = interestPoint.position;
        transform.eulerAngles = interestPoint.eulerRotation; 
    }

    public void OnClick() {

    }

    public void OnClickUp() {

    }

    public void OnHover() {

    }

    public void OnUnhover() {

    }
}

[Serializable]
public class SerializableInterestPoint {
    public Vector3 position;
    public Vector3 eulerRotation;
}