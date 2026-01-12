using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurveycreationState : EditorStateBase {
    public override void Enter() {
        EditorManager.Instance.SurveyManager.EnterSurveyBuilding();
        UIManager.Instance.HideUI(UIType.EditorHUD);
        EditorManager.Instance.ViewManager.ToggleCameraPreview(false);
    }

    public override void Exit() {

    }
}