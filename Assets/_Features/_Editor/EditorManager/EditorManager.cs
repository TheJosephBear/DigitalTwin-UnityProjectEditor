using System;
using System.Collections;

public class EditorManager : MainManagerBase {

    bool _hasUnsavedChanges = false;

    public void ToggleUnsavedChanges(bool unsavedChanges) { 
        _hasUnsavedChanges = unsavedChanges;
    }

    public void SaveProject() {

    //    MessageDisplayManager.Instance.DisplayMessage("SaveProject()");
        ProjectManager.Instance.SaveProject(ProjectSerializer.SerializeProject());
        ToggleUnsavedChanges(false);
    }

    public void ExitEditor(Action<bool> onComplete, bool save = true) {
        if (_hasUnsavedChanges) {
            PopUp.Instance.AreYouSurePopUp((exit) => {
                if (exit) {
                    onComplete.Invoke(true);
                    StartCoroutine(ExitEditorCoroutine(save));
                }
            });
            onComplete.Invoke(false);
        } else {
            onComplete.Invoke(true);
            StartCoroutine(ExitEditorCoroutine(save));
        }
    }

    IEnumerator ExitEditorCoroutine(bool save) {
     //   UIManager.Instance.ShowUI(UIType.LoadingScreen);

        if(save)
            SaveProject();

        ClearManagers();
        SunManager.Instance.ToggleUI(false);

        // Change scenes
        var loadTask = SceneLoadingManager.Instance.LoadSceneAsync(SceneType.ProjectList);
        while (!loadTask.IsCompleted) {
            yield return null;
        }

    //    UIManager.Instance.HideUI(UIType.LoadingScreen);
        var unloadTask = SceneLoadingManager.Instance.UnLoadSceneAsync(SceneType.Editing);
    }

    void ClearManagers() {
        AssetManager.Instance.ClearManager();
        MapManager.ClearEverything();
        ViewManager.ClearEverything();
        ImageManager.Instance.ClearManager();
    }
}

public enum AppState {
    Initialization,
    Freecam,
    GeoLocalization,
    MultiView,
    ViewActive,
    Survey,
    VariantAdjusting,
}
