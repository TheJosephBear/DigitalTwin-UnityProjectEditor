using System;
using System.Collections;
using UnityEngine;

public class EditorManager : MainManagerBase {

    bool _hasUnsavedChanges = false;

    public void ToggleUnsavedChanges(bool unsavedChanges) { 
        _hasUnsavedChanges = unsavedChanges;
    }

    public void SaveProject() {

    //    MessageDisplayManager.Instance.DisplayMessage("SaveProject()");
        ProjectManager.Instance.SaveProject(ProjectSerializer.SerializeProject(), (success) => {
            if (success) return;
            // Not on the server, ask again on exit
            ToggleUnsavedChanges(true);
            if (MessageDisplayManager.Instance != null) MessageDisplayManager.Instance.ShowMessage("Uložení projektu se nezdařilo.");
        });
        ToggleUnsavedChanges(false);
    }

    public void ExitEditor(Action<bool> onComplete) {
        if (_hasUnsavedChanges) {
            PopUp.Instance.SaveBeforeExitPopUp((choice) => {
                if (choice == UnsavedChangesChoice.Cancel) return;
                onComplete.Invoke(true);
                StartCoroutine(ExitEditorCoroutine(choice == UnsavedChangesChoice.SaveAndExit));
            });
            onComplete.Invoke(false);
        } else {
            onComplete.Invoke(true);
            StartCoroutine(ExitEditorCoroutine(false));
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
        MapManager.Instance.ClearEverything();
        ViewManager.Instance.ClearEverything();
        if (PinManager.Instance != null) PinManager.Instance.ClearEverything();
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
    FPS,
    PinPlacing
}
