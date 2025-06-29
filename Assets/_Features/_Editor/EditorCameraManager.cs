using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorCameraManager : Singleton<EditorCameraManager> {

    public GameObject vCamFreeCamRefference;

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

    public void UpdateFreeCamVcamPosition() {
        if (FreeCamCameraTransform == null) return;
        // free cam vcam is used for coming back to a preffered spot in freecam mode
        vCamFreeCamRefference.transform.position = FreeCamCameraTransform.position;
        vCamFreeCamRefference.transform.rotation = FreeCamCameraTransform.rotation;
        
    }


}
