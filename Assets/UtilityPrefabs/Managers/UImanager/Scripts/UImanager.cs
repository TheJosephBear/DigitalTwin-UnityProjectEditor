using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UImanager : MonoBehaviour {
    public static UImanager Instance { get; private set; }
    public List<UIElement> uiElements;
    UIType openedUI;
    UIType savedUI;

    void Awake() {
        if (Instance == null) {
            Instance = this;
            HideAllUIs();
        } else {
            Destroy(gameObject);
        }
    }

    public void HideAllUIs() {
        foreach (UIElement uiElement in uiElements) {
            uiElement.uiScript?.Hide();
        }
    }

    public void ShowUI(UIType uiType) {
        var uiElement = uiElements.FirstOrDefault(element => element.uiType == uiType);
        if (uiElement != null) {
            openedUI = uiElement.uiType;
            uiElement.uiScript.Show();
            if(uiElement.defaultSelectedButton != null) EventSystem.current.SetSelectedGameObject(uiElement.defaultSelectedButton.gameObject); // this is Button. i want to select it via code
        } else {
            Debug.LogWarning($"UIType {uiType} not found.");
        }
    }

    public void HideUI(UIType uiType) {
        var uiElement = uiElements.FirstOrDefault(element => element.uiType == uiType);
        if (uiElement != null) {
            uiElement.uiScript.Hide();
        } else {
            Debug.LogWarning($"UIType {uiType} not found.");
        }
    }

    public void ToggleUI(UIType uiType) {
        var uiElement = uiElements.FirstOrDefault(element => element.uiType == uiType);
        if (uiElement != null) {
            bool isActive = uiElement.canvas.gameObject.activeSelf;
            uiElement.canvas.gameObject.SetActive(!isActive);
        } else {
            Debug.LogWarning($"UIType {uiType} not found.");
        }
    }

    public void ToggleAllButtonsInUI(UIType uiType, bool enable) {
        var uiElement = uiElements.FirstOrDefault(element => element.uiType == uiType);
        if (uiElement != null) {
            var buttons = uiElement.canvas.GetComponentsInChildren<Button>(true);
            foreach (var button in buttons) {
                button.interactable = enable;
            }
        } else {
            Debug.LogWarning($"UIType {uiType} not found.");
        }
    }

    public void SaveOpenedUI(UIType uiType) {
        savedUI = uiType;
    }
    public void ShowSavedUI() {
        ShowUI(savedUI);
    }
}
public enum UIType {
    Login,
    Register,
    Projects,
    EditorHUD
}

[System.Serializable]
public class UIElement {
    public UIType uiType;
    public Canvas canvas;
    public UIBehaviour uiScript;
    public Button defaultSelectedButton;
}