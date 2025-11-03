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
        // If changing from view mode
        if (EditorModeCurrent == EditorMode.View) {
            // Return camera to the original position
            ViewManager.Instance.ExitViewMoving();
            /*
            CinemachineBrain brain = CinemachineCore.Instance.GetActiveBrain(0);
            var originalBlend = brain.m_DefaultBlend;
            var newBlend = new CinemachineBlendDefinition(originalBlend.m_Style, 0.001f); 
            brain.m_DefaultBlend = newBlend;
            brain.ManualUpdate();
            brain.m_DefaultBlend = originalBlend;
            */
        }

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
            case EditorMode.Initialization:
                EnterModeInitialization();
                break;
            case EditorMode.SurveyCreation:
                EnterModeSurvey();
                break;
        }
    }

    #region Editor mode logic

    void EnterModeInitialization() {
        UImanager.Instance.ShowUI(UIType.EditorInitUI);
    }

    void EnterModeFreecam() {
        UImanager.Instance.HideUI(UIType.EditorInitUI);
        UImanager.Instance.ShowUI(UIType.EditorHUD);
        //      if (TwoCameraInstantiated != null) Destroy(TwoCameraInstantiated);
        StartCoroutine(DisableCinemachineAfterTransition());
    }

    void EnterModeGeolocation() {
        UImanager.Instance.HideUI(UIType.EditorInitUI);
        UImanager.Instance.HideUI(UIType.EditorHUD);
        EditorCameraManager.Instance.UpdateFreeCamVcamPosition();
        CinemachineBrainRefference.enabled = (true);
        MapManager.Instance.ToggleMapVisibility();
        GeoMapManager.Instance.ActivateGeoLocalization();
    }

    void EnterModeTwoCameras() {
        if (!MapManager.Instance.hasVariant()) {
            MessageDisplayManager.Instance.DisplayMessage("No variants added!");
            return;
        }

        EditorCameraManager.Instance.UpdateFreeCamVcamPosition();
        UImanager.Instance.HideUI(UIType.EditorHUD);
        CinemachineBrainRefference.enabled = (true);
        MapDisplayManager.Instance.EnterMultiView();
    }

    void EnterModeView() {
        EditorCameraManager.Instance.UpdateFreeCamVcamPosition();
        CinemachineBrainRefference.enabled = (true);
        CinemachineCore.Instance.GetActiveBrain(0).ManualUpdate();
        ViewManager.Instance.StartViewMoving();
    }

    void EnterModeSurvey() {
        SurveyManager.Instance.EnterSurveyBuilding();
        UImanager.Instance.HideUI(UIType.EditorHUD);
    }

    #endregion

    public void ExitEditor() {
        ProjectManager.Instance.CloseProject();
    }

    IEnumerator DisableCinemachineAfterTransition() {
        yield return new WaitForSeconds(0.01f); // Wait so the blend can start
        while (CinemachineBrainRefference.IsBlending) {
            yield return null;
        }
        CinemachineBrainRefference.enabled = (false);
    }

}

public enum EditorMode {
    Initialization,
    Freecam,
    GeoLocalization,
    TwoMaps,
    View,
    SurveyCreation,
}