using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class ProjectListManager : Singleton<ProjectListManager> {

    ProjectListUI _ui;

    void Awake() {
        base.Awake();

        _ui = FindAnyObjectByType<ProjectListUI>();
    }

    public void RefreshProjectList() {
        _ui.RefreshProjectList();
    }

    public void GetProjectMetadataList(System.Action<List<ProjectMetadata>> onFinished) {
        StartCoroutine(ProjectManager.Instance.DownloadAllProjectsMetadataCoroutine((list) => {
            onFinished(list);
        }));
    }

    #region Context menu actions


    public void OpenProject(ProjectMetadata projectMedata) {
        print("open project started");
        // Download selected project data
        // Editor then deserializes it once the scene is loaded
        StartCoroutine(OpenProjectCoroutine(projectMedata));
    }

    IEnumerator OpenProjectCoroutine(ProjectMetadata projectMetadata) {
        bool downloadFinished = false;

        UIManager.Instance.ShowUI(UIType.LoadingScreen);
        print("started project download");
        StartCoroutine(ProjectManager.Instance.DownloadProjectData(projectMetadata, (list) => {
            downloadFinished = true;
        }));

        while (!downloadFinished)
            yield return null;

        print(" project downloaded");
        print(" loading editor");
        var loading = SceneLoadingManager.Instance.LoadSceneAsync(SceneType.Editing, 0f);
        while (!loading.IsCompleted)
            yield return null;

        print("editor loaded");

        UIManager.Instance.HideUI(UIType.LoadingScreen);
        UIManager.Instance.HideUI(UIType.ProjectsList);
        SceneLoadingManager.Instance.UnLoadSceneAsync(SceneType.ProjectList);
    }

    public void CreateNewProject() {
        ProjectManager.Instance.CreateNewProject(() => {
            _ui.RefreshProjectList(); // Only refresh here
        });
    }

    public void RenameProject(ProjectMetadata projectMetadata) {
        ProjectManager.Instance.RenameProject(projectMetadata, () => {
            _ui.RefreshProjectList();
        });
    }

    public void DuplicateProject(ProjectMetadata projectMetadata) {
        ProjectManager.Instance.DuplicateProject(projectMetadata, () => {
            _ui.RefreshProjectList();
        });
    }

    public void DeleteProject(ProjectMetadata projectMetadata) {
        ProjectManager.Instance.DeleteProject(projectMetadata, () => {
            _ui.RefreshProjectList();
        });
    }

    public void ExportProject(ProjectMetadata projectMetadata) {
        ProjectManager.Instance.GetProjectIframeExport(projectMetadata);
    }

    public void ShowFeedBack(ProjectMetadata projectMetadata) {
        ProjectManager.Instance.GetProjectSurveyResponseData(projectMetadata);
    }

    #endregion

}
