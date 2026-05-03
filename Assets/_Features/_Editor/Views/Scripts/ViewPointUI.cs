using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewPointUI : MonoBehaviour {

    public GameObject ViewPointButtonPrefab;
    public GameObject ScrollViewContentRefference;
    public GameObject AddViewButtonRefference;

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

        // Toggle state
        if (MainManagerBase.Instance.ActiveState == AppState.Freecam) {
            MainManagerBase.Instance.ChangeState(AppState.ViewActive);
        } else if (MainManagerBase.Instance.ActiveState == AppState.ViewActive) {
       //     MainManagerBase.Instance.ChangeState(AppState.Freecam);
        }
    }

    public void UpdateViewButtonList() {
        ClearViewButtonList();

        foreach (ViewPoint vp in _viewManager.GetViewPoints()) {
            ViewHUDButton buttonScript = Instantiate(ViewPointButtonPrefab, ScrollViewContentRefference.transform).GetComponent<ViewHUDButton>();
            buttonScript.Initialize(this, vp);
        }
    }

    public void ClearViewButtonList() {
        foreach (Transform child in ScrollViewContentRefference.transform) {
            if (child.GetComponent<ScrolviewAlwaysLastItem>() == null) {
                Destroy(child.gameObject);
            }
        }
    }
}
