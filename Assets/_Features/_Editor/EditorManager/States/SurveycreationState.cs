using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurveycreationState : StateBase {
    public override void Enter() {
        UIManager.Instance.HideUI(UIType.EditorHUD);
        EditorManager.Instance.ViewManager.ToggleCameraPreview(false);
        EditorManager.Instance.ViewManager.ToggleViewPointUI(false);
        EditorManager.Instance.SurveyManager.EnterSurveyBuilding();
    }

    public override void Exit() {

    }
}