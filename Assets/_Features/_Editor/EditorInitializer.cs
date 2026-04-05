using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorInitializer : MonoBehaviour, Iinitializer {

    EditorProjectSerializer _projectDeserializer;

    void Awake() {
        _projectDeserializer = FindAnyObjectByType<EditorProjectSerializer>();    
    }

    public void Initialize() {
        StartCoroutine(InitializeCoroutine());
    }

    public void StartRunning() {

    }

    public void Unload() {
        UIManager.Instance.HideUI(UIType.EditorHUD);
     //   UIManager.Instance.HideUI(UIType.EditorInitUI);
     //   UIManager.Instance.HideAllUIs();
    }

    public IEnumerator InitializeCoroutine() {
        SceneLoadingManager.Instance.SetActiveScene(SceneType.Editing);

        // Wait for project deserialization
        yield return StartCoroutine(_projectDeserializer.DeserializeProjectCoroutinable(
            ProjectManager.Instance.SelectedProject));

        if (!EditorManager.Instance.MapManager.IsBaseMapUploaded()) {
            EditorManager.Instance.ChangeState(AppState.GeoLocalization);
        } else {
            EditorManager.Instance.ChangeState(AppState.Freecam);
        }
    }
}
