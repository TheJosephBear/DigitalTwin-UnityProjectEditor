using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class GeoMapManager : Singleton<GeoMapManager> {

    public OnlineMaps OnlineMapsReff;
    public GameObject vcam;

    protected override void Awake() {
        base.Awake();
        OnlineMapsReff.gameObject.SetActive(false);
        if(vcam!=null) vcam.SetActive(false);
    }

    private void Update() {
        if (OnlineMapsReff.gameObject.activeSelf) {
            OnlineMaps map = OnlineMapsReff;
            double lng = map.position.x;
            double lat = map.position.y;
      //      print($"Map position (center): Latitude = {lat}, Longitude = {lng}");
        }
    }

    public void ToggleMapOnGeoMap() {
        OnlineMapsReff.gameObject.SetActive(!OnlineMapsReff.gameObject.activeSelf);
        if (vcam != null) vcam.SetActive(!vcam.activeSelf);
        StartCoroutine(DisableVcams());
    }

    public void ToggleGeoMapControl() {
        OnlineMapsReff.GetComponent<OnlineMapsTileSetControl>().enabled = !OnlineMapsReff.GetComponent<OnlineMapsTileSetControl>().enabled;
        print(OnlineMapsReff.enabled);
    }

    IEnumerator DisableVcams() {
        if (Camera.main.GetComponent<CinemachineBrain>().IsBlending) {
            yield return null;  
        }
        
    }

}
