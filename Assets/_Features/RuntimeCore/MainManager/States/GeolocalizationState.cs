using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeolocalizationState : StateBase {
    public override void Enter() {
        UIManager.Instance.HideUI(UIType.EditorInitUI);
        MainManagerBase.Instance.ToggleHUD(false);
        CameraManager.Instance.UpdateFreeCamVcamPosition();
        CameraManager.Instance.ToggleCinemachineBrain(true);
        MapManager.Instance.ToggleMapVisibility();
        GeoMapManager.Instance.ActivateGeoLocalization();
        ViewManager.Instance.ToggleViewPointUI(false);
        CameraManager.Instance.ToggleVcamVisbility(false);
    }

    public override void Exit() {

    }
}
