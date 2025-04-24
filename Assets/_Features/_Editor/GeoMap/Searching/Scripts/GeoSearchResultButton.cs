using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GeoSearchResultButton : MonoBehaviour {
    public TextMeshProUGUI buttonText;
    double latitude;
    double longitude;

    public void SetupButton(double latitude, double longitude, string buttonText) {
        this.latitude = latitude;
        this.longitude = longitude;
        this.buttonText.text = buttonText;
    }

    public void OnClick() {
        OnlineMaps.instance.SetPositionAndZoom(longitude, latitude, 16f);
    }
}
