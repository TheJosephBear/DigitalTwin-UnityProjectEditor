using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorManager : Singleton<EditorManager> {
    /// <summary>
    /// Controls the state of the Editor
    /// </summary>

    public GameObject TwoCameraPrefab;
    public Vector3 TwoCameraPrefabSpawnPosition;
    GameObject TwoCameraInstantiated;

    public EditorMode EditorModeCurrent { get; private set; }

    protected override void Awake() {
        base.Awake();

    }

    public void ToggleCameraViewMode() {
        if (EditorModeCurrent == EditorMode.classic) {
            ChangeEditorMode(EditorMode.showingOffCamera);
        } else if (EditorModeCurrent == EditorMode.showingOffCamera) {
            ChangeEditorMode(EditorMode.classic);
        }
    }

    public void ChangeEditorMode(EditorMode viewMode) {
        EditorModeCurrent = viewMode;
        switch (viewMode) {
            case EditorMode.classic:
                ViewModeClassic();
                break;
            case EditorMode.twoMaps:
                ViewModeTwoMaps();
                break;
            case EditorMode.showingOffCamera:
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

public enum EditorMode {
    classic,
    twoMaps,
    showingOffCamera,
    // New
    Freecam,
    GeoLocalization,
    View
}