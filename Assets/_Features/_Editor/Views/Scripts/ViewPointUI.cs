using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewPointUI : MonoBehaviour {

    public GameObject ViewPointButtonPrefab;
    public GameObject ScrollViewContentRefference;

    public void OnAddView() {
        ViewManager.Instance.CreateNewViewPoint();
        UpdateViewButtonList();
    }

    public void UpdateViewButtonList() {
        foreach (Transform child in ScrollViewContentRefference.transform) {
            if (child.GetComponent<ScrolviewAlwaysLastItem>() == null) {
                Destroy(child.gameObject);
            }
        }

        foreach (ViewPoint vp in ViewManager.Instance.GetViewPoints()) {
            ViewHUDButton buttonScript = Instantiate(ViewPointButtonPrefab, ScrollViewContentRefference.transform).GetComponent<ViewHUDButton>();
            buttonScript.ViewPointRefference = vp;
        }
    }

    void AddViewButtonToList() {

    }
}
