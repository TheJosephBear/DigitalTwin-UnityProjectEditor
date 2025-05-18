using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorManager : Singleton<EditorManager> {
    /// <summary>
    /// Controls the state of the Editor itself
    /// </summary>

    public GameObject TwoCameraPrefab;
    public Vector3 TwoCameraPrefabSpawnPosition;
    GameObject TwoCameraInstantiated;

    public EditorViewMode ViewModeCurrent { get; private set; }

    protected override void Awake() {
        base.Awake();

    }

    public void ToggleCameraViewMode() {
        if (ViewModeCurrent == EditorViewMode.classic) {
            ChangeEditorViewMode(EditorViewMode.showingOffCamera);
        } else if (ViewModeCurrent == EditorViewMode.showingOffCamera) {
            ChangeEditorViewMode(EditorViewMode.classic);
        }
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
            case EditorViewMode.showingOffCamera:
                ViewModeCameraShowing();
                break;
        }
    }

    void ViewModeClassic() {
        UImanager.Instance.HideUI(UIType.TwoMapsCameraView);
        if (TwoCameraInstantiated != null) Destroy(TwoCameraInstantiated);
        ViewManager.Instance.DeactivateViewPoint();
    }

    void ViewModeTwoMaps() {
        if (!MapManager.Instance.hasVariant())
            return;

        TwoCameraInstantiated = SceneLoadingManager.Instance.InstantiateObjectInScene(TwoCameraPrefab, TwoCameraPrefabSpawnPosition, SceneType.Editing);
        UImanager.Instance.ShowUI(UIType.TwoMapsCameraView);
        FindAnyObjectByType<TwoMapsUI>().Initialize();
    }

    void ViewModeCameraShowing() {
        ViewManager.Instance.ActivateViewPoint();
    }


}

public enum EditorViewMode {
    classic,
    twoMaps,
    showingOffCamera
}