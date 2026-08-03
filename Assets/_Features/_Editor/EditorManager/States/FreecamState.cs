using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreecamState : StateBase {
    public override void Enter() {
        UIManager.Instance.HideUI(UIType.EditorInitUI);
        MainManagerBase.Instance.ToggleHUD(true);
        MainManagerBase.Instance.ViewManager.ToggleViewPointUI(true);
        CameraManager.Instance.InitializeFreeCamBounds();
        CameraManager.Instance.ToggleVcamVisbility(true);
        //      if (TwoCameraInstantiated != null) Destroy(TwoCameraInstantiated);
        MainManagerBase.Instance.EditorCameraManager.DisableCinemachineAfterTransition();
    }

    public override void Exit() {

    }
}
