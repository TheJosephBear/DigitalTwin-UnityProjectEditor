using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GeoSearchResultButton : MonoBehaviour {

    public TextMeshProUGUI buttonText;
    double latitude;
    double longitude;
    GeoSearch _searchScript;

    public void SetupButton(double latitude, double longitude, string buttonText, GeoSearch searchScript) {
        this.latitude = latitude;
        this.longitude = longitude;
        this.buttonText.text = buttonText;
        _searchScript = searchScript;
    }

    public void OnClick() {
        _searchScript.OnResultClick(longitude, latitude);
    }
}
