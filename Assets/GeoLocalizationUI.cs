using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GeoLocalizationUI : MonoBehaviour {

    public TextMeshProUGUI InfoTextReff;

    public void PrintInfo(string text) {
        InfoTextReff.text = text;
    }
}
