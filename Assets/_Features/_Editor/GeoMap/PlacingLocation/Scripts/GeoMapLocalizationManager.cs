using System.Collections;
using System.Collections.Generic;
using RTG;
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
    RTGApp _rtgReff;
    GeoLocalizationData _geoData;


    protected override void Awake() {
        base.Awake();
        _rtgReff = FindAnyObjectByType<RTGApp>();
    }

    public void Setup() {
        _rtgReff.gameObject.SetActive(false);
        ToggleGeoMapZoom(true);
        GeoMapManager.Instance.ToggleGeoMapControl(true);
        // Create base map copy
        if (_baseMapCopy == null && MapManager.Instance.IsBaseMapUploaded()) {
            MapVariant baseMapReff = MapManager.Instance.GetBaseMap();
            if (!baseMapReff.IsVisible) {
                baseMapReff.ToggleMeshVisibility(true);
            }
            _baseMapCopy = SceneLoadingManager.Instance.InstantiateObjectInScene(MapManager.Instance.GetBaseMap().ModelAsset.ModelGameObject, MapCenterPosition, ActiveSceneType);
            Movable movableScript = _baseMapCopy.AddComponent<Movable>();
            movableScript.ShownAxis = GizmoAxis.All;
            movableScript.MovableType = MovableType.Universal;
            _baseMapCopy.AddComponent<BoxCollider>().size = new Vector3(10, 10, 10);
            _baseMapCopy.SetActive(false);
            baseMapReff.ToggleMeshVisibility(false);
        }
    }

    public void Exit() {
        _rtgReff.gameObject.SetActive(true);
        _baseMapCopy.SetActive(false);
        MapManager.Instance.GetBaseMap().ToggleMeshVisibility(true);
    }


    public void LockGeoMap() {
        ToggleGeoMapZoom(false);
        GeoMapManager.Instance.ToggleGeoMapControl(false);
        _baseMapCopy.SetActive(true);
        _rtgReff.gameObject.SetActive(true);
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