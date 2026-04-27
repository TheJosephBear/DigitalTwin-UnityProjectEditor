using UnityEngine;

public class SurveyViewingState : StateBase {
    public override void Enter() {
        MainManagerBase.Instance.ToggleHUD(false);
        MainManagerBase.Instance.ViewManager.ToggleCameraPreview(false);
        MainManagerBase.Instance.ViewManager.ToggleViewPointUI(false);
        MainManagerBase.Instance.SurveyManager.EnterSurveyViewing();
        EditorManager.Instance.EditorCameraManager.ToggleCinemachineBrain(true);
    }

    public override void Exit() {

    }
}