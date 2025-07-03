using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapVarAdjustUI : UIBehaviour {
    public TMP_Dropdown DropdownRefference;

    public TMP_InputField posXInput, posYInput, posZInput;
    public TMP_InputField rotXInput, rotYInput, rotZInput;

    private List<MapVariant> _variantList;

    public void FillDropdown(List<MapVariant> variants) {
        _variantList = variants;

        DropdownRefference.ClearOptions();
        List<string> names = new List<string>();

        foreach (var variant in variants)
            names.Add(variant.Name);

        DropdownRefference.AddOptions(names);
        DropdownRefference.onValueChanged.AddListener(OnDropdownSelected);

        if (variants.Count > 0) {
            DropdownRefference.value = 0;
            SelectVariant(variants[0]);
        }
    }

    private void Update() {
        UpdateTransformValues();
    }

    public void OnDropdownSelected(int index) {
        if (index >= 0 && index < _variantList.Count)
            SelectVariant(_variantList[index]);
    }

    private void SelectVariant(MapVariant variant) {
        MapVariantAdjustManager.Instance.SelectVariant(variant);
    }
    public void UpdateTransformValues() {
        var manager = MapVariantAdjustManager.Instance;

        Vector3 pos = manager.GetPosition();
        Vector3 rot = manager.GetRotationEuler();

        if (!posXInput.isFocused) posXInput.text = pos.x.ToString("F2");
        if (!posYInput.isFocused) posYInput.text = pos.y.ToString("F2");
        if (!posZInput.isFocused) posZInput.text = pos.z.ToString("F2");

        if (!rotXInput.isFocused) rotXInput.text = rot.x.ToString("F2");
        if (!rotYInput.isFocused) rotYInput.text = rot.y.ToString("F2");
        if (!rotZInput.isFocused) rotZInput.text = rot.z.ToString("F2");
    }


    public void ClearTexts() {
        /*
        posXInput.text = "";
        posYInput.text = "";
        posZInput.text = "";

        rotXInput.text = "";
        rotYInput.text = "";
        rotZInput.text = "";
        */
    }

    #region Position and Rotation Inputs

    private void ApplySafePosition() {
        Vector3 current = MapVariantAdjustManager.Instance.GetPosition();
        float x = TryParseOr(current.x, posXInput.text);
        float y = TryParseOr(current.y, posYInput.text);
        float z = TryParseOr(current.z, posZInput.text);

        MapVariantAdjustManager.Instance.UpdatePosition(new Vector3(x, y, z));
    }

    public void OnTransformPositionChangedX(string _) => ApplySafePosition();
    public void OnTransformPositionChangedY(string _) => ApplySafePosition();
    public void OnTransformPositionChangedZ(string _) => ApplySafePosition();

    private float TryParseOr(float fallback, string input) {
        return float.TryParse(input, out float val) ? val : fallback;
    }

    private void ApplySafeRotation() {
        Vector3 current = MapVariantAdjustManager.Instance.GetRotationEuler();
        float x = TryParseOr(current.x, rotXInput.text);
        float y = TryParseOr(current.y, rotYInput.text);
        float z = TryParseOr(current.z, rotZInput.text);

        MapVariantAdjustManager.Instance.UpdateRotation(new Vector3(x, y, z));
    }

    public void OnTransformRotationChangedX(string _) => ApplySafeRotation();
    public void OnTransformRotationChangedY(string _) => ApplySafeRotation();
    public void OnTransformRotationChangedZ(string _) => ApplySafeRotation();


    #endregion


}
