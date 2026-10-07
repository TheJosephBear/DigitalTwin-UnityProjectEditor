using UnityEngine;

public class VariantAdjustingState : StateBase {
    public override void Enter() {
        if (!EditorManager.Instance.MapManager.HasVariant()) {
            MessageDisplayManager.Instance.DisplayMessage("No variants added!");
            return;
        }

        MainManagerBase.Instance.ToggleHUD(false);
        EditorManager.Instance.EditorCameraManager.ToggleCinemachineBrain(false);
        EditorManager.Instance.ViewManager.ToggleViewPointUI(false);
    }

    public override void Exit() {

    }
}
