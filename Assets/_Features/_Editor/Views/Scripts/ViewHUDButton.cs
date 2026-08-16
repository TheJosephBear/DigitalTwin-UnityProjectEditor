using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViewHUDButton : MonoBehaviour {

    public ViewPointUI UIreff;
    public ViewPoint ViewPointRefference;
    public Image ButtonImageRef;

    [SerializeField] float _inactiveHeight = 100f;
    [SerializeField] float _activeHeight = 200f;
    [SerializeField] GameObject ArrowsContainer;
    [SerializeField] GameObject ArrowUpButton;
    [SerializeField] GameObject ArrowDownButton;
    ViewPoint _previousActiveViewPoint;
    bool _isActiveViewpoint;


    public void Initialize(ViewPointUI ui, ViewPoint vp) {
        UIreff = ui;
        ViewPointRefference = vp;
        _isActiveViewpoint = ViewManager.Instance.GetActiveViewPoint() == ViewPointRefference;
        ToggleVisual(_isActiveViewpoint);
        ToggleMovableVisual(_isActiveViewpoint);
    }

    public void OnClick() {
        UIreff.OnHUDButtonClick(ViewPointRefference);
    }

    public void ToggleVisual(bool toggleOn) {
    //    print(toggleOn);
        string color = "#FFFFFF";
        if (toggleOn) {
            color = "#FF92FE";
        }

        if (ColorUtility.TryParseHtmlString(color, out Color newColor)) {
            ButtonImageRef.color = newColor;
        }
    }

    public void OnHover() {
        if (ViewManager.Instance.isViewMovingActive) return;

        _previousActiveViewPoint = ViewManager.Instance.GetActiveViewPoint();
        ViewManager.Instance.SetActiveViewPoint(ViewPointRefference);
        ViewManager.Instance.ToggleCameraPreview(true);
        if (_previousActiveViewPoint != null) ViewManager.Instance.SetActiveViewPoint(_previousActiveViewPoint);
    }

    public void OnUnhover() {
        ViewManager.Instance.ToggleCameraPreview(false);
    }

    public void OnArrowUp() {
        UIreff.OnMoveButton(transform.GetSiblingIndex(), moveUp: true);
    }

    public void OnArrowDown() {
        UIreff.OnMoveButton(transform.GetSiblingIndex(), moveUp: false);
    }
    
    public void ToggleMovableVisual(bool toggleOn) {
        RectTransform rectTransform = GetComponent<RectTransform>();

        Vector2 size = rectTransform.sizeDelta;
        size.y = toggleOn ? _activeHeight : _inactiveHeight;
        rectTransform.sizeDelta = size;

        ArrowsContainer.SetActive(toggleOn);
    }
}
