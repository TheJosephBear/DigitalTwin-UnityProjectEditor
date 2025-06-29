using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapVarAdjustUI : UIBehaviour {

    public TMP_Dropdown DropdownRefference;
    public TMP_InputField posXInput, posYInput, posZInput;
    public TMP_InputField rotXInput, rotYInput, rotZInput;

    private MapVariant _selectedVariant;

    public void FillDropdown(List<MapVariant> variants) {
        DropdownRefference.ClearOptions();

        List<string> names = new List<string>();
        foreach (var variant in variants) {
            names.Add(variant.Name);
        }

        DropdownRefference.AddOptions(names);
        DropdownRefference.onValueChanged.AddListener(OnDropdownSelected);

        // Auto-select first if exists
        if (variants.Count > 0) {
            SelectVariant(variants[0]);
            DropdownRefference.value = 0;
        }
    }

    public void OnDropdownSelected(int index) {
        List<MapVariant> variants = MapManager.Instance.GetVariants();
        if (index >= 0 && index < variants.Count) {
            SelectVariant(variants[index]);
        }
    }

    void SelectVariant(MapVariant variant) {
        _selectedVariant = variant;
        UpdateTransformValues();
    }

    public void UpdateTransformValues() {
        if (_selectedVariant == null) return;

        Vector3 pos = _selectedVariant.transform.position;
        Vector3 rot = _selectedVariant.transform.rotation.eulerAngles;

        posXInput.text = pos.x.ToString("F2");
        posYInput.text = pos.y.ToString("F2");
        posZInput.text = pos.z.ToString("F2");

        rotXInput.text = rot.x.ToString("F2");
        rotYInput.text = rot.y.ToString("F2");
        rotZInput.text = rot.z.ToString("F2");
    }

    public void OnTransformPositionChangedX(float newValue) {
        if (_selectedVariant != null) {
            Vector3 pos = _selectedVariant.transform.position;
            _selectedVariant.transform.position = new Vector3(newValue, pos.y, pos.z);
            UpdateTransformValues();
        }
    }

    public void OnTransformPositionChangedY(float newValue) {
        if (_selectedVariant != null) {
            Vector3 pos = _selectedVariant.transform.position;
            _selectedVariant.transform.position = new Vector3(pos.x, newValue, pos.z);
            UpdateTransformValues();
        }
    }

    public void OnTransformPositionChangedZ(float newValue) {
        if (_selectedVariant != null) {
            Vector3 pos = _selectedVariant.transform.position;
            _selectedVariant.transform.position = new Vector3(pos.x, pos.y, newValue);
            UpdateTransformValues();
        }
    }

    public void OnTransformRotationChangedX(float newValue) {
        if (_selectedVariant != null) {
            Vector3 rot = _selectedVariant.transform.rotation.eulerAngles;
            _selectedVariant.transform.rotation = Quaternion.Euler(newValue, rot.y, rot.z);
            UpdateTransformValues();
        }
    }

    public void OnTransformRotationChangedY(float newValue) {
        if (_selectedVariant != null) {
            Vector3 rot = _selectedVariant.transform.rotation.eulerAngles;
            _selectedVariant.transform.rotation = Quaternion.Euler(rot.x, newValue, rot.z);
            UpdateTransformValues();
        }
    }

    public void OnTransformRotationChangedZ(float newValue) {
        if (_selectedVariant != null) {
            Vector3 rot = _selectedVariant.transform.rotation.eulerAngles;
            _selectedVariant.transform.rotation = Quaternion.Euler(rot.x, rot.y, newValue);
            UpdateTransformValues();
        }
    }

}
