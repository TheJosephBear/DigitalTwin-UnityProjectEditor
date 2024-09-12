using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ContextMenu : MonoBehaviour {
    [SerializeField]
    List<ContextMenuButtonEditor> buttons = new List<ContextMenuButtonEditor>();
    GameObject canvasPrefab;
    GameObject contextMenuPrefab;
    GameObject buttonPrefab;
    EventSystem eventSystem;
    GraphicRaycaster graphicRaycaster;
    PointerEventData pointerEventData;
    GameObject activeContextMenu;
    bool isContextMenuActive = false;

    void Start() {
        ContextMenuManager manager = FindAnyObjectByType<ContextMenuManager>();
        canvasPrefab = manager.canvasPrefab;
        contextMenuPrefab = manager.contextMenuPrefab;
        buttonPrefab = manager.buttonPrefab;
        eventSystem = FindObjectOfType<EventSystem>();
        graphicRaycaster = FindObjectOfType<GraphicRaycaster>();
    }

    void Update() {
        if (isContextMenuActive && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))) {
            print("Closing!!!");
            CloseContextMenu();
        }
        if (Input.GetMouseButtonDown(1)) {
            CheckRightClickUI();
        }
    }

    void CheckRightClickUI() {
        pointerEventData = new PointerEventData(eventSystem) {
            position = Input.mousePosition
        };
        var results = new List<RaycastResult>();
        graphicRaycaster.Raycast(pointerEventData, results);

        foreach (var result in results) {
            if (result.gameObject == this.gameObject) {
                Vector2 clickPos = Input.mousePosition;
                OpenContextMenu(clickPos);
                return;
            }
        }
    }

    void OpenContextMenu(Vector2 clickPos) {
        GameObject canvas = Instantiate(canvasPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        activeContextMenu = Instantiate(contextMenuPrefab, canvas.transform);
        SetContextMenuSize();
        InstantiateButtons();
        SetContextMenuPosition(clickPos);
        isContextMenuActive = true;
    }

    void InstantiateButtons() {
        foreach (var buttonEditor in buttons) {
            GameObject newButton = Instantiate(buttonPrefab, activeContextMenu.transform);
            ContextMenuButton contextMenuButton = newButton.GetComponent<ContextMenuButton>();
            contextMenuButton.Initialize(buttonEditor);
        }
    }

    void SetContextMenuPosition(Vector2 clickPosition) {
        Vector2 menuSize = activeContextMenu.GetComponent<RectTransform>().sizeDelta;
        Vector2 position = clickPosition;

        if (position.x <= Screen.width / 2) {
            position.x += menuSize.x;
        } else {
            position.x -= menuSize.x;
        }
        position.y -= menuSize.y;

        if (position.x + menuSize.x > Screen.width)
            position.x = Screen.width - menuSize.x;
        if (position.y + menuSize.y > Screen.height)
            position.y = Screen.height - menuSize.y;

        Vector2 localPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            activeContextMenu.transform as RectTransform,
            position,
            null,
            out localPosition);

        activeContextMenu.GetComponent<RectTransform>().anchoredPosition = localPosition;
    }

    void SetContextMenuSize() {
        float buttonHeight = buttonPrefab.GetComponent<RectTransform>().sizeDelta.y;
        float totalHeight = (buttonHeight * buttons.Count) + (2 * (buttons.Count - 1));
        activeContextMenu.GetComponent<RectTransform>().sizeDelta = new Vector2(activeContextMenu.GetComponent<RectTransform>().sizeDelta.x, totalHeight);
    }


    public void CloseContextMenu() {
        if (activeContextMenu != null) {
            Destroy(activeContextMenu);
            activeContextMenu = null;
            isContextMenuActive = false;
        }
    }
}

[System.Serializable]
public class ContextMenuButtonEditor {
    public string text = "Default text";
    public UnityEvent onClickAction;
}
