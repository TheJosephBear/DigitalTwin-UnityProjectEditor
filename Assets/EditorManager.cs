using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorManager : Singleton<EditorManager> {
    /// <summary>
    /// Controls the state of the Editor itself
    /// </summary>

    EditorViewMode viewModeCurrent;

    void Awake() {

    }


    public void ChangeEditorViewMode(EditorViewMode viewMode) {
        viewModeCurrent = viewMode;
        switch (viewMode) {
            case EditorViewMode.classic:
                ViewModeClassic();
                break;
            case EditorViewMode.twoMaps:
                ViewModeTwoMaps();
                break;
        }
    }

    void ViewModeClassic() {

    }

    void ViewModeTwoMaps() {
        // Kamera má extra kameru, ta vidí tu extra verzi, render texture ukazuje vedle sebe
    }


}

public enum EditorViewMode {
    classic,
    twoMaps
}