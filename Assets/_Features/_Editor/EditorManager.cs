using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class EditorManager : Singleton<EditorManager> {
    /// <summary>
    /// Controls the state of the Editor
    /// </summary>

    public CinemachineBrain CinemachineBrainRefference;
    public GameObject TwoCameraPrefab;
    public Vector3 TwoCameraPrefabSpawnPosition;

    public EditorMode EditorModeCurrent { get; private set; }

    protected override void Awake() {
        base.Awake();

    }

    public void ToggleViewMode() {
        if (EditorModeCurrent == EditorMode.Freecam) {
            ChangeEditorMode(EditorMode.View);
        } else if (EditorModeCurrent == EditorMode.View) {
            ChangeEditorMode(EditorMode.Freecam);
        }
    }

    public void ChangeEditorMode(EditorMode editorMode) {
        EditorModeCurrent = editorMode;
        switch (editorMode) {
            case EditorMode.Freecam:
                EnterModeFreecam();
                break;
            case EditorMode.GeoLocalization:
                EnterModeGeolocation();
                break;
            case EditorMode.TwoMaps:
                EnterModeTwoCameras();
                break;
            case EditorMode.View:
                EnterModeView();
                break;
        }
    }

    #region Editor mode logic

    void EnterModeFreecam() {
        UImanager.Instance.ShowUI(UIType.EditorHUD);
  //      if (TwoCameraInstantiated != null) Destroy(TwoCameraInstantiated);
        ViewManager.Instance.DeactivateViewPoint();
        CinemachineBrainRefference.enabled = (false);
    }

    void EnterModeGeolocation() {
        if (MapManager.Instance.IsBaseMapUploaded()) {
            UImanager.Instance.HideUI(UIType.EditorHUD);
            CinemachineBrainRefference.enabled = (true);
            MapManager.Instance.ToggleMapVisibility();
            GeoMapManager.Instance.ActivateGeoLocalization();
        } else {
            MessageDisplayManager.Instance.DisplayMessage("Upload a map model first!");
        }
    }

    void EnterModeTwoCameras() {
        if (!MapManager.Instance.hasVariant()) {
            MessageDisplayManager.Instance.DisplayMessage("No variants added!");
            return;
        }

        UImanager.Instance.HideUI(UIType.EditorHUD);
        CinemachineBrainRefference.enabled = (true);
        MapDisplayManager.Instance.EnterMultiView();
    }

    void EnterModeView() {
        CinemachineBrainRefference.enabled = (true);
        ViewManager.Instance.ActivateViewPoint();
    }

    #endregion

}

public enum EditorMode {
    Freecam,
    GeoLocalization,
    TwoMaps,
    View,
}