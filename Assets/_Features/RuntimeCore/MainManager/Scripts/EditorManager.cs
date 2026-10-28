using Cinemachine;
using System;
using System.Collections;
using System.Linq;
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
        UpdateProjectPreview();
    }

    // Proportions of the preview box in the project list (ImageContainer in ProjectListItem.prefab, 163.49 x 104.91),
    // the image fills it without being cropped
    const float PreviewAspect = 163.49f / 104.91f;
    const int PreviewWidth = 1280;
    static readonly int PreviewHeight = Mathf.RoundToInt(PreviewWidth / PreviewAspect);

    // Project list preview = render of the first view, or of the default (starting) view when there are none.
    // Rendered right away, the views are cleared when saving on exit. A preview uploaded by the user is kept.
    void UpdateProjectPreview() {
        RenderTexture rt;
        ViewPoint firstView = ViewManager.Instance != null ? ViewManager.Instance.GetViewPoints().FirstOrDefault() : null;
        if (firstView != null) {
            rt = ViewRenderer.Render(firstView, PreviewWidth, PreviewHeight);
        } else if (CameraManager.Instance != null && CameraManager.Instance.InitialCameraPosition != null) {
            Transform defaultView = CameraManager.Instance.InitialCameraPosition;
            var freeCam = CameraManager.Instance.GetFreeCamVcam()?.GetComponent<CinemachineVirtualCamera>();
            rt = ViewRenderer.Render(defaultView.position, defaultView.rotation, freeCam != null ? freeCam.m_Lens.FieldOfView : -1f, PreviewWidth, PreviewHeight);
        } else {
            return;
        }

        Texture2D render = ViewRenderer.ToTexture2D(rt);
        if (render == null) return;

        ImageManager.Instance.UpdateGeneratedPreview(ProjectManager.Instance.SelectedProject.ProjectName, render);
        Destroy(render);
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
