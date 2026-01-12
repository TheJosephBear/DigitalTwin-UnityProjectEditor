using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreecamState : EditorStateBase {
    public override void Enter() {
        UIManager.Instance.HideUI(UIType.EditorInitUI);
        UIManager.Instance.ShowUI(UIType.EditorHUD);
        //      if (TwoCameraInstantiated != null) Destroy(TwoCameraInstantiated);
        EditorManager.Instance.EditorCameraManager.DisableCinemachineAfterTransition();
    }

    public override void Exit() {

    }
}
