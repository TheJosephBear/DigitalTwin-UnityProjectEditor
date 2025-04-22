using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class GeoMapManager : Singleton<GeoMapManager> {

    public GameObject OnlineMapGameObject;
    public GameObject vcam;

    protected override void Awake() {
        base.Awake();
        OnlineMapGameObject.SetActive(false);
        vcam.SetActive(false);
    }

    private void Update() {
        if (OnlineMapGameObject.activeSelf) {
            OnlineMaps map = OnlineMapGameObject.GetComponent<OnlineMaps>();
            double lng = map.position.x;
            double lat = map.position.y;
            print($"Map position (center): Latitude = {lat}, Longitude = {lng}");
        }
    }

    public void ToggleMapOnGeoMap() {
        OnlineMapGameObject.SetActive(!OnlineMapGameObject.activeSelf);
        vcam.SetActive(!vcam.activeSelf);
        StartCoroutine(DisableVcams());
    }

    IEnumerator DisableVcams() {
        if (Camera.main.GetComponent<CinemachineBrain>().IsBlending) {
            yield return null;  
        }
        
    }

}
