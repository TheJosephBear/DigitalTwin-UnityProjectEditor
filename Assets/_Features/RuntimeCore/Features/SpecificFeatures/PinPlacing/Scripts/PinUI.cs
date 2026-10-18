using System.Collections.Generic;
using UnityEngine;

public class PinUI: MonoBehaviour {

    public GameObject PinButtonPrefab;
    public GameObject ScrollViewContentRefference;
    public GameObject AddPinButtonRefference;

    List<PinHUDButton> _buttonList = new List<PinHUDButton>();
    PinManager _pinManager;


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

    public void OnHUDButtonClick(PinObject pin) {
        _pinManager.SelectPin(pin);
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
        if (AddPinButtonRefference != null && AddPinButtonRefference.transform.parent == ScrollViewContentRefference.transform) {
            AddPinButtonRefference.transform.SetAsLastSibling();
        }
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
