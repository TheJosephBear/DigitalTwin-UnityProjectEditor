using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterestPoint : EditorObjectBase, IClickable {

    public GameObject vcam;

    public void OnClick() {
    //    print("im clicked and the editor viewmode is: "+ EditorManager.Instance.ViewModeCurrent);
        if (EditorManager.Instance.ViewModeCurrent != EditorViewMode.showingOffCamera) {
            GizmoManager.Instance.SetTargetGameObject(gameObject);
            GizmoManager.Instance.ShowUniversalGizmo();
            InterestPointManager.Instance.SetActiveInterestPoint(this);
        }
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
}

[Serializable]
public class SerializableInterestPoint {
    public Vector3 position;
    public Vector3 eulerRotation;
}