using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewPointUI : MonoBehaviour {

    public GameObject ViewPointButtonPrefab;
    public GameObject ScrollViewContentRefference;
    public GameObject AddViewButtonRefference;

    List<ViewHUDButton> _buttonList = new List<ViewHUDButton>();
    ViewManager _viewManager;


    public void Initialize(ViewManager viewManager, bool showAddViewButton) {
        _viewManager = viewManager;
        AddViewButtonRefference.SetActive(showAddViewButton);
    }

    public void OnExitMoving() {
        UpdateViewButtonList();
        foreach (ViewHUDButton button in _buttonList) {
            button.ToggleMovableVisual(false);
        }

        ResetButtonsVisual();
    }

    public void OnAddView() {
        ViewPoint newViewpoint = _viewManager.CreateNewViewPoint().GetComponent<ViewPoint>();
        // Activate!
        _viewManager.SetActiveViewPoint(newViewpoint);
        _viewManager.ActivateViewPoint();

        if (MainManagerBase.Instance.ActiveState != AppState.ViewActive) {
            MainManagerBase.Instance.ChangeState(AppState.ViewActive);
        } else {
            _viewManager.StartViewMoving();
        }

        UpdateViewButtonList();
    }

    public void OnHUDButtonClick(ViewPoint ViewPointRefference) {
        bool clickedSameViewTwice = ViewPointRefference == _viewManager.GetActiveViewPoint();

        _viewManager.SetActiveViewPoint(ViewPointRefference);
        UpdateViewButtonList();

        if (SceneLoadingManager.Instance.GetActiveScene() == SceneType.Viewing) {
            if (MainManagerBase.Instance.ActiveState != AppState.ViewActive) {
                MainManagerBase.Instance.ChangeState(AppState.ViewActive);
            }

            // 1. Trigger the camera movement to the viewpoint
            ViewManager.Instance.ActivateViewPoint();

            // 2. Immediately switch to Freecam and reset button visuals
            MainManagerBase.Instance.ChangeState(AppState.Freecam);
            ResetButtonsVisual();
        } else {
            if (MainManagerBase.Instance.ActiveState == AppState.ViewActive) {
                ViewManager.Instance.StartViewMoving();
            } else {
                MainManagerBase.Instance.ChangeState(AppState.ViewActive);
            }
        }
    }

    public void OnMoveButton(int index, bool moveUp) {
        int newIndex = moveUp ? index - 1 : index + 1;
        List<ViewPoint> list = _viewManager.GetViewPoints();

        // Bounds check
        if (newIndex < 0 || newIndex >= list.Count) return;

        // 1. Swap elements in ViewManager's list
        ViewPoint temp = list[index];
        list[index] = list[newIndex];
        list[newIndex] = temp;

        // 2. Re-render UI buttons in the new list order
        UpdateViewButtonList();
    }

    public void UpdateViewButtonList() {
        ClearViewButtonList();

        foreach (ViewPoint vp in _viewManager.GetViewPoints()) {
            ViewHUDButton buttonScript = Instantiate(ViewPointButtonPrefab, ScrollViewContentRefference.transform).GetComponent<ViewHUDButton>();
            buttonScript.Initialize(this, vp);
            _buttonList.Add(buttonScript);
        }
    }

    public void ClearViewButtonList() {
        /*
        foreach (Transform child in ScrollViewContentRefference.transform) {
            if (child.GetComponent<ScrolviewAlwaysLastItem>() == null) {
                Destroy(child.gameObject);
            }
        }
        */
        foreach (ViewHUDButton button in _buttonList) {
            if (button != null) {
                Destroy(button.gameObject);
            }
        }

        _buttonList.Clear();
    }

    public void ResetButtonsVisual() {
        foreach (ViewHUDButton button in _buttonList) {
             button.ToggleVisual(false);
        }
    }
}
