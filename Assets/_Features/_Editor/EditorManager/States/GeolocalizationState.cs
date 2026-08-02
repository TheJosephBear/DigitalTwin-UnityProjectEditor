using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeolocalizationState : StateBase {
    public override void Enter() {
        UIManager.Instance.HideUI(UIType.EditorInitUI);
        MainManagerBase.Instance.ToggleHUD(false);
        EditorManager.Instance.EditorCameraManager.UpdateFreeCamVcamPosition();
        EditorManager.Instance.EditorCameraManager.ToggleCinemachineBrain(true);
        EditorManager.Instance.MapManager.ToggleMapVisibility();
        EditorManager.Instance.GeoMapManager.ActivateGeoLocalization();
        EditorManager.Instance.ViewManager.ToggleViewPointUI(false);
        CameraManager.Instance.ToggleVcamVisbility(false);
    }

    public override void Exit() {

    }
}
