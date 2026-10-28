using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class ViewactiveState : StateBase {

    bool _isCollisionEnabled = false;

    public override void Enter() {
        CameraManager.Instance.UpdateFreeCamVcamPosition();
        CameraManager.Instance.ToggleCinemachineBrain(true);
        CinemachineCore.Instance.GetActiveBrain(0).ManualUpdate();
        if(MainManagerBase.Instance is EditorManager) {
            _isCollisionEnabled = CameraManager.Instance.IsCollisionEnabled;
            CameraManager.Instance.ToggleCameraCollision(false);
            ViewManager.Instance.StartViewMoving();
        } else {
            ViewManager.Instance.ActivateViewPoint();
        }
      //  MainManagerBase.Instance.ToggleHUD(false);
    }

    public override void Exit() {
        CameraManager.Instance.ToggleCameraCollision(_isCollisionEnabled);
        MainManagerBase.Instance.ToggleHUD(true);
    }
}
