using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorInitializer : MonoBehaviour, Iinitializer {

    EditorProjectSerializer _projectDeserializer;

    void Awake() {
        _projectDeserializer = FindAnyObjectByType<EditorProjectSerializer>();    
    }

    public void Initialize() {
        SceneLoadingManager.Instance.SetActiveScene(SceneType.Editing);

        // Deserialize the selected project
        _projectDeserializer.DeserializeProject(ProjectManager.Instance.SelectedProject);

        // Show geo map right away if there is no base map model
        if(!EditorManager.Instance.MapManager.IsBaseMapUploaded())
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
