using UnityEngine;

public class VariantAdjustingState : StateBase {
    public override void Enter() {
        if (!MapManager.Instance.HasVariant()) {
            MessageDisplayManager.Instance.DisplayMessage("No variants added!");
            return;
        }

        MainManagerBase.Instance.ToggleHUD(false);
        CameraManager.Instance.ToggleCinemachineBrain(false);
        ViewManager.Instance.ToggleViewPointUI(false);
    }

    public override void Exit() {

    }
}
