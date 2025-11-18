using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiviewState : EditorStateBase {
    public override void Enter() {
        print("Entering multi state");
        if (!EditorManager.Instance.MapManager.hasVariant()) {
            MessageDisplayManager.Instance.DisplayMessage("No variants added!");
            return;
        }

        EditorManager.Instance.EditorCameraManager.UpdateFreeCamVcamPosition();
        UImanager.Instance.HideUI(UIType.EditorHUD);
        EditorManager.Instance.EditorCameraManager.ToggleCinemachineBrain(true);
        EditorManager.Instance.MultiViewManager.EnterMultiView();
    }

    public override void Exit() {

    }
}