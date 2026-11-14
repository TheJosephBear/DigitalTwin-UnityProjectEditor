using System;
using System.Collections;
using System.Collections.Generic;
using RTG;
using Unity.VisualScripting;
using UnityEngine;
public class GeoMapLocalizationManager: Singleton<GeoMapLocalizationManager> {
    /// <summary>
    /// Handles the process of localization
    /// Keeps result data and settings
    /// </summary>

    public GameObject GeoLocalizationUIPrefab;
    public Vector3 MapCenterPosition;
    public float ModelTransparency = 0.8f;
    [HideInInspector]
    public bool InitialLocalization = false;

    [Tooltip("See-through material of the model's copy while placing it (GeoPreviewTransparent shader), referenced here so builds include its shader")]
    [SerializeField] Material _previewMaterial;

    const string PreviewShaderName = "GeoMapLocalization/GeoPreviewTransparent";
    static readonly int MainTexId = Shader.PropertyToID("_MainTex");
    static readonly int ColorId = Shader.PropertyToID("_Color");
    static readonly int OpacityId = Shader.PropertyToID("_Opacity");

    [Tooltip("Part of the screen on each side the model's center can't be dragged into, so its gizmo stays on screen")]
    [Range(0f, 0.45f)]
    [SerializeField] float _screenEdgeMargin = 0.1f;

    struct FollowerPose {
        public Transform Target;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Normal;    // pins: turned through SurfaceNormal instead of the rotation
        public PinObject Pin;
        public MapVariant Variant; // variants also store their transform when placed
    }

    GameObject _baseMapCopy;
    Vector3 _copyCenterLocal; // center of the copy's bounds in its local space, where the gizmo sits
    readonly List<Material> _previewMaterials = new List<Material>(); // created for the copy, destroyed with it
    GeoLocalizationData _geoData;
    GeoLocalizationUI _UIInstance;
    bool _lastLockToggle = false;
    bool _firstOpen = false; // the new project's first placing: the camera has no view of the model to keep yet
    float _opacity = -1f; // the copy's opacity, ModelTransparency until the slider changes it (kept for the next opening)
    float Opacity => _opacity >= 0f ? _opacity : ModelTransparency;

    // Pins, view points and map variants follow the copy while placing: their poses when geolocalization opened,
    // relative to the base map's pose then. Restored on exit without placing, kept when the model is placed.
    readonly List<FollowerPose> _followers = new List<FollowerPose>();
    Vector3 _referencePosition;
    Quaternion _referenceRotation = Quaternion.identity;
    bool _followingCopy = false;


    protected override void Awake() {
        base.Awake();
    }

    public void Setup(bool firstOpen = true) {
        _firstOpen = firstOpen;
        ToggleUI(true);
        _UIInstance.Initialize(firstOpen);
        _UIInstance.SetOpacityVisual(Opacity);
        GizmoManager.Instance.HideGizmo();
        ToggleGeoMapZoom(true);
        GeoMapManager.Instance.ToggleGeoMapControl(true);

        if (MapManager.Instance.IsBaseMapUploaded()) {
            if (_baseMapCopy == null) {
                CreateBaseMapCopy();
            } else {
                // Starts where the model is, a previous session may have left it elsewhere without placing it
                _baseMapCopy.SetActive(true);
                ResetCopyToBaseMap();
                HideBaseMap();
            }
        }

        CaptureFollowers();

        if (_baseMapCopy != null) {
            ToggleLock(true);
            FocusVcamOnBaseMapCopy();
        } else {
            GeoMapManager.Instance.ResetVcamHeight();
        }
    }

    public void UploadMap(ModelAsset uploadedModel) {
        if (_baseMapCopy != null) {
            Destroy(_baseMapCopy.gameObject);
            _baseMapCopy = null;
            DestroyPreviewMaterials();
        }

        MapManager.Instance.SetBaseMapModel(uploadedModel);
        CreateBaseMapCopy();
        // The pins, views and variants now move relative to the new model
        if (_followingCopy) SetFollowerReference();
        else CaptureFollowers();
        ToggleLock(true);
        FocusVcamOnBaseMapCopy();
    }

    void FocusVcamOnBaseMapCopy() {
        if (_baseMapCopy != null && FileLoadingManager.TryCalculateBounds(_baseMapCopy, out Bounds bounds)) {
            GeoMapManager.Instance.FocusVcamOnBounds(bounds);
        }
    }

    void ToggleUI(bool toggleOn) {
        if (_UIInstance == null) {
            _UIInstance = SceneLoadingManager.Instance.InstantiateObjectInScene(GeoLocalizationUIPrefab).GetComponent<GeoLocalizationUI>();
        }
        _UIInstance.gameObject.SetActive(toggleOn);
    }

    void CreateBaseMapCopy() {
        if (_baseMapCopy == null && MapManager.Instance.IsBaseMapUploaded()) {
            MapVariant baseMapReff = MapManager.Instance.GetBaseMap();
            if (!baseMapReff.IsVisible) {
                baseMapReff.ToggleMeshVisibility(true);
                baseMapReff.gameObject.SetActive(true);
            }

            // Vector3 spawnPosition = MapCenterPosition;
            Vector3 spawnPosition = baseMapReff.transform.position;

            _baseMapCopy = SceneLoadingManager.Instance.InstantiateObjectInScene(
                MapManager.Instance.GetBaseMap().ModelAsset.ModelGameObject,
                spawnPosition
            );

            _baseMapCopy.SetActive(true); // the loaded model is kept deactivated
            ResetCopyToBaseMap();

            // Add mesh collider
            foreach (Transform child in _baseMapCopy.GetComponentsInChildren<Transform>()) {
                if (child.GetComponent<MeshRenderer>() != null || child.GetComponent<MeshFilter>() != null) {
                    child.gameObject.AddComponent<MeshCollider>();
                }
            }

            // Also make it transparent
            ApplyPreviewMaterials();
            SetPreviewOpacity(Opacity);

            // Add movable component for clickability
            BaseMapCopyMovable movableScript = _baseMapCopy.AddComponent<BaseMapCopyMovable>();

            // Setup gizmo axes and restrictions
            List<GizmoAxis> axes = new List<GizmoAxis> { GizmoAxis.X, GizmoAxis.Y, GizmoAxis.Z };
            movableScript.ShownAxis = axes;
            movableScript.MovableType = GizmoType.Universal;

            HideBaseMap();
        }
    }

    // The copy stands in for the model while placing. Deactivated, not only hidden: its colliders sit where the copy is
    // and would catch the clicks meant for the copy, so its gizmo never showed
    void HideBaseMap() {
        MapVariant baseMap = MapManager.Instance.GetBaseMap();
        if (baseMap == null) return;
        baseMap.ToggleMeshVisibility(false);
        baseMap.gameObject.SetActive(false);
    }

    /// <summary>
    /// Puts the copy on the base map's pose, lifted (or lowered) so its lowest geometry point is 1 unit above the geo map.
    /// </summary>
    void ResetCopyToBaseMap() {
        MapVariant baseMap = MapManager.Instance.GetBaseMap();
        if (_baseMapCopy == null || baseMap == null) return;

        _baseMapCopy.transform.SetPositionAndRotation(baseMap.transform.position, baseMap.transform.rotation);

        if (FileLoadingManager.TryCalculateBounds(_baseMapCopy, out Bounds copyBounds)) {
            float targetLowestY = GeoMapManager.Instance.OnlineMapsReff.transform.position.y + 1f;
            _baseMapCopy.transform.position += Vector3.up * (targetLowestY - copyBounds.min.y);
            _copyCenterLocal = _baseMapCopy.transform.InverseTransformPoint(copyBounds.center + Vector3.up * (targetLowestY - copyBounds.min.y));
        } else {
            _copyCenterLocal = Vector3.zero;
        }
    }

    public void ZoomMap(float value) {
        GeoMapManager.Instance.ZoomMap(value);
    }

    public void Exit() {
        GizmoManager.Instance.HideGizmo();

        // Leaving without placing puts the pins, views and variants back
        if (_followingCopy) RestoreFollowers();
        _followingCopy = false;
        _followers.Clear();

        if (_baseMapCopy != null) _baseMapCopy.SetActive(false);
        MapVariant baseMap = MapManager.Instance.GetBaseMap();
        if (baseMap != null) {
            baseMap.ToggleMeshVisibility(true);
            baseMap.gameObject.SetActive(true);
        }
        ToggleUI(false);
    }

    void LateUpdate() {
        if (_baseMapCopy == null || !_baseMapCopy.activeInHierarchy) return;
        KeepCopyOnScreen();
        if (_followingCopy) MoveFollowersWithCopy();
    }

    /// <summary>
    /// While the copy is dragged by its gizmo, its center (where the gizmo is) can't leave the screen,
    /// otherwise the gizmo is out of reach. Only while dragging, so the camera blending in on entering doesn't push it.
    /// </summary>
    void KeepCopyOnScreen() {
        GizmoManager gizmos = GizmoManager.Instance;
        if (gizmos == null || gizmos.GetTargetObject() != _baseMapCopy || !gizmos.IsShowingGizmo()) return;
        if (gizmos.GetActiveGizmo() == null || !gizmos.GetActiveGizmo().Gizmo.IsDragged) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 center = _baseMapCopy.transform.TransformPoint(_copyCenterLocal);
        Vector3 viewport = cam.WorldToViewportPoint(center);
        if (viewport.z <= 0f) return;

        float min = _screenEdgeMargin, max = 1f - _screenEdgeMargin;
        Vector2 clamped = new Vector2(Mathf.Clamp(viewport.x, min, max), Mathf.Clamp(viewport.y, min, max));
        if (Mathf.Approximately(clamped.x, viewport.x) && Mathf.Approximately(clamped.y, viewport.y)) return;

        // Same height, at the clamped point of the screen
        Ray ray = cam.ViewportPointToRay(clamped);
        if (!new Plane(Vector3.up, center).Raycast(ray, out float enter)) return;
        _baseMapCopy.transform.position += ray.GetPoint(enter) - center;
    }

    #region Followers (pins, view points, map variants)

    void CaptureFollowers() {
        _followers.Clear();
        _followingCopy = false;
        if (_baseMapCopy == null) return;

        if (PinManager.Instance != null) {
            foreach (PinObject pin in PinManager.Instance.GetPins()) {
                if (pin != null) _followers.Add(CreateFollower(pin.transform, pin: pin));
            }
        }
        if (ViewManager.Instance != null) {
            foreach (ViewPoint view in ViewManager.Instance.GetViewPoints()) {
                if (view != null) _followers.Add(CreateFollower(view.transform)); // its vcam is a child
            }
        }
        foreach (MapVariant variant in MapManager.Instance.GetVariantsWithoutBase()) {
            if (variant != null) _followers.Add(CreateFollower(variant.transform, variant: variant));
        }

        SetFollowerReference();
        _followingCopy = true;
    }

    static FollowerPose CreateFollower(Transform target, PinObject pin = null, MapVariant variant = null) {
        return new FollowerPose {
            Target = target,
            Position = target.position,
            Rotation = target.rotation,
            Normal = pin != null ? pin.SurfaceNormal : Vector3.up,
            Pin = pin,
            Variant = variant,
        };
    }

    // The followers keep their offset from the base map's pose, applied to the copy's pose
    void SetFollowerReference() {
        MapVariant baseMap = MapManager.Instance.GetBaseMap();
        if (baseMap == null) return;
        _referencePosition = baseMap.transform.position;
        _referenceRotation = baseMap.transform.rotation;
    }

    void MoveFollowersWithCopy() {
        Quaternion delta = _baseMapCopy.transform.rotation * Quaternion.Inverse(_referenceRotation);
        Vector3 copyPosition = _baseMapCopy.transform.position;
        foreach (FollowerPose pose in _followers) {
            if (pose.Target == null) continue;
            Vector3 position = copyPosition + delta * (pose.Position - _referencePosition);
            if (pose.Pin != null) {
                pose.Target.position = position;
                pose.Pin.SurfaceNormal = delta * pose.Normal; // also turns the pin
            } else {
                pose.Target.SetPositionAndRotation(position, delta * pose.Rotation);
            }
        }
    }

    void RestoreFollowers() {
        foreach (FollowerPose pose in _followers) {
            if (pose.Target == null) continue;
            if (pose.Pin != null) {
                pose.Target.position = pose.Position;
                pose.Pin.SurfaceNormal = pose.Normal;
            } else {
                pose.Target.SetPositionAndRotation(pose.Position, pose.Rotation);
            }
        }
    }

    // Keeps the followers where the copy took them, variants also store their new transform.
    // The FPS spawn point isn't visible here, so it only moves now.
    void CommitFollowers() {
        if (!_followingCopy || _baseMapCopy == null) return;
        MoveFollowersWithCopy();

        Quaternion delta = _baseMapCopy.transform.rotation * Quaternion.Inverse(_referenceRotation);
        Vector3 copyPosition = _baseMapCopy.transform.position;

        FPSManager fps = FPSManager.Instance;
        if (fps != null && fps.SpawnPlaced) {
            fps.MoveSpawnWithMap(copyPosition + delta * (fps.SpawnPosition - _referencePosition));
        }

        // The free cam comes back to the view it had before geolocalization (kept on its vcam), which keeps showing the model.
        // The first placing frames the model instead (PlaceMapModel).
        GameObject freeCamVcam = CameraManager.Instance != null ? CameraManager.Instance.GetFreeCamVcam() : null;
        if (!_firstOpen && freeCamVcam != null) {
            Transform cam = freeCamVcam.transform;
            cam.SetPositionAndRotation(copyPosition + delta * (cam.position - _referencePosition), delta * cam.rotation);
        }

        foreach (FollowerPose pose in _followers) {
            if (pose.Variant != null) {
                MapManager.Instance.ApplyAndSaveMapTransform(pose.Variant, pose.Variant.transform.position, pose.Variant.transform.eulerAngles);
            }
        }
        _followingCopy = false;
        _followers.Clear();
    }

    #endregion

    public void ToggleLock() {
        _lastLockToggle = !_lastLockToggle;
        LockGeoMap(_lastLockToggle);
    }

    public void ToggleLock(bool toggleOn) {
        _lastLockToggle = toggleOn;
        LockGeoMap(_lastLockToggle);
        _UIInstance.ChangeLockVisual(toggleOn);
    }

    void LockGeoMap(bool lockToggle) {
        ToggleGeoMapZoom(!lockToggle);
        GeoMapManager.Instance.ToggleGeoMapControl(!lockToggle);
        // The model stays visible while the map moves under it (the opacity slider can hide it),
        // only its colliders are off so it can't be selected and clicks reach the map
        ToggleCopyColliders(lockToggle);
        GizmoManager.Instance.HideGizmo();
    }

    void ToggleCopyColliders(bool enabled) {
        if (_baseMapCopy == null) return;
        foreach (Collider col in _baseMapCopy.GetComponentsInChildren<Collider>(true)) {
            col.enabled = enabled;
        }
    }

    public void PlaceMapModel() {
        if (_baseMapCopy == null) return;
        CommitFollowers();
        ApplyTransformToBaseMap();
        // A new project: the free cam is still at the scene's default position, unrelated to the model
        if (_firstOpen) CameraManager.Instance.FrameFreeCamOn(MapManager.Instance.GetBaseMap().gameObject);
        if (MainManagerBase.Instance is EditorManager editorManager) editorManager.ToggleUnsavedChanges(true);
        SaveGeoMapData(); // exits once the elevation arrives
    }

    public GeoLocalizationData GetPlacementMapData() {
        return _geoData;
    }

    public void UpdateCloneOpacity(float opacityValue) {
        _opacity = opacityValue;
        if (_baseMapCopy == null) return;
        SetPreviewOpacity(opacityValue);
    }

    public void InitializeWithPlacementMapData(GeoLocalizationData geoData) {
        _geoData = geoData;
    }

    void ToggleGeoMapZoom(bool toggleOn) {
        GeoMapManager.Instance.OnlineMapsReff.GetComponent<OnlineMapsTileSetControl>().allowZoom = toggleOn;
    }

    void SaveGeoMapData() {
        double lng, lat;
        OnlineMapsTileSetControl.instance.GetCoordsByWorldPosition(_baseMapCopy.transform.position, out lng, out lat);

        ElevationFetcher.Instance.GetElevation(new Vector2((float)lng, (float)lat), elevation => {
            SetGeoData((float)lat, (float)lng, elevation);
            GeoMapManager.Instance.ExitGeoLocalization();
        }, error => {
            Debug.LogError(error);
            SetGeoData((float)lat, (float)lng, 0);
            GeoMapManager.Instance.ExitGeoLocalization();
        });
    }

    void SetGeoData(float latitude, float longitude, float elevation) {
        _geoData = new GeoLocalizationData {
            longtitude = longitude,
            latitude = latitude,
            elevation = elevation,
        };
        // The sun otherwise gets the location only when the project is loaded
        SunManager.Instance?.SetGeoCoordinates(latitude, longitude);
    }

    void ApplyTransformToBaseMap() {
        MapManager.Instance.SetBaseMapPositionAndRotation(_baseMapCopy.transform.position, _baseMapCopy.transform.rotation);
    }

    #region Preview materials

    /// <summary>
    /// Gives the copy see-through materials with the model's texture and color.
    /// A dedicated shader instead of switching the model's own materials to transparent: that needs shader variants
    /// (Standard + _ALPHABLEND_ON with the model's keywords) that builds don't contain, so the opacity only changed in the editor.
    /// Created once per copy, the opacity slider only changes a value on them.
    /// </summary>
    void ApplyPreviewMaterials() {
        DestroyPreviewMaterials();
        Shader shader = _previewMaterial != null ? _previewMaterial.shader : Shader.Find(PreviewShaderName);
        if (shader == null) return;

        // Renderers sharing a material share its preview material too
        Dictionary<Material, Material> previews = new Dictionary<Material, Material>();
        foreach (Renderer rend in _baseMapCopy.GetComponentsInChildren<Renderer>(true)) {
            Material[] materials = rend.sharedMaterials; // .materials would make (and leak) copies
            for (int i = 0; i < materials.Length; i++) {
                Material original = materials[i];
                if (original == null) continue;

                if (!previews.TryGetValue(original, out Material preview)) {
                    preview = _previewMaterial != null ? new Material(_previewMaterial) : new Material(shader);
                    if (original.HasProperty(MainTexId)) {
                        preview.SetTexture(MainTexId, original.GetTexture(MainTexId));
                        preview.SetTextureScale(MainTexId, original.GetTextureScale(MainTexId));
                        preview.SetTextureOffset(MainTexId, original.GetTextureOffset(MainTexId));
                    }
                    if (original.HasProperty(ColorId)) preview.SetColor(ColorId, original.GetColor(ColorId));
                    previews.Add(original, preview);
                    _previewMaterials.Add(preview);
                }
                materials[i] = preview;
            }
            rend.sharedMaterials = materials;
        }
    }

    void SetPreviewOpacity(float opacity) {
        foreach (Material preview in _previewMaterials) {
            if (preview != null) preview.SetFloat(OpacityId, opacity);
        }
    }

    void DestroyPreviewMaterials() {
        foreach (Material preview in _previewMaterials) {
            if (preview != null) Destroy(preview);
        }
        _previewMaterials.Clear();
    }

    void OnDestroy() {
        DestroyPreviewMaterials();
    }

    #endregion
}
