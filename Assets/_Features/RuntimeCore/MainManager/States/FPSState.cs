using Cinemachine;
using UnityEngine;

public class FPSState: StateBase {
    public override void Enter() {
        SunManager.Instance.ToggleUI(false);
        MainManagerBase.Instance.ToggleHUD(false);
        ViewManager.Instance.ToggleViewPointUI(false);
        if (MainManagerBase.Instance is EditorManager manager) {
            CameraManager.Instance.ToggleVcamVisbility(false);
        }
        //      if (TwoCameraInstantiated != null) Destroy(TwoCameraInstantiated);
        CameraManager.Instance.ToggleCinemachineBrain(true);
        CinemachineCore.Instance.GetActiveBrain(0).ManualUpdate();
    }

    public override void Exit() {

    }
}
