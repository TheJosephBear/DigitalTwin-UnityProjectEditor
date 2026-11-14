using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeolocalizationState : StateBase {
    public override void Enter() {
        MainManagerBase.Instance.ToggleHUD(false);
        CameraManager.Instance.UpdateFreeCamVcamPosition();
        CameraManager.Instance.ToggleCinemachineBrain(true);
        MapManager.Instance.ToggleMapVisibility();
        GeoMapManager.Instance.ActivateGeoLocalization();
        ViewManager.Instance.ToggleViewPointUI(false);
        CameraManager.Instance.ToggleVcamVisbility(false);
        TogglePinUIs(false);
    }

    public override void Exit() {
        TogglePinUIs(true);
    }

    // The pin list and an open pin's context UI (the context UI comes back only when a pin is still being edited)
    static void TogglePinUIs(bool show) {
        PinManager pins = PinManager.Instance;
        if (pins == null) return;
        pins.TogglePinUI(show);
        pins.ToggleContextUI(show && pins.GetActivePin() != null);
    }
}
