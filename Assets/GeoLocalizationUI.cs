using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GeoLocalizationUI : MonoBehaviour {

    public TextMeshProUGUI InfoTextReff;
    public TMP_InputField ZoomScaleInputFieldReff;

    public void PrintInfo(string text) {
        InfoTextReff.text = text;
    }

    public void ZoomToScale() {
        float zoomScale;
        if (float.TryParse(ZoomScaleInputFieldReff.text, out zoomScale)) {
            GeoMapManager.Instance.ZoomToFitScale(zoomScale, 1f);
        }
    }
}
