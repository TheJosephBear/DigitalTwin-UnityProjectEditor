using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewPointUI : MonoBehaviour {

    public GameObject ViewPointButtonPrefab;
    public GameObject ScrollViewContentRefference;

    public void OnAddView() {
        EditorManager.Instance.ViewManager.CreateNewViewPoint();
        UpdateViewButtonList();
    }

    public void UpdateViewButtonList() {
        ClearViewButtonList();

        foreach (ViewPoint vp in EditorManager.Instance.ViewManager.GetViewPoints()) {
            ViewHUDButton buttonScript = Instantiate(ViewPointButtonPrefab, ScrollViewContentRefference.transform).GetComponent<ViewHUDButton>();
            buttonScript.ViewPointRefference = vp;
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
