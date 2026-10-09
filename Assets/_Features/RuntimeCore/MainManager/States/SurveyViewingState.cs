using Cinemachine;
using UnityEditor;
using UnityEngine;

public class SurveyViewingState : StateBase {
    public override void Enter() {
        MainManagerBase.Instance.ToggleHUD(false);
        ViewManager.Instance.ToggleCameraPreview(false);
        ViewManager.Instance.ToggleViewPointUI(false);
        SurveyManager.Instance.EnterSurveyViewing(hasData => {
            if (!hasData) {
                MainManagerBase.Instance.ChangeState(AppState.Freecam);
            }
        });
        CameraManager.Instance.ToggleCinemachineBrain(false); // So you can move the cam via RTG
    }

    public override void Exit() {

    }
}
