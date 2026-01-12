using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeolocalizationState : EditorStateBase {
    public override void Enter() {
        UIManager.Instance.HideUI(UIType.EditorInitUI);
        UIManager.Instance.HideUI(UIType.EditorHUD);
        EditorManager.Instance.EditorCameraManager.UpdateFreeCamVcamPosition();
        EditorManager.Instance.EditorCameraManager.ToggleCinemachineBrain(true);
        EditorManager.Instance.MapManager.ToggleMapVisibility();
        EditorManager.Instance.GeoMapManager.ActivateGeoLocalization();
    }

    public override void Exit() {

    }
}