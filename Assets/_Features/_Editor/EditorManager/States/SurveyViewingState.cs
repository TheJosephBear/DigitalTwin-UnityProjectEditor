using Cinemachine;
using UnityEngine;

public class SurveyViewingState : StateBase {
    public override void Enter() {
        MainManagerBase.Instance.ToggleHUD(false);
        MainManagerBase.Instance.ViewManager.ToggleCameraPreview(false);
        MainManagerBase.Instance.ViewManager.ToggleViewPointUI(false);
        SurveyManager.Instance.EnterSurveyViewing(hasData => {
            if (!hasData) {
                MainManagerBase.Instance.ChangeState(AppState.Freecam);
            }
        });
        EditorManager.Instance.EditorCameraManager.ToggleCinemachineBrain(false); // So you can move the cam via RTG
    }

    public override void Exit() {

    }
}