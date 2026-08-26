using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Text.RegularExpressions;

public class ProjectListManager : Singleton<ProjectListManager> {

    public GameObject ProjectExportUIReff;
    ProjectListUINew _ui;

    void Awake() {
        base.Awake();

        _ui = FindAnyObjectByType<ProjectListUINew>();
    }

    public void RefreshProjectList() {
        _ui.RefreshProjectList(() => { });
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
        StartCoroutine(ProjectManager.Instance.DownloadProjectData(projectMetadata, (list, success) => {
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

      //  UIManager.Instance.HideUI(UIType.LoadingScreen);
        UIManager.Instance.HideUI(UIType.ProjectsList);
        SceneLoadingManager.Instance.UnLoadSceneAsync(SceneType.ProjectList);
    }

    public void CreateNewProject() {
        ProjectManager.Instance.CreateNewProject((id) => {
            GetProjectMetadataList((list) => {
                ProjectMetadata createdProject = ProjectManager.Instance.GetProjectMetadataByID(id);
                if(createdProject != null) {
                    OpenProject(createdProject);
                    return;
                }

                RefreshProjectList();
            });
            
        });
    }

    public void RenameProject(ProjectMetadata projectMetadata) {
        ProjectManager.Instance.RenameProject(projectMetadata, () => {
            RefreshProjectList();
        });
    }

    public void EditProject(string oldName, string newName, string description, string imageID, System.Action onCompleted) {
        ProjectManager.Instance.EditProject(oldName, newName, description, imageID, () => {
            onCompleted();
        });
    }

    public void RenameProject(ProjectMetadata projectMetadata, string text) {
        ProjectManager.Instance.RenameProject(projectMetadata, text, () => {
            _ui.RefreshProjectList(() => { });
        });
    }

    public void DuplicateProject(ProjectMetadata projectMetadata) {
        ProjectManager.Instance.DuplicateProject(projectMetadata, () => {
            RefreshProjectList();
        });
    }

    public void DeleteProject(ProjectMetadata projectMetadata) {
        ProjectManager.Instance.DeleteProject(projectMetadata, () => {
            RefreshProjectList();
        });
    }

    public void ExportProject(ProjectMetadata projectMetadata) {
        ProjectManager.Instance.GetProjectIframeExport(projectMetadata, (iframeString) =>
        {
            if (string.IsNullOrEmpty(iframeString))
                return;

            ProjectExportUI exportUI =
                SceneLoadingManager.Instance
                .InstantiateObjectInScene(ProjectExportUIReff)
                .GetComponent<ProjectExportUI>();

            string url = GetUrlFromIframe(iframeString);

            print($"trying to fill {exportUI.name} with {iframeString} and {url}");

            exportUI.FillTextFields(iframeString, url);
        });
    }

    public void DownloadSurveyResponses(ProjectMetadata projectMetadata) {
        ProjectManager.Instance.DownloadSurveyResponses(projectMetadata);
    }

    public void ShowFeedBack(ProjectMetadata projectMetadata) {
        DownloadSurveyResponses(projectMetadata);
    }

    #endregion

    string GetUrlFromIframe(string iframe) {
        if (string.IsNullOrEmpty(iframe))
            return null;

        Match match = Regex.Match(iframe, "src\\s*=\\s*\"([^\"]+)\"");

        if (match.Success)
            return match.Groups[1].Value;

        Debug.LogError("Failed to extract URL from iframe.");
        return null;
    }

}
