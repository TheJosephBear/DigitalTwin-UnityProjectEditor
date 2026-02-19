using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewPointUI : UIBehaviour {

    public GameObject ViewPointButtonPrefab;
    public GameObject ScrollViewContentRefference;

    ViewManager _viewManager;

    public override void Show() {
        base.Show();
    }

    public void Initialize(ViewManager viewManager) {
        _viewManager = viewManager;
        print("INITIALIZED: "+_viewManager.name);
    }

    public void OnAddView() {
        _viewManager.CreateNewViewPoint();
        UpdateViewButtonList();
    }

    public void OnHUDButtonClick(ViewPoint ViewPointRefference) {
        _viewManager.SetActiveViewPoint(ViewPointRefference);

        // Toggle state
        if (MainManagerBase.Instance.ActiveState == ProjectState.Freecam) {
            MainManagerBase.Instance.ChangeState(ProjectState.ViewActive);
        } else if (MainManagerBase.Instance.ActiveState == ProjectState.ViewActive) {
            MainManagerBase.Instance.ChangeState(ProjectState.Freecam);
        }
    }

    public void UpdateViewButtonList() {
        ClearViewButtonList();

        print("viewpoints count: " + _viewManager.name);
        print("viewpoints count: " + _viewManager.GetViewPoints().Count);
        print("viewpoints count: " + _viewManager.GetViewPoints().Count);
        foreach (ViewPoint vp in _viewManager.GetViewPoints()) {
            ViewHUDButton buttonScript = Instantiate(ViewPointButtonPrefab, ScrollViewContentRefference.transform).GetComponent<ViewHUDButton>();
            buttonScript.Initialize(this, vp);
            print("added butt script" + buttonScript.name);
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
