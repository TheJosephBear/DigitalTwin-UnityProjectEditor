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

    public void ExitEditor(bool save = true) {
        StartCoroutine(ExitEditorCoroutine(save));
    }

    IEnumerator ExitEditorCoroutine(bool save) {
        UIManager.Instance.ShowUI(UIType.LoadingScreen);

        if(save)
            SaveProject();

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

public enum AppState {
    Initialization,
    Freecam,
    GeoLocalization,
    MultiView,
    ViewActive,
    Survey,
}