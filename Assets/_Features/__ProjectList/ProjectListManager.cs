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

    IEnumerator LoadEditing() {
        UImanager.Instance.ShowUI(UIType.LoadingScreen);
        var loading = SceneLoadingManager.Instance.LoadSceneAsync(SceneType.Editing, 0f);
        while (!loading.IsCompleted) {
            yield return null;
        }
        // Open the project (download project data) after the editor scene has been loaded
        // ProjectManager.Instance.OpenProject(projectWebRefference); // NOOOOOO

        UImanager.Instance.HideUI(UIType.LoadingScreen);
        UImanager.Instance.HideUI(UIType.ProjectsList);
        SceneLoadingManager.Instance.UnLoadSceneAsync(SceneType.ProjectList);
    }

    #region Context menu actions

    public void OpenProject(ProjectMetadata projectMedata) {
        ProjectManager.Instance.OpenProject(projectMedata);
        // Do the opening in project manager
        // That sets all the data we need, using it is editors job
        StartCoroutine(LoadEditing());
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
