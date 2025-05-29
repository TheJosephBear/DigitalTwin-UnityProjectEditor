using System.Collections;
using System.Collections.Generic;
using RTG;
using Unity.VisualScripting;
using UnityEngine;
using static OnlineMapsGoogleDirectionsResult;

public class GeoMapLocalizationManager : Singleton<GeoMapLocalizationManager> {
    /// <summary>
    /// Handles the process of localization
    /// Keeps result data and settings
    /// </summary>

    public Vector3 MapCenterPosition;
    public SceneType ActiveSceneType = SceneType.Editing;

    GameObject _baseMapCopy;
    GizmoManager gizmoManager;
    GeoLocalizationData _geoData;


    protected override void Awake() {
        base.Awake();
        gizmoManager = GizmoManager.Instance;
    }

    public void Setup() {
        gizmoManager.HideGizmo();
        ToggleGeoMapZoom(true);
        GeoMapManager.Instance.ToggleGeoMapControl(true);
        // Create base map copy
        if (_baseMapCopy == null && MapManager.Instance.IsBaseMapUploaded()) {
            MapVariant baseMapReff = MapManager.Instance.GetBaseMap();
            if (!baseMapReff.IsVisible) {
                baseMapReff.ToggleMeshVisibility(true);
            }
            _baseMapCopy = SceneLoadingManager.Instance.InstantiateObjectInScene(MapManager.Instance.GetBaseMap().ModelAsset.ModelGameObject, MapCenterPosition, ActiveSceneType);

            // Add mesh collider
            foreach (Transform child in _baseMapCopy.GetComponentsInChildren<Transform>()) {
                if (child.GetComponent<MeshRenderer>() != null || child.GetComponent<MeshFilter>() != null) {
                    child.AddComponent<MeshCollider>();
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

    public void Exit() {
        gizmoManager.HideGizmo();
        _baseMapCopy.SetActive(false);
        MapManager.Instance.GetBaseMap().ToggleMeshVisibility(true);
    }


    public void LockGeoMap() {
        ToggleGeoMapZoom(false);
        GeoMapManager.Instance.ToggleGeoMapControl(false);
        _baseMapCopy.SetActive(true);
        gizmoManager.HideGizmo();

    }

    public void PlaceMapModel() {
        SaveGeoMapData();
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
        }, error => {
            Debug.LogError(error);
        });
    }
}

public class GeoLocalizationData {
    public float longtitude, latitude, elevation;
}