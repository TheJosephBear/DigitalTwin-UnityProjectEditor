using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

/// <summary>
/// Global project service: holds the opened project, the cached project metadata
/// and the server calls that are shared between scenes.
/// Project list user flows (create, rename, delete...) live in ProjectListManager.
/// </summary>
public class ProjectManager : Singleton<ProjectManager> {

    public Project SelectedProject;

    List<ProjectMetadata> _projectMetadataList = new List<ProjectMetadata>();

    protected override void Awake() {
        base.Awake();
        SelectedProject = new Project();
    }

    /// <summary>
    /// Save new project data into the database
    /// </summary>
    public void SaveProject(SerializableProject serializableProject) {
    //    ImageManager.Instance.UploadImagesToServer(SelectedProject.ProjectName);
        MemoryLogger.Log("After starting model and image uploads");
        string serializedProject = JsonUtility.ToJson(serializableProject);
        ServerCommunicationManager.Instance.StartProjectDataUpload(serializedProject, serializableProject.projectName);
    //    MessageDisplayManager.Instance.DisplayMessage("Projekt úspěšně uložen");
    }




    /// <summary>
    /// Download selected projects whole data
    /// </summary>
    ///
    public IEnumerator DownloadProjectData(ProjectMetadata projectMetadata, System.Action<string, bool> onFinished) {
        yield return DownloadProjectDataCoroutine(projectMetadata.projectName, onFinished);
    }

    public IEnumerator DownloadProjectData(string projectName, System.Action<string, bool> onFinished) {
        yield return DownloadProjectDataCoroutine(projectName, onFinished);
    }

    private IEnumerator DownloadProjectDataCoroutine(string projectName, System.Action<string, bool> onFinished) {
        bool finished = false;
        bool success = false;
        string downloadedData = "";

        ServerCommunicationManager.Instance.StartDataDownload(
            projectName,
            async (successful, data) => {
                //      print("data is: " + data);

                success = successful && !string.IsNullOrEmpty(data);

                if (success) {
                    downloadedData = data;

                    SelectedProject = new Project();
                    SelectedProject.CreateSerializableProjectFromJson(data);
                    //   MessageDisplayManager.Instance.DisplayMessage("Projekt úspěšně stažen");
                }

                finished = true;
            });

        yield return new WaitUntil(() => finished);

        if (!success) {
        //    PopUp.Instance.ShowPopUpWindow("Načítání projektu selhalo.");
            onFinished?.Invoke(null, false);
            yield break;
        }

        onFinished?.Invoke(downloadedData, true);
    }

    #region Project export

    // Stays here (not in ProjectListManager) because the editor uses it for the opened project,
    // and the project list scene is unloaded while editing.
    public void GetProjectIframeExport(string projectName, System.Action<string> onFinished) {
        ServerCommunicationManager.Instance.GenerateViewerIframe(
        projectName,
        (success, data) => {
            if (!success || string.IsNullOrEmpty(data)) {
                PopUp.Instance.ShowPopUpWindow("Failed to generate iframe.");
                onFinished?.Invoke(null);
                return;
            }

            onFinished?.Invoke(data);
        });
    }

    public void GetOpenedProjectExportURL(System.Action<string> onFinished) {
        GetProjectIframeExport(SelectedProject.ProjectName, (iframeString) => {
            if (string.IsNullOrEmpty(iframeString)) {
                onFinished.Invoke(null);
                return;
            }

            onFinished.Invoke(GetUrlFromIframe(iframeString));
        });
    }

    public static string GetUrlFromIframe(string iframe) {
        if (string.IsNullOrEmpty(iframe))
            return null;

        Match match = Regex.Match(iframe, "src\\s*=\\s*\"([^\"]+)\"");

        if (match.Success)
            return match.Groups[1].Value;

        Debug.LogError("Failed to extract URL from iframe.");
        return null;
    }

    #endregion

    #region Project Metadata

    /// <summary>
    /// Download all of the users projects metadata for project list
    /// </summary>
    public IEnumerator DownloadAllProjectsMetadataCoroutine(System.Action<List<ProjectMetadata>> onFinished) {
        bool finished = false;
        bool success = false;
        ProjectMetadata[] projects = null;

        ServerCommunicationManager.Instance.FetchAllProjects((successful, proj) => {
            success = successful;
            print(proj);
            projects = proj;
            finished = true;
        });

        yield return new WaitUntil(() => finished);

        if (!success) {
            PopUp.Instance.ShowPopUpWindow("Načítání projektů selhalo.");
            onFinished(null);
            yield break;
        }

        _projectMetadataList.Clear();

        foreach (ProjectMetadata project in projects) {
            print(project.projectId);
            print(project.projectName);
            _projectMetadataList.Add(project);
        }

        onFinished(_projectMetadataList);
    }

    public List<ProjectMetadata> GetProjectMetadataList() {
        return _projectMetadataList;
    }

    public ProjectMetadata GetProjectMetadataByID(string ID) {
        return _projectMetadataList.Find(x => x.projectId  == ID);
    }

    #endregion
}
