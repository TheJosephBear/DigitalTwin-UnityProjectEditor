using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreecamState : StateBase {
    public override void Enter() {
        SunManager.Instance.ToggleUI(true);
        MainManagerBase.Instance.ToggleHUD(true);
        ViewManager.Instance.ToggleViewPointUI(true);
        CameraManager.Instance.InitializeFreeCamBounds(MapManager.Instance?.GetBaseMap()?.gameObject);
        CameraManager.Instance.ToggleFreecamUpdating(true);
        CameraManager.Instance.ToggleBoundsEnforcing(true);

        if (MainManagerBase.Instance is EditorManager manager) {
            CameraManager.Instance.ToggleVcamVisbility(false);
        }
        //      if (TwoCameraInstantiated != null) Destroy(TwoCameraInstantiated);
        CameraManager.Instance.DisableCinemachineAfterTransition();
    }

    public override void Exit() {
        SunManager.Instance.ToggleUI(false);
        CameraManager.Instance.ToggleFreecamUpdating(false);
        CameraManager.Instance.ToggleBoundsEnforcing(false);
    }
}
