using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewHUDButton : MonoBehaviour {
    public ViewPoint ViewPointRefference;

    public void OnClick() {
        ViewManager.Instance.SetActiveViewPoint(ViewPointRefference);
        EditorManager.Instance.ToggleViewMode();
    }

}
