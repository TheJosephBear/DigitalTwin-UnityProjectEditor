using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurveycreationState : StateBase {
    public override void Enter() {
        MainManagerBase.Instance.ToggleHUD(false);
        ViewManager.Instance.ToggleCameraPreview(false);
        ViewManager.Instance.ToggleViewPointUI(false);
        SurveyManager.Instance.EnterSurveyBuilding();
        CameraManager.Instance.ToggleFreecamUpdating(true);
        CameraManager.Instance.ToggleBoundsEnforcing(true);
    }

    public override void Exit() {
        CameraManager.Instance.ToggleFreecamUpdating(false);
        CameraManager.Instance.ToggleBoundsEnforcing(false);

    }
}
