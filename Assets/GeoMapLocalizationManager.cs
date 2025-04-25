using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeoMapLocalizationManager : Singleton<GeoMapLocalizationManager> {
    /// <summary>
    /// Handles the process of placing a map on some location and locking it there.
    /// Sends result data and setting to the GeoMapManager
    /// </summary>

    public GameObject BaseMap;

    public void EnterLockingPhase() {
        BaseMap.SetActive(false);
        GeoMapManager.Instance.OnlineMapsReff.GetComponent<OnlineMapsTileSetControl>().allowZoom = true;
    }

    public void EnterPlacingPhase() {
        LockGeoMapZoom();
        BaseMap.SetActive(true);
    }

    public void SaveLocalizationSettings() {
        SaveGeoMapPosition();
    }

    void LockGeoMapZoom() {
        GeoMapManager.Instance.OnlineMapsReff.GetComponent<OnlineMapsTileSetControl>().allowZoom = false;
    }

    void SaveGeoMapPosition() {
        // lat, long, elev
        print(GeoMapManager.Instance.OnlineMapsReff.position.x+" "+GeoMapManager.Instance.OnlineMapsReff.position.y);
        ElevationFetcher.Instance.GetElevation(GeoMapManager.Instance.OnlineMapsReff.position, elevation => {
            Debug.Log($"Elevation: {elevation} meters");
        },
        error => {
            Debug.LogError(error);
        });
    }



}
