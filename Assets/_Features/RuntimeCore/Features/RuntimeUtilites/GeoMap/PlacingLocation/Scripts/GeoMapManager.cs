using System;
using Cinemachine;
using UnityEngine;

public class GeoMapManager : Singleton<GeoMapManager> {

    public OnlineMaps OnlineMapsReff;
    public GameObject vcam;
    public float MinVcamHeight = 200f; // the closest the vcam zooms in
    GeoMapLocalizationManager _localizationManager;
    int _equator = 40075000;
    float _previousZoomValue;
    float _maxVcamHeight; // the vcam's starting height is the furthest it zooms out

    protected override void Awake() {
        base.Awake();
        _localizationManager = GeoMapLocalizationManager.Instance;

        OnlineMapsReff.gameObject.SetActive(false);
        if(vcam!=null) {
            _maxVcamHeight = vcam.transform.position.y;
            vcam.SetActive(false);
        }
    }

    /// <summary>
    /// Moves the vcam (looking straight down) on the Y axis only, so the bounds fit in its view with some padding.
    /// It stays between MinVcamHeight and its starting height.
    /// </summary>
    public void FocusVcamOnBounds(Bounds bounds, float padding = 1.2f) {
        if (vcam == null) return;

        float tanHalfVertical = Mathf.Tan(vcam.GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView * 0.5f * Mathf.Deg2Rad);
        float tanHalfHorizontal = tanHalfVertical * Camera.main.aspect;

        // The vcam doesn't move on X/Z, so the bounds' offset from it counts too.
        // Looking down, the screen's horizontal axis is world X and its vertical axis is world Z.
        Vector3 camPos = vcam.transform.position;
        float halfX = (Mathf.Abs(bounds.center.x - camPos.x) + bounds.extents.x) * padding;
        float halfZ = (Mathf.Abs(bounds.center.z - camPos.z) + bounds.extents.z) * padding;
        float distance = Mathf.Max(halfX / tanHalfHorizontal, halfZ / tanHalfVertical);

        camPos.y = Mathf.Clamp(bounds.max.y + distance, MinVcamHeight, _maxVcamHeight);
        vcam.transform.position = camPos;
    }

    public void ResetVcamHeight() {
        if (vcam == null) return;
        Vector3 camPos = vcam.transform.position;
        camPos.y = _maxVcamHeight;
        vcam.transform.position = camPos;
    }

    public void ActivateGeoLocalization(bool firstOpen = true) {
    //    UIManager.Instance.ShowUI(UIType.GeoLocalizationUI);
        ToggleGeoMap(true);
        GeoMapLocalizationManager.Instance.Setup(firstOpen);
    }

    public void ExitGeoLocalization() {
    //    UIManager.Instance.HideUI(UIType.GeoLocalizationUI);
        ToggleGeoMap(false);
        GeoMapLocalizationManager.Instance.Exit();
        EditorManager.Instance.ChangeState(AppState.Freecam);
    }

    public void LeaveToMenu(Action<bool> onComplete = null) {
        if(MainManagerBase.Instance is EditorManager editorMan) {
            editorMan.ExitEditor((exitSuccess) => {
                onComplete.Invoke(true);
            });
        }

        onComplete.Invoke(false);
        // TO IMPLEMENT PROPERLY

        //UIManager.Instance.HideUI(UIType.GeoLocalizationUI);
        //ToggleGeoMap(false);
        //GeoMapLocalizationManager.Instance.Exit();
        //if (MainManagerBase.Instance is EditorManager editorMgr) {
        //    editorMgr.ExitEditor();
        //}
    }

    public void ZoomMap(float zoomValue) {
        float delta = zoomValue - _previousZoomValue;
        OnlineMapsReff.floatZoom += delta;
        _previousZoomValue = zoomValue;
    }

    public void ResetZoomBaseline(float defaultValue = 0.5f) {
        _previousZoomValue = defaultValue;
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

    public void ToggleGeoMapControl(bool toggleOn) {
        OnlineMapsReff.GetComponent<OnlineMapsTileSetControl>().enabled = toggleOn;
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

    // Returns Vector2.zero if the project hasn't been geolocated yet (e.g. new project)
    public Vector2 GetCoordinates() {
        GeoLocalizationData data = GeoMapLocalizationManager.Instance?.GetPlacementMapData();
        if (data == null) return Vector2.zero;
        return new Vector2(data.latitude, data.longtitude);
    }

    public SerializableGeoMapManager SerializeManager() {
        return new SerializableGeoMapManager {
            geoData = GeoMapLocalizationManager.Instance.GetPlacementMapData()
        };
    }

    public void DeserializeManager(SerializableGeoMapManager serializedData) {
        // Null geoData clears data left over from a previously opened project
        GeoMapLocalizationManager.Instance?.InitializeWithPlacementMapData(serializedData?.geoData);
    }
}
