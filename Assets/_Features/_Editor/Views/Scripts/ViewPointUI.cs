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

    public void OnAddView() {
        _viewManager.CreateNewViewPoint();
        UpdateViewButtonList();
    }

    public void OnHUDButtonClick(ViewPoint ViewPointRefference) {
        _viewManager.SetActiveViewPoint(ViewPointRefference);
        UpdateViewButtonList();

        if (MainManagerBase.Instance.ActiveState == AppState.ViewActive) {
            ViewManager.Instance.StartViewMoving();
        } else {
            MainManagerBase.Instance.ChangeState(AppState.ViewActive);
        }

        /*
        if (MainManagerBase.Instance is EditorManager manager) {
            MainManagerBase.Instance.ChangeState(AppState.ViewActive);
        } else {
            // Toggle state
            if (MainManagerBase.Instance.ActiveState == AppState.Freecam) {
                MainManagerBase.Instance.ChangeState(AppState.ViewActive);
            } else if (MainManagerBase.Instance.ActiveState == AppState.ViewActive) {
                MainManagerBase.Instance.ChangeState(AppState.Freecam);
            }
        }
        */
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
