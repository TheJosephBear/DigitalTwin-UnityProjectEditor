using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiviewState : StateBase {
    public override void Enter() {
        print("Entering multi state");
        if (!MapManager.Instance.HasVariant()) {
            MessageDisplayManager.Instance.DisplayMessage("No variants added!");
            return;
        }

        CameraManager.Instance.UpdateFreeCamVcamPosition();
        MainManagerBase.Instance.ToggleHUD(false);
        CameraManager.Instance.ToggleCinemachineBrain(true);
        MultiViewManager.Instance.EnterMultiView();
        ViewManager.Instance.ToggleViewPointUI(false);
    }

    public override void Exit() {

    }
}
