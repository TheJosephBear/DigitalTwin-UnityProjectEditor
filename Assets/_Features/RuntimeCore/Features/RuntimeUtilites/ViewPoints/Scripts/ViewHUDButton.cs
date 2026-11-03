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
    bool _isActiveViewpoint;


    public void Initialize(ViewPointUI ui, ViewPoint vp) {
        UIreff = ui;
        ViewPointRefference = vp;
        _isActiveViewpoint = ViewManager.Instance.GetActiveViewPoint() == ViewPointRefference;
        ToggleVisual(_isActiveViewpoint);
        ToggleMovableVisual(_isActiveViewpoint);

        // The first item can't move up and the last can't move down
        List<ViewPoint> viewPoints = ViewManager.Instance.GetViewPoints();
        int index = viewPoints.IndexOf(vp);
        ArrowUpButton.SetActive(index > 0);
        ArrowDownButton.SetActive(index < viewPoints.Count - 1);
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
        print("View moving active? " + ViewManager.Instance.isViewMovingActive);
        if (ViewManager.Instance.isViewMovingActive) return;

        // Preview without switching the active view point: switching deactivates its vcam and moves the free cam,
        // which cut the camera blend started by clicking a view point in the viewer (the list rebuilds under the cursor)
        ViewManager.Instance.ToggleCameraPreview(true, ViewPointRefference);
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
        if (MainManagerBase.Instance is ViewingManager) {
            toggleOn = false;
        }

        RectTransform rectTransform = GetComponent<RectTransform>();

        Vector2 size = rectTransform.sizeDelta;
        size.y = toggleOn ? _activeHeight : _inactiveHeight;
        rectTransform.sizeDelta = size;

        ArrowsContainer.SetActive(toggleOn);
    }
}
