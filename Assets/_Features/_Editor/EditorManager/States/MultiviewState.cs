using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiviewState : StateBase {
    public override void Enter() {
        print("Entering multi state");
        if (!EditorManager.Instance.MapManager.hasVariant()) {
            MessageDisplayManager.Instance.DisplayMessage("No variants added!");
            return;
        }

        EditorManager.Instance.EditorCameraManager.UpdateFreeCamVcamPosition();
        MainManagerBase.Instance.ToggleHUD(false);
        EditorManager.Instance.EditorCameraManager.ToggleCinemachineBrain(true);
        EditorManager.Instance.MultiViewManager.EnterMultiView();
        EditorManager.Instance.ViewManager.ToggleViewPointUI(false);
    }

    public override void Exit() {

    }
}