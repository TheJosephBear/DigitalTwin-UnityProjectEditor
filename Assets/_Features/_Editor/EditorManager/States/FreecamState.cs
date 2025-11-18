using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreecamState : EditorStateBase {
    public override void Enter() {
        UImanager.Instance.HideUI(UIType.EditorInitUI);
        UImanager.Instance.ShowUI(UIType.EditorHUD);
        //      if (TwoCameraInstantiated != null) Destroy(TwoCameraInstantiated);
        EditorManager.Instance.EditorCameraManager.DisableCinemachineAfterTransition();
    }

    public override void Exit() {

    }
}
