using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorInitializer : MonoBehaviour, Iinitializer {
    public void Initialize() {
        SceneLoadingManager.Instance.SetActiveScene(SceneType.Editing);
        // Show geo map right away
        EditorManager.Instance.ChangeEditorMode(EditorState.GeoLocalization);


    }

    public void StartRunning() {

    }

    public void Unload() {
        UImanager.Instance.HideUI(UIType.EditorHUD);
     //   UImanager.Instance.HideUI(UIType.EditorInitUI);
     //   UImanager.Instance.HideAllUIs();
    }

}
