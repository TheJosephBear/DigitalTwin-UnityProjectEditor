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

    public Vector3 MapCenterPosition;
    public SceneType ActiveSceneType = SceneType.Editing;
    public float MapTransparency = 0.8f;

    GameObject _baseMapCopy;
    GizmoManager gizmoManager;
    GeoLocalizationData _geoData;
    bool _lastLockToggle = false;


    protected override void Awake() {
        base.Awake();
        gizmoManager = GizmoManager.Instance;
    }

    public void Setup() {
        gizmoManager.HideGizmo();
        ToggleGeoMapZoom(true);
        EditorManager.Instance.GeoMapManager.ToggleGeoMapControl(true);
        // Create base map copy
        if (_baseMapCopy == null && EditorManager.Instance.MapManager.IsBaseMapUploaded()) {
            MapVariant baseMapReff = EditorManager.Instance.MapManager.GetBaseMap();
            if (!baseMapReff.IsVisible) {
                baseMapReff.ToggleMeshVisibility(true);
            }
            _baseMapCopy = SceneLoadingManager.Instance.InstantiateObjectInScene(EditorManager.Instance.MapManager.GetBaseMap().ModelAsset.ModelGameObject, MapCenterPosition, ActiveSceneType);

            // Add mesh collider
            foreach (Transform child in _baseMapCopy.GetComponentsInChildren<Transform>()) {
                if (child.GetComponent<MeshRenderer>() != null || child.GetComponent<MeshFilter>() != null) {
                    child.AddComponent<MeshCollider>();
                    // Also make it transparent
                    MakeMaterialsTransparent(child.gameObject, MapTransparency);
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
        EditorManager.Instance.GeoMapManager.ZoomMap(value);
    }

    public void Exit() {
        gizmoManager.HideGizmo();
        _baseMapCopy.SetActive(false);
        EditorManager.Instance.MapManager.GetBaseMap().ToggleMeshVisibility(true);
    }

    public void ToggleLock() {
        _lastLockToggle = !_lastLockToggle; 
        LockGeoMap(_lastLockToggle);
    }

    void LockGeoMap(bool lockToggle) {
        ToggleGeoMapZoom(!lockToggle);
        EditorManager.Instance.GeoMapManager.ToggleGeoMapControl(!lockToggle);
        _baseMapCopy.SetActive(lockToggle);
        gizmoManager.HideGizmo();
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
        EditorManager.Instance.GeoMapManager.OnlineMapsReff.GetComponent<OnlineMapsTileSetControl>().allowZoom = toggleOn;
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
            EditorManager.Instance.GeoMapManager.ExitGeoLocalization();
        }, error => {
            Debug.LogError(error);
        });
    }

    void ApplyTransformToBaseMap() {
        EditorManager.Instance.MapManager.SetBaseMapPositionAndRotation(_baseMapCopy.transform.position, _baseMapCopy.transform.rotation);
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