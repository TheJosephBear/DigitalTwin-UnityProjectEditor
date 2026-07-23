using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreecamState : StateBase {
    public override void Enter() {
        UIManager.Instance.HideUI(UIType.EditorInitUI);
        MainManagerBase.Instance.ToggleHUD(true);
        MainManagerBase.Instance.ViewManager.ToggleViewPointUI(true);
        CameraManager.Instance.InitializeFreeCamBounds();
        //      if (TwoCameraInstantiated != null) Destroy(TwoCameraInstantiated);
        MainManagerBase.Instance.EditorCameraManager.DisableCinemachineAfterTransition();
    }

    public override void Exit() {

    }
}
