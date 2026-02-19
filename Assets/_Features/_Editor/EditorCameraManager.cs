using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class EditorCameraManager : MonoBehaviour {

    public GameObject vCamFreeCamRefference;
    public CinemachineBrain CinemachineBrainRefference;

    Transform FreeCamCameraTransform;
    MainManagerBase editorManager;

    private void Update() {
        if (editorManager == null) {
            editorManager = MainManagerBase.Instance;
        }
        if (editorManager.ActiveState == ProjectState.Freecam) {
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

    public void ToggleCinemachineBrain(bool toggleOn) {
        CinemachineBrainRefference.enabled = toggleOn;
    }

    public void DisableCinemachineAfterTransition() {
        StartCoroutine(DisableCinemachineAfterTransitionCoroutine());
    }

    IEnumerator DisableCinemachineAfterTransitionCoroutine() {
        yield return new WaitForSeconds(0.01f); // Wait so the blend can start
        while (CinemachineBrainRefference.IsBlending) {
            yield return null;
        }
        CinemachineBrainRefference.enabled = (false);
    }

}
