using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraManager : Singleton<CameraManager> {

    public GameObject VcamFreeCamPrefab;
    public Transform InitialCameraPosition;

    [Header("Bounds Settings")]
    [Tooltip("Extra padding added to the base map bounds (X, Y, Z).")]
    public Vector3 boundsPadding = new Vector3(5f, 5f, 5f);
    public bool enableBoundsClamping = true;

    // Bounds tracking
    private Bounds _mapBounds;
    private bool _hasBounds = false;

    CinemachineBrain _cinemachineBrainRefference;
    GameObject vCamFreeCamRefference;
    Transform _freeCamCameraTransform;
    MainManagerBase _editorManager;

    Coroutine _disableCoroutine;

    protected override void Awake() {
        base.Awake();

        _cinemachineBrainRefference = FindAnyObjectByType<CinemachineBrain>();
        print(_cinemachineBrainRefference.name);

        vCamFreeCamRefference = SceneLoadingManager.Instance.InstantiateObjectInScene(VcamFreeCamPrefab, InitialCameraPosition.position, MainManagerBase.Instance.SceneType);
        vCamFreeCamRefference.transform.rotation = InitialCameraPosition.rotation;
        //  DisableCinemachineAfterTransition();
    }

    private void Update() {
        if (_editorManager == null) {
            _editorManager = MainManagerBase.Instance;
        }

        if (_editorManager != null && _editorManager.ActiveState == AppState.Freecam) {
            UpdateFreeCamTransform();
        }
    }

    // LateUpdate guarantees we enforce bounds AFTER the external script moves the camera
    private void LateUpdate() {
        if (_editorManager != null && _editorManager.ActiveState == AppState.Freecam) {
            EnforceCameraBounds();
        }
    }

    public void InitializeFreeCamBounds() {
        GameObject baseMapGameObject = MapManager.Instance?.GetBaseMap()?.gameObject;

        if (baseMapGameObject != null) {
            // Calculate combined bounds across all renderers on the base map
            Renderer[] renderers = baseMapGameObject.GetComponentsInChildren<Renderer>();

            if (renderers.Length > 0) {
                Bounds rawBounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++) {
                    rawBounds.Encapsulate(renderers[i].bounds);
                }

                // Define custom min/max bounds manually
                Vector3 min = new Vector3(
                    rawBounds.min.x - boundsPadding.x,
                    rawBounds.min.y,                   // No padding applied to the bottom (negative Y)
                    rawBounds.min.z - boundsPadding.z
                );

                Vector3 max = new Vector3(
                    rawBounds.max.x + boundsPadding.x,
                    rawBounds.max.y + boundsPadding.y, // Top padding still applies
                    rawBounds.max.z + boundsPadding.z
                );

                // Reconstruct the bounds from the new min/max
                Bounds paddedBounds = new Bounds();
                paddedBounds.SetMinMax(min, max);

                _mapBounds = paddedBounds;
                _hasBounds = true;
            } else {
                Debug.LogWarning("[CameraManager] Base map GameObject has no renderers to calculate bounds from.");
            }
        }
    }

    private void EnforceCameraBounds() {
        if (!_hasBounds || !enableBoundsClamping || _freeCamCameraTransform == null) return;

        Vector3 currentPos = _freeCamCameraTransform.position;

        // Clamp position within min and max allowed vectors
        Vector3 clampedPos = new Vector3(
            Mathf.Clamp(currentPos.x, _mapBounds.min.x, _mapBounds.max.x),
            Mathf.Clamp(currentPos.y, _mapBounds.min.y, _mapBounds.max.y),
            Mathf.Clamp(currentPos.z, _mapBounds.min.z, _mapBounds.max.z)
        );

        // Re-assign clamped position back to the camera
        _freeCamCameraTransform.position = clampedPos;
        Camera.main.transform.position = clampedPos;   
        // print("trying to set the free cam position");
    }

    public Transform GetFreeCamTransform() {
        return _freeCamCameraTransform;
    }

    void UpdateFreeCamTransform() {
        if (Camera.main != null) {
            _freeCamCameraTransform = Camera.main.transform;
        }
    }

    public void UpdateFreeCamVcamPosition() {
        if (_freeCamCameraTransform == null) return;

        // Ensure virtual camera position updates to the clamped camera position
        vCamFreeCamRefference.transform.position = _freeCamCameraTransform.position;
        vCamFreeCamRefference.transform.rotation = _freeCamCameraTransform.rotation;
    }

    public void ToggleCinemachineBrain(bool toggleOn) {
        if (toggleOn) {
            if (_disableCoroutine != null) {
                StopCoroutine(_disableCoroutine);
                _disableCoroutine = null;
            }
        }
        _cinemachineBrainRefference.enabled = toggleOn;
    }

    public void DisableCinemachineAfterTransition() {
        if (_disableCoroutine != null) {
            StopCoroutine(_disableCoroutine);
        }
        _disableCoroutine = StartCoroutine(DisableCinemachineAfterTransitionCoroutine());
    }

    IEnumerator DisableCinemachineAfterTransitionCoroutine() {
        yield return new WaitForSeconds(0.01f);

        while (_cinemachineBrainRefference.IsBlending) {
            yield return null;
        }

        _cinemachineBrainRefference.enabled = false;

        // Clear the reference now that it's finished
        _disableCoroutine = null;
    }

    public GameObject GetFreeCamVcam() {
        return vCamFreeCamRefference;
    }
}