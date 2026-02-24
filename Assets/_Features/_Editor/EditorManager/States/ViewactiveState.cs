using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class ViewactiveState : StateBase {
    public override void Enter() {
        EditorManager.Instance.EditorCameraManager.UpdateFreeCamVcamPosition();
        EditorManager.Instance.EditorCameraManager.ToggleCinemachineBrain(true);
        CinemachineCore.Instance.GetActiveBrain(0).ManualUpdate();
        EditorManager.Instance.ViewManager.StartViewMoving();
    }

    public override void Exit() {
        EditorManager.Instance.ViewManager.ExitViewMoving();

    }
}