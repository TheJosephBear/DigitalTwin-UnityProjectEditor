using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RadioButton : MonoBehaviour {
    [Serializable]
    public class RadioButtonSelectedEvent : UnityEvent<int> { }

    List<Toggle> _toggles = new List<Toggle>();

    [Tooltip("Called when a new toggle is selected. Int = index, String = label text.")]
    public RadioButtonSelectedEvent OnSelected;

    private bool isUpdating = false;

    void Awake() {
        GetToggles();
        InitializeToggles();
    }

    void OnValidate() {
        // Ensure toggles are hooked even in edit mode
        InitializeToggles();
    }

    void GetToggles() {
        _toggles.AddRange(GetComponentsInChildren<Toggle>(true));
        foreach (Toggle toggle in _toggles) {
            toggle.isOn = false;
        }
        _toggles[0].isOn = true;
    }

    void InitializeToggles() {
        // Clean up old listeners
        foreach (var toggle in _toggles) {
            if (toggle != null)
                toggle.onValueChanged.RemoveAllListeners();
        }

        // Assign new listeners
        for (int i = 0; i < _toggles.Count; i++) {
            int index = i;
            Toggle t = _toggles[i];
            if (t == null) continue;

            t.group = null; // handled manually
            t.onValueChanged.AddListener((isOn) => OnToggleChanged(index, isOn));
        }
    }

    void OnToggleChanged(int changedIndex, bool isOn) {
        if (isUpdating || !isOn) return;

        isUpdating = true;

        // Turn off all others
        for (int i = 0; i < _toggles.Count; i++) {
            if (i != changedIndex && _toggles[i] != null)
                _toggles[i].isOn = false;
        }

        // Get label text if available
        string labelText = "";
        var label = _toggles[changedIndex].GetComponentInChildren<Text>();
        if (label != null)
            labelText = label.text;

        // Invoke UnityEvent callback
        OnSelected?.Invoke(changedIndex);

        isUpdating = false;
    }

    public void Select(int index) {
        if (index >= 0 && index < _toggles.Count && _toggles[index] != null) {
            _toggles[index].isOn = true;
        }
    }

    public int GetSelectedIndex() {
        for (int i = 0; i < _toggles.Count; i++) {
            if (_toggles[i] != null && _toggles[i].isOn)
                return i;
        }
        return -1;
    }

}
