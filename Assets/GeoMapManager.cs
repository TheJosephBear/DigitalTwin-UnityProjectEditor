using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class GeoMapManager : Singleton<GeoMapManager> {

    public OnlineMaps OnlineMapsReff;
    public GameObject vcam;
    int _equator = 40075000;

    protected override void Awake() {
        base.Awake();
        OnlineMapsReff.gameObject.SetActive(false);
        if(vcam!=null) vcam.SetActive(false);
    }

    private void Update() {
        if (OnlineMapsReff.gameObject.activeSelf) {
            GetCurrentMapScale();
        }
    }

    public void ToggleMapOnGeoMap() {
        OnlineMapsReff.gameObject.SetActive(!OnlineMapsReff.gameObject.activeSelf);
        if (vcam != null) vcam.SetActive(!vcam.activeSelf);
        StartCoroutine(DisableVcams());
    }

    public void ToggleGeoMapControl() {
        OnlineMapsReff.GetComponent<OnlineMapsTileSetControl>().enabled = !OnlineMapsReff.GetComponent<OnlineMapsTileSetControl>().enabled;
    }

    // Returns maps current map to ground scale as an int (1 : returned value)
    public float GetCurrentMapScale() {
        float size;
        if (OnlineMapsReff.zoom < 5) size = (_equator / (1 << OnlineMapsReff.zoom) * OnlineMapsReff.zoomFactor * OnlineMapsReff.width / OnlineMapsUtils.tileSize);
        else size = (OnlineMapsUtils.DistanceBetweenPoints(OnlineMapsReff.topLeftPosition, OnlineMapsReff.bottomRightPosition).x * 1000);
        print("Width of the map in irl meters: " + size);
        return size / OnlineMapsReff.width;
    }

    // Zoom the Geo map to fit the given scale (1 : wanted scale)
    public void ZoomToFitScale(float requiredScale, float threshold = 0.1f) {
        float step = 0.01f;
        OnlineMapsReff.floatZoom = 3f;

        // Currently bruteforcing because i have no idea how else to do it
        while (Mathf.Abs(GetCurrentMapScale() - requiredScale) >= threshold) {
            OnlineMapsReff.floatZoom += step;
            if (OnlineMapsReff.floatZoom > 20) return;
        }
        print("Finished scale zooming, it should be 1: " + GetCurrentMapScale());
    }

    IEnumerator DisableVcams() {
        if (Camera.main.GetComponent<CinemachineBrain>().IsBlending) {
            yield return null;  
        }
        
    }

}
