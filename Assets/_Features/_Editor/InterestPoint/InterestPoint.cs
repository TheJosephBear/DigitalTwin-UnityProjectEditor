using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterestPoint : MonoBehaviour, IClickable {

    public GameObject vcam;
    public string Name { get; private set; }


    public void Rename(string newName) {
        Name = newName;
    }

    public void OnClick() {
        print("im clicked and the editor viewmode is: "+ EditorManager.Instance.ViewModeCurrent);
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

}
