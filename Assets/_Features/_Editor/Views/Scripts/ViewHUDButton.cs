using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewHUDButton : MonoBehaviour {

    public ViewPoint ViewPointRefference;

    public void OnClick() {
        EditorManager.Instance.ViewManager.SetActiveViewPoint(ViewPointRefference);

        // Toggle state
        if (EditorManager.Instance.ActiveState == EditorState.Freecam) {
            EditorManager.Instance.ChangeEditorMode(EditorState.ViewActive);
        } else if (EditorManager.Instance.ActiveState == EditorState.ViewActive) {
            EditorManager.Instance.ChangeEditorMode(EditorState.Freecam);
        }
    }

}
