using Cinemachine;
using UnityEngine;

public class FPSState: StateBase {
    public override void Enter() {
        SunManager.Instance.ToggleUI(false);
        MainManagerBase.Instance.ToggleHUD(false);
        MainManagerBase.Instance.ViewManager.ToggleViewPointUI(false);
        if (MainManagerBase.Instance is EditorManager manager) {
            CameraManager.Instance.ToggleVcamVisbility(false);
        }
        //      if (TwoCameraInstantiated != null) Destroy(TwoCameraInstantiated);
        MainManagerBase.Instance.EditorCameraManager.ToggleCinemachineBrain(true);
        CinemachineCore.Instance.GetActiveBrain(0).ManualUpdate();
    }

    public override void Exit() {

    }
}
