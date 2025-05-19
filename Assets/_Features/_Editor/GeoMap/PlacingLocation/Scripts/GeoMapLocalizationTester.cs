using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeoMapLocalizationTester : MonoBehaviour {
    
    void Start() {
        GeoMapManager.Instance.ToggleMapOnGeoMap();
        GeoMapLocalizationManager.Instance.Setup();
    }

}
