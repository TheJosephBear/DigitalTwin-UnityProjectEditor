using System.Collections;
using System.Collections.Generic;
using RTG;
using UnityEngine;
using static OnlineMapsGoogleDirectionsResult;

public class GeoMapLocalizationManager : Singleton<GeoMapLocalizationManager> {
    /// <summary>
    /// Handles the process of placing a map on some location and locking it there.
    /// Sends result data and setting to the GeoMapManager
    /// </summary>

    public GameObject BaseMap;
    public GeoLocalizationUI GeoLocalizationUIreff;
    RTGApp _rtgReff;

    protected override void Awake() {
        base.Awake();
        _rtgReff = FindAnyObjectByType<RTGApp>();
    }

    public void Setup() {
        _rtgReff.gameObject.SetActive(false);
        EnterLockingPhase();
    }

    public void Exit() {
        _rtgReff.gameObject.SetActive(true);
    }

    public void EnterLockingPhase() {
        BaseMap.SetActive(false);
   //     GeoMapManager.Instance.OnlineMapsReff.GetComponent<OnlineMapsTileSetControl>().allowZoom = true;
    }

    public void EnterPlacingPhase() {
        _rtgReff.gameObject.SetActive(true);
        //     LockGeoMapZoom();
        GeoMapManager.Instance.ToggleGeoMapControl();
        BaseMap.SetActive(true);
    }

    public void SaveLocalizationSettings() {
        SaveGeoMapPosition();
    }

    void LockGeoMapZoom() {
        GeoMapManager.Instance.OnlineMapsReff.GetComponent<OnlineMapsTileSetControl>().allowZoom = !GeoMapManager.Instance.OnlineMapsReff.GetComponent<OnlineMapsTileSetControl>().allowZoom;
    }

    void SaveGeoMapPosition() {
        double lng, lat;
        OnlineMapsTileSetControl.instance.GetCoordsByWorldPosition(BaseMap.transform.position, out lng, out lat);

        ElevationFetcher.Instance.GetElevation(new Vector2((float)lng, (float)lat), elevation => {
            GeoLocalizationUIreff.PrintInfo($"Lokalita umístìna na {lng}, {lat} \n" +
            $"Nadmoøská výška lokality je {elevation} metrù.");
        }, error => {
            Debug.LogError(error);
        });
    }



}
