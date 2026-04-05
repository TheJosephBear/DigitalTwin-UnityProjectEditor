using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour {

    public GameObject VcamFreeCamPrefab;
    public Transform InitialCameraPosition;

    CinemachineBrain _cinemachineBrainRefference;
    GameObject vCamFreeCamRefference;
    Transform _freeCamCameraTransform;
    MainManagerBase _editorManager;

    private void Awake() {
        _cinemachineBrainRefference = FindAnyObjectByType<CinemachineBrain>();

        vCamFreeCamRefference = SceneLoadingManager.Instance.InstantiateObjectInScene(VcamFreeCamPrefab, InitialCameraPosition.position, MainManagerBase.Instance.SceneType);
        vCamFreeCamRefference.transform.rotation = InitialCameraPosition.rotation;
    //    DisableCinemachineAfterTransition();
    }

    private void Update() {
        if (_editorManager == null) {
            _editorManager = MainManagerBase.Instance;
        }
        if (_editorManager.ActiveState == AppState.Freecam) {
            UpdateFreeCamTransform();
        }
    }

    public Transform GetFreeCamTransform() {
        return _freeCamCameraTransform;
    }

    void UpdateFreeCamTransform() {
        _freeCamCameraTransform = Camera.main.transform;
    }

    public void UpdateFreeCamVcamPosition() {
        if (_freeCamCameraTransform == null) return;
        // free cam vcam is used for coming back to a preffered spot in freecam mode
        vCamFreeCamRefference.transform.position = _freeCamCameraTransform.position;
        vCamFreeCamRefference.transform.rotation = _freeCamCameraTransform.rotation;
        
    }

    public void ToggleCinemachineBrain(bool toggleOn) {
        _cinemachineBrainRefference.enabled = toggleOn;
    }

    public void DisableCinemachineAfterTransition() {
        StartCoroutine(DisableCinemachineAfterTransitionCoroutine());
    }

    IEnumerator DisableCinemachineAfterTransitionCoroutine() {
        yield return new WaitForSeconds(0.01f); // Wait so the blend can start
        while (_cinemachineBrainRefference.IsBlending) {
            yield return null;
        }
        _cinemachineBrainRefference.enabled = (false);
    }

}
