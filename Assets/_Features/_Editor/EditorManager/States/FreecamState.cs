using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreecamState : StateBase {
    public override void Enter() {
        UIManager.Instance.HideUI(UIType.EditorInitUI);
        if (MainManagerBase.Instance is EditorManager editorMgr) {
            UIManager.Instance.ShowUI(UIType.EditorHUD);
        }

        MainManagerBase.Instance.ViewManager.ToggleViewPointUI(true);
        //      if (TwoCameraInstantiated != null) Destroy(TwoCameraInstantiated);
        MainManagerBase.Instance.EditorCameraManager.DisableCinemachineAfterTransition();
    }

    public override void Exit() {

    }
}
