using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TwoMapsUI : UIBehaviour {

    public TMP_Dropdown dropDown;



    public void UpdateDropDown() {
        List<ModelAsset> mapVariants = MapManager.Instance.GetVariants();
        dropDown.ClearOptions();
        List<string> options = new List<string>();
        foreach (var variant in mapVariants) {
            options.Add(variant.ModelID);
        }

        dropDown.AddOptions(options);

        dropDown.onValueChanged.AddListener(OnMapVariantSelected);
    }

    // Called when a new map variant is selected from the dropdown
    private void OnMapVariantSelected(int index) {
        MapManager.Instance.SpawnSelectedVariant(index);
    }
}
