using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorManager : Singleton<EditorManager> {
    /// <summary>
    /// Controls the state of the Editor itself
    /// </summary>

    public EditorViewMode ViewModeCurrent { get; private set; }

    protected override void Awake() {
        base.Awake();

    }

    public void ChangeEditorViewMode(EditorViewMode viewMode) {
        ViewModeCurrent = viewMode;
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
        UImanager.Instance.HideUI(UIType.TwoMapCamera);

    }

    void ViewModeTwoMaps() {
        if (!MapManager.Instance.hasVariant())
            return;

        UImanager.Instance.ShowUI(UIType.TwoMapCamera);
        MapManager.Instance.SpawnSelectedVariant(0);
        FindAnyObjectByType<TwoMapsUI>().UpdateDropDown();
    }



}

public enum EditorViewMode {
    classic,
    twoMaps
}