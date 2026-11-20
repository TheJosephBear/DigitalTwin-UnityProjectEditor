using System;
using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;

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
    [Range(0.0000001f, 0.5f)]
    public float MinPanSpeed = 0.1f;
    public float PanSlowDistance = 10f;

    public LayerMask GroundGeometryLayers;
    [Tooltip("RTG multiplies pan/zoom by the camera's distance to its focus point, kept this far in front of the camera so the speed is the same everywhere (350 = what the editor had at its initial view)")]
    public float SpeedReferenceDistance = 350f;

    [Header("Viewer")]
    [Tooltip("Degrees per mouse axis unit when rotating with a left click drag in the viewer")]
    public float LeftDragLookSensitivity = 3f;

    [Header("Rise until visible")]
    [Tooltip("[min, max] units per second the camera rises at when a clicked pin is hidden behind geometry. Starts at min and speeds up to max over RiseRampTime")]
    [MinMaxSlider(0f, 200f)]
    public Vector2 RiseSpeed = new Vector2(30f, 100f);
    [Tooltip("Seconds the rise takes to speed up from the min to the max speed")]
    public float RiseRampTime = 1f;
    [Tooltip("Degrees per second the camera turns toward the pin while rising")]
    public float RiseTurnSpeed = 120f;
    [Tooltip("The camera stops rising after this many units even when the pin is still hidden")]
    public float MaxRiseHeight = 500f;

    // Bounds tracking
    private Bounds _mapBounds;
    private bool _hasBounds = false;
    private bool _isCollisionEnabled;
    private bool _canUpdateFreecam;
    private bool _canEnforceBounds;
    public bool IsCollisionEnabled {
        get { return _isCollisionEnabled; }
        private set { _isCollisionEnabled = value; }
    }

    CinemachineBrain _cinemachineBrainRefference;
    GameObject _vCamFreeCamRefference;
    Transform _freeCamCameraTransform;
    MainManagerBase _editorManager;

    Coroutine _disableCoroutine;
    Coroutine _flyToVcamCoroutine;
    GameObject _flyToVcamTarget;
    bool _isFlyingToVcam;
    const int FlyToVcamPriority = 1000; // above any vcam priority set in the project
    int _freeCamPriority;
    int _flyToVcamTargetPriority;
    bool _isLeftDragLooking;
    Coroutine _riseCoroutine;

    protected override void Awake() {
        base.Awake();

        _cinemachineBrainRefference = FindAnyObjectByType<CinemachineBrain>();

        // The free cam belongs to this manager's own scene (Editing or Viewing). MainManagerBase.Instance
        // can't be used here: Awake order between scene objects isn't guaranteed, so it may not be set yet.
        _vCamFreeCamRefference = Instantiate(VcamFreeCamPrefab, InitialCameraPosition.position, InitialCameraPosition.rotation);
        if (_vCamFreeCamRefference.scene != gameObject.scene) {
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(_vCamFreeCamRefference, gameObject.scene);
        }
    }

    /*
    IEnumerator WaitForGlobalSettingsLoad() {
        yield return new WaitUntil(() => GlobalSettings.Instance != null && GlobalSettings.Instance.IsLoaded);

        ToggleCameraCollision(GlobalSettings.Instance.Data.IsCameraCollisionOn);
    }
    */

    private void Update() {
        if (_canUpdateFreecam) {
            UpdateFreeCamTransform();
            if (MainManagerBase.Instance is ViewingManager) LeftDragLook();
        }

        UpdateCameraSpeed();
    }

    /// <summary>
    /// Viewer only: rotating with a left click drag, like Google Street View - the view follows the cursor as if dragging it.
    /// Easier on a touchpad than holding the right button.
    /// </summary>
    void LeftDragLook() {
        // Only drags that start in the scene, not on the UI
        if (Input.GetMouseButtonDown(0)) {
            _isLeftDragLooking = EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject();
        }
        if (!Input.GetMouseButton(0)) {
            _isLeftDragLooking = false;
            return;
        }
        if (!_isLeftDragLooking) return;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        if (mouseX == 0f && mouseY == 0f) return;

        // Opposite of the right click look: dragging right turns the view left, dragging up tilts it down
        GizmoManager.Instance.RTFocusCameraScript.LookAround(mouseY * LeftDragLookSensitivity, -mouseX * LeftDragLookSensitivity);
    }

    // LateUpdate guarantees we enforce bounds AFTER the external script moves the camera
    private void LateUpdate() {
        if (_canEnforceBounds) {
            EnforceCameraBounds();
        }
    }

    public void ToggleFreecamUpdating(bool toggleOn) {
        _canUpdateFreecam = toggleOn;
    }

    public void ToggleBoundsEnforcing(bool toggleOn) {
        _canEnforceBounds = toggleOn;
    }

    public void ToggleCameraCollision(bool toggleOn) {
        //    GlobalSettings.Instance.Set(a => a.IsCameraCollisionOn = toggleOn);
        CameraColissionScriptRef.enabled = toggleOn;
        _isCollisionEnabled = CameraColissionScriptRef.enabled;
        ProjectSettingsManager.Instance.EditorCameraCollision = toggleOn;
    }

    public void InitializeFreeCamBounds(GameObject baseMapGameObject) {
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
        GizmoManager.Instance.RTFocusCameraScript.SetSpeedReferencePoint(camTransform.position + camTransform.forward * SpeedReferenceDistance);
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

        _vCamFreeCamRefference.transform.position = _freeCamCameraTransform.position;
        _vCamFreeCamRefference.transform.rotation = _freeCamCameraTransform.rotation;
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

    /// <summary>
    /// Blends the main camera from where it is to the (inactive) vcam with the default Cinemachine blend,
    /// then puts the free cam there and turns the vcam and the brain off, so the camera can be moved again.
    /// </summary>
    public void FlyToVcam(GameObject vcam) {
        StopRise();
        CancelFlyToVcam();
        if (vcam == null || Camera.main == null) return;

        var freeVcam = _vCamFreeCamRefference.GetComponent<CinemachineVirtualCameraBase>();
        var targetVcam = vcam.GetComponent<CinemachineVirtualCameraBase>();

        // The brain cuts to the highest priority active vcam. Any other vcam left on (e.g. a view point's, priority 10 like the pins')
        // beats the free cam (priority 0), so the camera jumped there first and blended from it. Lifted above everything for the flight:
        // the free cam for the cut, the target for the blend. Restored in CancelFlyToVcam
        _freeCamPriority = freeVcam.Priority;
        _flyToVcamTargetPriority = targetVcam != null ? targetVcam.Priority : 0;
        freeVcam.Priority = FlyToVcamPriority;
        if (targetVcam != null) targetVcam.Priority = FlyToVcamPriority + 1;

        // Fresh brain (disabling clears its frame stack) starting from the free cam at the current view
        _cinemachineBrainRefference.enabled = false;
        _vCamFreeCamRefference.transform.SetPositionAndRotation(Camera.main.transform.position, Camera.main.transform.rotation);
        ToggleCinemachineBrain(true);
        _cinemachineBrainRefference.ManualUpdate();

        _isFlyingToVcam = true;
        _flyToVcamTarget = vcam;
        // Like ViewactiveState: collision off while flying, else the enforcer pulls the camera back to where it got stuck once the blend ends.
        // Only the component, the collision setting stays as it is
        CameraColissionScriptRef.enabled = false;
        vcam.SetActive(true);
        _flyToVcamCoroutine = StartCoroutine(FlyToVcamCoroutine(targetVcam));
    }

    IEnumerator FlyToVcamCoroutine(CinemachineVirtualCameraBase vcam) {
        // The brain picks up the vcam on its next update
        while (vcam != null && (!_cinemachineBrainRefference.IsLive(vcam) || _cinemachineBrainRefference.IsBlending)) {
            yield return null;
        }

        _flyToVcamCoroutine = null;
        CancelFlyToVcam();
    }

    /// <summary>
    /// Stops at the current view (or the vcam when the blend is done), gives the camera back to the free cam.
    /// </summary>
    public void CancelFlyToVcam() {
        if (!_isFlyingToVcam) return;
        _isFlyingToVcam = false;

        if (_flyToVcamCoroutine != null) {
            StopCoroutine(_flyToVcamCoroutine);
            _flyToVcamCoroutine = null;
        }

        _vCamFreeCamRefference.transform.SetPositionAndRotation(Camera.main.transform.position, Camera.main.transform.rotation);
        _vCamFreeCamRefference.GetComponent<CinemachineVirtualCameraBase>().Priority = _freeCamPriority;
        if (_flyToVcamTarget != null) { // null when its pin got deleted
            var targetVcam = _flyToVcamTarget.GetComponent<CinemachineVirtualCameraBase>();
            if (targetVcam != null) targetVcam.Priority = _flyToVcamTargetPriority;
            _flyToVcamTarget.SetActive(false);
        }
        _flyToVcamTarget = null;
        ToggleCinemachineBrain(false);
        CameraColissionScriptRef.enabled = _isCollisionEnabled; // enabling it resets its last valid position to where the camera is now
    }

    /// <summary>
    /// Moves the free cam straight up while turning it to look at the target, until isHidden() returns false,
    /// then finishes turning. Also stops at MaxRiseHeight, at the top of the camera bounds, or when the user presses a key/button or scrolls.
    /// </summary>
    public void RiseUntilVisible(Transform target, Func<bool> isHidden) {
        StopRise();
        CancelFlyToVcam();
        if (target == null || Camera.main == null) return;

        // Like FlyToVcam: the enforcer would pull the camera back down under a roof. Only the component, the setting stays
        CameraColissionScriptRef.enabled = false;
        _riseCoroutine = StartCoroutine(RiseCoroutine(target, isHidden));
    }

    public void StopRise() {
        if (_riseCoroutine == null) return;
        StopCoroutine(_riseCoroutine);
        EndRise();
    }

    void EndRise() {
        _riseCoroutine = null;
        CameraColissionScriptRef.enabled = _isCollisionEnabled; // enabling it resets its last valid position to where the camera is now
    }

    IEnumerator RiseCoroutine(Transform target, Func<bool> isHidden) {
        Transform cam = Camera.main.transform;
        float startY = cam.position.y;
        float expectedY = cam.position.y;
        bool rising = true;
        float elapsed = 0f;

        yield return null; // the click's own input isn't the user taking over

        while (target != null && cam != null) {
            if (Input.anyKeyDown || Input.mouseScrollDelta.y != 0f) break; // the user takes over

            // Occlusion is updated when the pin renders, so it reflects last frame's position.
            // Lower than where it was moved = clamped by the camera bounds, it can't go higher
            if (rising) rising = isHidden() && cam.position.y - startY < MaxRiseHeight && cam.position.y >= expectedY - 0.001f;
            if (rising) {
                elapsed += Time.deltaTime;
                float speed = Mathf.Lerp(RiseSpeed.x, RiseSpeed.y, RiseRampTime > 0f ? elapsed / RiseRampTime : 1f);
                cam.position += Vector3.up * (speed * Time.deltaTime);
                expectedY = cam.position.y;
            }

            Vector3 toTarget = target.position - cam.position;
            if (toTarget.sqrMagnitude < 0.0001f) break;
            Quaternion look = Quaternion.LookRotation(toTarget, Vector3.up);
            cam.rotation = Quaternion.RotateTowards(cam.rotation, look, RiseTurnSpeed * Time.deltaTime);

            if (!rising && Quaternion.Angle(cam.rotation, look) < 0.1f) break;
            yield return null;
        }

        EndRise();
    }

    public GameObject GetFreeCamVcam() {
        return _vCamFreeCamRefference;
    }

    #region Framing

    /// <summary>
    /// Puts the free cam where the whole model is in view, looking at it at the InitialCameraPosition's angle.
    /// Used as the default camera position when a project opens (the scene's position isn't tied to the model).
    /// </summary>
    public void FrameFreeCamOn(GameObject model) {
        if (!TryGetFramingPose(model, out Vector3 position, out Quaternion rotation)) return;

        _vCamFreeCamRefference.transform.SetPositionAndRotation(position, rotation);
        // The brain blends to the free vcam only when it's on, the free cam itself moves the main camera
        if (Camera.main != null) Camera.main.transform.SetPositionAndRotation(position, rotation);
    }

    /// <summary>
    /// Pose looking at the model at the InitialCameraPosition's angle, from the distance where all of its bounds fit
    /// in the free cam's view (with some padding). False when the model has no geometry.
    /// </summary>
    public bool TryGetFramingPose(GameObject model, out Vector3 position, out Quaternion rotation, float padding = 1.1f) {
        position = Vector3.zero;
        rotation = InitialCameraPosition != null ? InitialCameraPosition.rotation : Quaternion.Euler(45f, 0f, 0f);
        if (model == null || !FileLoadingManager.TryCalculateBounds(model, out Bounds bounds)) return false;

        var lens = _vCamFreeCamRefference != null ? _vCamFreeCamRefference.GetComponent<CinemachineVirtualCamera>() : null;
        float fov = lens != null ? lens.m_Lens.FieldOfView : (Camera.main != null ? Camera.main.fieldOfView : 60f);
        float aspect = Camera.main != null ? Camera.main.aspect : 16f / 9f;
        float tanVertical = Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
        float tanHorizontal = tanVertical * aspect;

        // Each corner of the bounds, in the camera's space around the center, must be inside the view:
        // |x| <= (distance + z) * tanHorizontal, |y| <= (distance + z) * tanVertical  (z toward the camera is negative)
        Quaternion inverse = Quaternion.Inverse(rotation);
        float distance = 0f;
        for (int i = 0; i < 8; i++) {
            Vector3 corner = new Vector3(
                (i & 1) == 0 ? -bounds.extents.x : bounds.extents.x,
                (i & 2) == 0 ? -bounds.extents.y : bounds.extents.y,
                (i & 4) == 0 ? -bounds.extents.z : bounds.extents.z);
            Vector3 local = inverse * corner;
            distance = Mathf.Max(distance,
                Mathf.Abs(local.x) * padding / tanHorizontal - local.z,
                Mathf.Abs(local.y) * padding / tanVertical - local.z);
        }

        position = bounds.center - rotation * Vector3.forward * distance;
        return true;
    }

    #endregion

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
