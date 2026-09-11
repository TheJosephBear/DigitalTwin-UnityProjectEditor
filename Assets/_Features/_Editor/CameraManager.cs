using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraManager: Singleton<CameraManager> {

    public CameraCollisionEnforcer CameraColissionScriptRef;
    public GameObject VcamFreeCamPrefab;
    public Transform InitialCameraPosition;

    [Header("Bounds")]
    public Vector3 BoundsPadding;
    public bool EnableBoundsClamping = true;

    [Header("Camera & Movement")]
    [Range(0.1f, 10f)]
    public float MaxScrollSpeed = 0.9f;
    [Range(0f, 0.5f)]
    public float MinScrollSpeed = 0.2f;
    public float ScrollSlowDistance = 10f;

    [Header("Pan Speed Settings")]
    [Range(0.1f, 10f)]
    public float MaxPanSpeed = 1.0f;
    [Range(0.0001f, 0.5f)]
    public float MinPanSpeed = 0.1f;
    public float PanSlowDistance = 10f;

    public LayerMask GroundGeometryLayers;

    // Bounds tracking
    private Bounds _mapBounds;
    private bool _hasBounds = false;
    private bool _isCollisionEnabled;
    public bool IsCollisionEnabled {
        get { return _isCollisionEnabled; }
        private set { _isCollisionEnabled = value; }
    }

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

        StartCoroutine(WaitForGlobalSettingsLoad());
    }

    IEnumerator WaitForGlobalSettingsLoad() {
        yield return new WaitUntil(() => GlobalSettings.Instance != null && GlobalSettings.Instance.IsLoaded);

        ToggleCameraCollision(GlobalSettings.Instance.Data.IsCameraCollisionOn);
    }

    private void Update() {
        if (_editorManager == null) {
            _editorManager = MainManagerBase.Instance;
        }

        if (_editorManager != null && (_editorManager.ActiveState == AppState.Freecam || _editorManager.ActiveState == AppState.Survey)) {
            UpdateFreeCamTransform();
        }

        UpdateCameraSpeed();
    }

    // LateUpdate guarantees we enforce bounds AFTER the external script moves the camera
    private void LateUpdate() {
        if (_editorManager != null && (_editorManager.ActiveState == AppState.Freecam || _editorManager.ActiveState == AppState.Survey)) {
            EnforceCameraBounds();
        }
    }

    public void ToggleCameraCollision(bool toggleOn) {
        GlobalSettings.Instance.Set(a => a.IsCameraCollisionOn = toggleOn);
        CameraColissionScriptRef.enabled = toggleOn;
        _isCollisionEnabled = CameraColissionScriptRef.enabled;
    }

    public void InitializeFreeCamBounds() {
        GameObject baseMapGameObject = MapManager.Instance?.GetBaseMap()?.gameObject;

        if (baseMapGameObject == null) return;

        Renderer[] renderers = baseMapGameObject.GetComponentsInChildren<Renderer>();

        if (renderers.Length > 0) {
            Bounds rawBounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) {
                rawBounds.Encapsulate(renderers[i].bounds);
            }

            Vector3 halfSize = rawBounds.size * 0.5f;

            Camera targetCam = Camera.main;
            float verticalFOV = targetCam.fieldOfView;
            float aspect = targetCam.aspect;

            float requiredDistForHeight = (rawBounds.size.z * 0.5f) / Mathf.Tan(verticalFOV * 0.5f * Mathf.Deg2Rad);
            float horizontalFOV = 2f * Mathf.Atan(Mathf.Tan(verticalFOV * 0.5f * Mathf.Deg2Rad) * aspect);
            float requiredDistForWidth = (rawBounds.size.x * 0.5f) / Mathf.Tan(horizontalFOV * 0.5f);

            float viewDistancePadding = Mathf.Max(requiredDistForHeight, requiredDistForWidth);

            Vector3 min = new Vector3(
                rawBounds.min.x - halfSize.x - BoundsPadding.x,
                rawBounds.min.y,
                rawBounds.min.z - halfSize.z - BoundsPadding.z
            );

            Vector3 max = new Vector3(
                rawBounds.max.x + halfSize.x + BoundsPadding.x,
                rawBounds.max.y + viewDistancePadding + BoundsPadding.y,
                rawBounds.max.z + halfSize.z + BoundsPadding.z
            );

            Bounds paddedBounds = new Bounds();
            paddedBounds.SetMinMax(min, max);

            _mapBounds = paddedBounds;
            _hasBounds = true;
        } else {
            Debug.LogWarning("[CameraManager] Base map GameObject has no renderers to calculate bounds from.");
        }
    }

    void UpdateCameraSpeed() {
        Transform camTransform = Camera.main.transform;
        float currentDistance = ScrollSlowDistance; // Default to max distance if nothing is hit

        Ray ray = new Ray(camTransform.position, camTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, ScrollSlowDistance, GroundGeometryLayers)) {
            currentDistance = hit.distance;
        }

        // Map distance (0 to ScrollSlowDistance) to t (0 to 1)
        float t = Mathf.Clamp01(currentDistance / ScrollSlowDistance);
        float tPan = Mathf.Clamp01(currentDistance / PanSlowDistance);

        // Smoothly transition speeds based on distance
        float newScrollSpeed = Mathf.Lerp(MinScrollSpeed, MaxScrollSpeed, t);
        float newPanSpeed = Mathf.Lerp(MinPanSpeed, MaxPanSpeed, t);

        GizmoManager.Instance.SetFreecamScrollSpeed(newScrollSpeed);
        GizmoManager.Instance.SetFreecamPanSpeed(newPanSpeed);
    }

    private void EnforceCameraBounds() {
        if (!_hasBounds || !EnableBoundsClamping || _freeCamCameraTransform == null) return;

        Vector3 currentPos = _freeCamCameraTransform.position;

        Vector3 clampedPos = new Vector3(
            Mathf.Clamp(currentPos.x, _mapBounds.min.x, _mapBounds.max.x),
            Mathf.Clamp(currentPos.y, _mapBounds.min.y, _mapBounds.max.y),
            Mathf.Clamp(currentPos.z, _mapBounds.min.z, _mapBounds.max.z)
        );

        _freeCamCameraTransform.position = clampedPos;
        Camera.main.transform.position = clampedPos;
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
        _disableCoroutine = null;
    }

    public GameObject GetFreeCamVcam() {
        return vCamFreeCamRefference;
    }

    public void ToggleVcamVisbility(bool toggleOn) {
        int layerIndex = LayerMask.NameToLayer("InterestPoint");
        int layerMask = 1 << layerIndex;

        if (toggleOn) {
            Camera.main.cullingMask |= layerMask;
        } else {
            Camera.main.cullingMask &= ~layerMask;
        }
    }
}
