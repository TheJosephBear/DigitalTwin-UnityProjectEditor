using UnityEngine;

public class PinPlacingState : StateBase {
    public override void Enter() {
        // PinPlacingManager hides the pin UIs itself, so placing also works without a main manager
        MainManagerBase.Instance.ToggleHUD(false);
        SunManager.Instance.ToggleUI(false);
        ViewManager.Instance.ToggleViewPointUI(false);
        ViewManager.Instance.ToggleCameraPreview(false);

        // Keep the camera movable so the pin can be placed anywhere on the map
        CameraManager.Instance.ToggleFreecamUpdating(true);
        CameraManager.Instance.ToggleBoundsEnforcing(true);
    }

    public override void Exit() {
        CameraManager.Instance.ToggleFreecamUpdating(false);
        CameraManager.Instance.ToggleBoundsEnforcing(false);
    }
}
