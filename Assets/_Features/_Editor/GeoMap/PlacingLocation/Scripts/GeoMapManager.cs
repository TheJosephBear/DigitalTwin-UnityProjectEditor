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


    public void ActivateGeoLocalization() {
        UImanager.Instance.ShowUI(UIType.GeoLocalizationUI);
        ToggleGeoMap(true);
        GeoMapLocalizationManager.Instance.Setup();
    }

    public void ExitGeoLocalization() {
        UImanager.Instance.HideUI(UIType.GeoLocalizationUI);
        ToggleGeoMap(false);
        GeoMapLocalizationManager.Instance.Exit();
        EditorManager.Instance.ChangeEditorMode(EditorMode.Freecam);
    }



    public void ToggleGeoMap() {
        OnlineMapsReff.gameObject.SetActive(!OnlineMapsReff.gameObject.activeSelf);
        if (vcam != null) vcam.SetActive(!vcam.activeSelf);
    }

    public void ToggleGeoMap(bool toggleOn) {
        OnlineMapsReff.gameObject.SetActive(toggleOn);
        if (vcam != null) vcam.SetActive(toggleOn);
    }

    public void ToggleGeoMapControl() {
        OnlineMapsReff.GetComponent<OnlineMapsTileSetControl>().enabled = !OnlineMapsReff.GetComponent<OnlineMapsTileSetControl>().enabled;
    }

    // Returns maps current map to ground scale as an int (1 : returned value)
    public float GetCurrentMapScale() {
        float size;
        if (OnlineMapsReff.zoom < 5) size = (_equator / (1 << OnlineMapsReff.zoom) * OnlineMapsReff.zoomFactor * OnlineMapsReff.width / OnlineMapsUtils.tileSize);
        else size = (OnlineMapsUtils.DistanceBetweenPoints(OnlineMapsReff.topLeftPosition, OnlineMapsReff.bottomRightPosition).x * 1000);
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
    }

}
