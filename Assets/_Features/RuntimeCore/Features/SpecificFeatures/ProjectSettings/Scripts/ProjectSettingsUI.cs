using UISwitcher;
using UnityEngine;

public class ProjectSettingsUI: MonoBehaviour {

    ProjectSettingsManager _manager;
    public UISwitcher.UISwitcher CollToggleRef;
    public UISwitcher.UISwitcher FPSToggleRef;
    public UISwitcher.UISwitcher MultiviewToggleRef;
    public UISwitcher.UISwitcher SurveyToggleRef;

    public void Initialize(ProjectSettingsManager manager) {
        _manager = manager;
        CollToggleRef.SetWithoutNotify(manager.ViewerCameraCollision);
        FPSToggleRef.SetWithoutNotify(manager.FirstPersonAllowed);
        MultiviewToggleRef.SetWithoutNotify(manager.MultiviewAllowed);
        SurveyToggleRef.SetWithoutNotify(manager.SurveyAllowed);
    }

    public void OnCamCollision(bool val) {
        _manager.ViewerCameraCollision = val;
    }

    public void OnFPS(bool val) {
        _manager.FirstPersonAllowed = val;
    }

    public void OnMultiview(bool val) {
        _manager.MultiviewAllowed = val;
    }

    public void OnSurvey(bool val) {
        _manager.SurveyAllowed = val;
    }

    public void OnX() {
        _manager.ToggleUI(false);
    }
}
