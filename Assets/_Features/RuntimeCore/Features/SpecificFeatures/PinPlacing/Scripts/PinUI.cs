using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PinUI: MonoBehaviour {

    public GameObject PinButtonPrefab;
    public GameObject ScrollViewContentRefference;
    public GameObject AddPinButtonRefference;

    [Header("Label mode")]
    [Tooltip("Hidden in the viewer")]
    public GameObject LabelModeSelectorRefference;
    [Tooltip("One per PinLabelMode, in its order")]
    public Image[] LabelModeButtonImages;
    public TMPro.TextMeshProUGUI[] LabelModeButtonTexts;
    public Color LabelModeSelectedColor = new Color(0.415686f, 0.345098f, 0.654902f, 1f);
    public Color LabelModeUnselectedColor = Color.white;

    List<PinHUDButton> _buttonList = new List<PinHUDButton>();
    PinManager _pinManager;

    void Start() {
        if (MainManagerBase.Instance is ViewingManager) {
            AddPinButtonRefference.SetActive(false);
            if (LabelModeSelectorRefference != null) LabelModeSelectorRefference.SetActive(false);
        }
        UpdateLabelModeVisual();
    }

    public void Initialize(PinManager pinManager) {
        _pinManager = pinManager;
    }

    public void OnExitEditing() {
        UpdatePinButtonList();
        foreach (PinHUDButton button in _buttonList) {
            button.ToggleMovableVisual(false);
        }

        ResetButtonsVisual();
    }

    public void OnAddPin() {
        _pinManager.StartPlacingNewPin();
    }

    /// <summary>Label mode buttons, the argument is the PinLabelMode as int.</summary>
    public void OnLabelModeButton(int mode) {
        _pinManager.SetLabelMode((PinLabelMode)mode);
    }

    // The current mode's button is filled, the others outlined
    public void UpdateLabelModeVisual() {
        if (_pinManager == null || LabelModeButtonImages == null) return;
        for (int i = 0; i < LabelModeButtonImages.Length; i++) {
            bool selected = i == (int)_pinManager.LabelMode;
            if (LabelModeButtonImages[i] != null) LabelModeButtonImages[i].color = selected ? LabelModeSelectedColor : LabelModeUnselectedColor;
            if (LabelModeButtonTexts != null && i < LabelModeButtonTexts.Length && LabelModeButtonTexts[i] != null)
                LabelModeButtonTexts[i].color = selected ? LabelModeUnselectedColor : LabelModeSelectedColor;
        }
    }

    public void OnHUDButtonClick(PinObject pin) {
        _pinManager.SelectPin(pin);
        _pinManager.FocusPin(pin);
        UpdatePinButtonList();
    }

    public void OnMoveButton(int index, bool moveUp) {
        int newIndex = moveUp ? index - 1 : index + 1;
        List<PinObject> list = _pinManager.GetPins();

        // Bounds check
        if (newIndex < 0 || newIndex >= list.Count) return;

        _pinManager.SwapPins(index, newIndex);
        UpdatePinButtonList();
    }

    public void UpdatePinButtonList() {
        ClearPinButtonList();

        foreach (PinObject pin in _pinManager.GetPins()) {
            PinHUDButton buttonScript = Instantiate(PinButtonPrefab, ScrollViewContentRefference.transform).GetComponent<PinHUDButton>();
            buttonScript.Initialize(this, pin);
            _buttonList.Add(buttonScript);
        }

        // Always keep the "+" button as the last element in the Scroll Content
        if (MainManagerBase.Instance is EditorManager)
            if (AddPinButtonRefference != null && AddPinButtonRefference.transform.parent == ScrollViewContentRefference.transform) 
                AddPinButtonRefference.transform.SetAsLastSibling();
        
    }

    public void ClearPinButtonList() {
        foreach (PinHUDButton button in _buttonList) {
            if (button != null) {
                Destroy(button.gameObject);
            }
        }

        _buttonList.Clear();
    }

    public void ResetButtonsVisual() {
        foreach (PinHUDButton button in _buttonList) {
            button.ToggleVisual(false);
        }
    }
}
