using System;
using System.Collections;
using System.Collections.Generic;
using RTG;
using Unity.VisualScripting;
using UnityEngine;
public class GeoMapLocalizationManager : Singleton<GeoMapLocalizationManager> {
    /// <summary>
    /// Handles the process of localization
    /// Keeps result data and settings
    /// </summary>

    public GameObject GeoLocalizationUIPrefab;
    public Vector3 MapCenterPosition;
    public float ModelTransparency = 0.8f;

    GameObject _baseMapCopy;
    GeoLocalizationData _geoData;
    GeoLocalizationUI _UIInstance;
    bool _lastLockToggle = false;


    protected override void Awake() {
        base.Awake();
    }

    public void Setup(bool firstOpen = true) {
        ToggleUI(true);
        _UIInstance.ToggleExitToMenuButtonVisibility(firstOpen);
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
        }
    }

    public void UploadMap(ModelAsset uploadedModel) {
        if (_baseMapCopy != null) {
            Destroy(_baseMapCopy.gameObject);
        }

        MapManager.Instance.SetBaseMapModel(uploadedModel);
        CreateBaseMapCopy();
        ToggleLock(true);
    }

    void ToggleUI(bool toggleOn) {
        if(_UIInstance == null) {
            _UIInstance = SceneLoadingManager.Instance.InstantiateObjectInScene(GeoLocalizationUIPrefab).GetComponent<GeoLocalizationUI>();
        }
        _UIInstance.gameObject.SetActive(toggleOn);
    }

    void CreateBaseMapCopy() {
        if (_baseMapCopy == null && MapManager.Instance.IsBaseMapUploaded()) {
            MapVariant baseMapReff = MapManager.Instance.GetBaseMap();
            if (!baseMapReff.IsVisible) {
                baseMapReff.ToggleMeshVisibility(true);
            }

            _baseMapCopy = SceneLoadingManager.Instance.InstantiateObjectInScene(
                MapManager.Instance.GetBaseMap().ModelAsset.ModelGameObject, 
                MapCenterPosition
            );

            // Add mesh collider
            foreach (Transform child in _baseMapCopy.GetComponentsInChildren<Transform>()) {
                if (child.GetComponent<MeshRenderer>() != null || child.GetComponent<MeshFilter>() != null) {
                    child.AddComponent<MeshCollider>();
                    // Also make it transparent
                    MakeMaterialsTransparent(child.gameObject, ModelTransparency);
                }
            }

            // Add movable component for clickability
            BaseMapCopyMovable movableScript = _baseMapCopy.AddComponent<BaseMapCopyMovable>();

            // Setup gizmo axes and restrictions
            List<GizmoAxis> axes = new List<GizmoAxis> { GizmoAxis.X, GizmoAxis.Y, GizmoAxis.Z };
            movableScript.ShownAxis = axes;
            movableScript.MovableType = GizmoType.Universal;

            // Disable the map at the beginning
            _baseMapCopy.SetActive(false);
            baseMapReff.ToggleMeshVisibility(false);
        }
    }

    public void ZoomMap(float value) {
        GeoMapManager.Instance.ZoomMap(value);
    }

    public void Exit() {
        GizmoManager.Instance.HideGizmo();
        _baseMapCopy.SetActive(false);
        MapManager.Instance.GetBaseMap().ToggleMeshVisibility(true);
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
            _geoData = new GeoLocalizationData { 
                longtitude = (float)lng,
                latitude = (float)lat,
                elevation = elevation,
            };
            GeoMapManager.Instance.ExitGeoLocalization();
        }, error => {
            Debug.LogError(error);
        });
    }

    void ApplyTransformToBaseMap() {
        MapManager.Instance.SetBaseMapPositionAndRotation(_baseMapCopy.transform.position, _baseMapCopy.transform.rotation);
    }

    void MakeMaterialsTransparent(GameObject targetObject, float transparency) {
        Renderer rend = targetObject.GetComponent<Renderer>();
        Material[] materials = rend.materials;
        foreach(Material mat in materials) {
            Color color = mat.color;
            color.a = transparency;
            mat.color = color;
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
        }
    }
}

[Serializable]
public class GeoLocalizationData {
    public float longtitude, latitude, elevation;
}