using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class EditorManager : MainManagerBase {

    public void SaveProject() {
        ProjectManager.Instance.SaveProject(ProjectSerializer.SerializeProject());
    }

    public void ExitEditor() {
        StartCoroutine(ExitEditorCoroutine());
    }

    IEnumerator ExitEditorCoroutine() {
        UIManager.Instance.ShowUI(UIType.LoadingScreen);

        // Save project
        SaveProject();

        // Clear managers
        ClearManagers();

        // Change scenes
        var loadTask = SceneLoadingManager.Instance.LoadSceneAsync(SceneType.ProjectList);
        while (!loadTask.IsCompleted) {
            yield return null;
        }

        UIManager.Instance.HideUI(UIType.LoadingScreen);
        var unloadTask = SceneLoadingManager.Instance.UnLoadSceneAsync(SceneType.Editing);
    }

    void ClearManagers() {
        AssetManager.Instance.ClearManager();
        MapManager.ClearEverything();
        ViewManager.ClearEverything();
    }
}

public enum ProjectState {
    Initialization,
    Freecam,
    GeoLocalization,
    MultiView,
    ViewActive,
    Survey,
}