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

    GameObject _baseMapCopy;
    readonly List<Material> _previewMaterials = new List<Material>(); // created for the copy, destroyed with it
    GeoLocalizationData _geoData;
    GeoLocalizationUI _UIInstance;
    bool _lastLockToggle = false;


    protected override void Awake() {
        base.Awake();
    }

    public void Setup(bool firstOpen = true) {
        ToggleUI(true);
        _UIInstance.Initialize(firstOpen);
        GizmoManager.Instance.HideGizmo();
        ToggleGeoMapZoom(true);
        GeoMapManager.Instance.ToggleGeoMapControl(true);

        if (MapManager.Instance.IsBaseMapUploaded() && _baseMapCopy != null) {
            _baseMapCopy.SetActive(true);
        } else if (MapManager.Instance.IsBaseMapUploaded()) {
            CreateBaseMapCopy();
        }

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
            GameObject geoMapGameObject = GeoMapManager.Instance.OnlineMapsReff.gameObject;

            _baseMapCopy = SceneLoadingManager.Instance.InstantiateObjectInScene(
                MapManager.Instance.GetBaseMap().ModelAsset.ModelGameObject,
                spawnPosition
            );

            _baseMapCopy.transform.rotation = baseMapReff.transform.rotation;

            // Lift (or lower) the copy so its lowest geometry point is 1 unit above the geo map
            if (FileLoadingManager.TryCalculateBounds(_baseMapCopy, out Bounds copyBounds)) {
                float targetLowestY = geoMapGameObject.transform.position.y + 1f;
                _baseMapCopy.transform.position += Vector3.up * (targetLowestY - copyBounds.min.y);
            }

            // Add mesh collider
            foreach (Transform child in _baseMapCopy.GetComponentsInChildren<Transform>()) {
                if (child.GetComponent<MeshRenderer>() != null || child.GetComponent<MeshFilter>() != null) {
                    child.gameObject.AddComponent<MeshCollider>();
                }
            }

            // Also make it transparent
            ApplyPreviewMaterials();
            SetPreviewOpacity(ModelTransparency);

            // Add movable component for clickability
            BaseMapCopyMovable movableScript = _baseMapCopy.AddComponent<BaseMapCopyMovable>();

            // Setup gizmo axes and restrictions
            List<GizmoAxis> axes = new List<GizmoAxis> { GizmoAxis.X, GizmoAxis.Y, GizmoAxis.Z };
            movableScript.ShownAxis = axes;
            movableScript.MovableType = GizmoType.Universal;

            // Disable the map at the beginning
            _baseMapCopy.SetActive(false);
            baseMapReff.ToggleMeshVisibility(false);
            baseMapReff.gameObject.SetActive(false);
        }
    }

    public void ZoomMap(float value) {
        GeoMapManager.Instance.ZoomMap(value);
    }

    public void Exit() {
        GizmoManager.Instance.HideGizmo();
        _baseMapCopy.SetActive(false);
        MapManager.Instance.GetBaseMap().ToggleMeshVisibility(true);
        MapManager.Instance.GetBaseMap().gameObject.SetActive(true);
        ToggleUI(false);
    }

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
        _baseMapCopy?.SetActive(lockToggle);
        GizmoManager.Instance.HideGizmo();
    }

    public void PlaceMapModel() {
        SaveGeoMapData();
        ApplyTransformToBaseMap();
    }

    public GeoLocalizationData GetPlacementMapData() {
        return _geoData;
    }

    public void UpdateCloneOpacity(float opacityValue) {
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
