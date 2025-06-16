using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorInitializer : MonoBehaviour, Iinitializer {
    public void Initialize() {
        // Ask for base model
        // After uploading model succesfully open geomap
        EditorManager.Instance.ChangeEditorMode(EditorMode.Initialization);

    }

    public void StartRunning() {

    }

    public void Unload() {
   //     UImanager.Instance.HideUI(UIType.EditorHUD);
     //   UImanager.Instance.HideUI(UIType.EditorInitUI);
        UImanager.Instance.HideAllUIs();
    }

}
