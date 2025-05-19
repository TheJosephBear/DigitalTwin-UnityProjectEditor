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
        // Create base map copy
        if (_baseMapCopy == null) {
            MapVariant baseMapReff = MapManager.Instance.GetBaseMap();
            if (!baseMapReff.IsVisible) {
                baseMapReff.ToggleMeshVisibility(true);
            }
            _baseMapCopy = SceneLoadingManager.Instance.InstantiateObjectInScene(MapManager.Instance.GetBaseMap().ModelAsset.ModelGameObject, MapCenterPosition, ActiveSceneType);
            _baseMapCopy.SetActive(false);
            baseMapReff.ToggleMeshVisibility(false);
        } else {
        
        }
    }

    public void Exit() {
        _rtgReff.gameObject.SetActive(true);
        _baseMapCopy.SetActive(false);
        MapManager.Instance.GetBaseMap().ToggleMeshVisibility(true);
    }


    public void EnterLockingPhase() {
    //    _baseMapCopy?.SetActive(true);
   //     GeoMapManager.Instance.OnlineMapsReff.GetComponent<OnlineMapsTileSetControl>().allowZoom = true;
    }

    public void EnterPlacingPhase() {
        _rtgReff.gameObject.SetActive(true);
        //     LockGeoMapZoom();
        GeoMapManager.Instance.ToggleGeoMapControl();
        _baseMapCopy.SetActive(true);
        print("Should be on");
    }

    public void SaveLocalizationSettings() {
        SaveGeoMapPosition();
    }

    void LockGeoMapZoom() {
        GeoMapManager.Instance.OnlineMapsReff.GetComponent<OnlineMapsTileSetControl>().allowZoom = !GeoMapManager.Instance.OnlineMapsReff.GetComponent<OnlineMapsTileSetControl>().allowZoom;
    }

    void SaveGeoMapPosition() {
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