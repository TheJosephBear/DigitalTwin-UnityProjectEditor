using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class ViewactiveState : StateBase {

    bool _isCollisionEnabled = false;

    public override void Enter() {
        EditorManager.Instance.EditorCameraManager.UpdateFreeCamVcamPosition();
        EditorManager.Instance.EditorCameraManager.ToggleCinemachineBrain(true);
        CinemachineCore.Instance.GetActiveBrain(0).ManualUpdate();
        if(MainManagerBase.Instance is EditorManager em) {
            _isCollisionEnabled = CameraManager.Instance.IsCollisionEnabled;
            CameraManager.Instance.ToggleCameraCollision(false);
            em.ViewManager.StartViewMoving();
        } else {
            MainManagerBase.Instance.ViewManager.ActivateViewPoint();
        }
      //  MainManagerBase.Instance.ToggleHUD(false);
    }

    public override void Exit() {
        CameraManager.Instance.ToggleCameraCollision(_isCollisionEnabled);
        MainManagerBase.Instance.ToggleHUD(true);
    }
}
