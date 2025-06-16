using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorCameraManager : Singleton<EditorCameraManager> {

    Transform FreeCamCameraTransform;
    EditorManager editorManager;


    private void Update() {
        if (editorManager == null) {
            editorManager = EditorManager.Instance;
        }
        if (editorManager.EditorModeCurrent == EditorMode.Freecam) {
            UpdateFreeCamTransform();
        }
    }

    public Transform GetFreeCamTransform() {
        return FreeCamCameraTransform;
    }

    void UpdateFreeCamTransform() {
        FreeCamCameraTransform = Camera.main.transform;
    }


}
