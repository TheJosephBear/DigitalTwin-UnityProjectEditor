using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// User flows of the project list scene: open, create, rename, edit, duplicate, delete, export, feedback.
/// Shared data and server access stays in ProjectManager.
/// </summary>
public class ProjectListManager: Singleton<ProjectListManager> {

    public GameObject ProjectExportUIReff;
    ProjectListUINew _ui;

    protected override void Awake() {
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
        // Download selected project data
        // Editor then deserializes it once the scene is loaded
        StartCoroutine(OpenProjectCoroutine(projectMedata));
    }

    IEnumerator OpenProjectCoroutine(ProjectMetadata projectMetadata) {
        bool downloadFinished = false;

        UIManager.Instance.ShowUI(UIType.LoadingScreen);
        StartCoroutine(ProjectManager.Instance.DownloadProjectData(projectMetadata, (list, success) => {
            downloadFinished = true;
        }));

        while (!downloadFinished)
            yield return null;

        MemoryLogger.Log($"After downloading project data {projectMetadata.projectName}");
        var loading = SceneLoadingManager.Instance.LoadSceneAsync(SceneType.Editing, 0f);
        while (!loading.IsCompleted)
            yield return null;

        //  UIManager.Instance.HideUI(UIType.LoadingScreen);
        UIManager.Instance.HideUI(UIType.ProjectsList);
        SceneLoadingManager.Instance.UnLoadSceneAsync(SceneType.ProjectList);
    }

    public void CreateNewProject() {
        PopUp.Instance.AskForInput("Jméno projektu", (userInput) => {
            if (string.IsNullOrWhiteSpace(userInput)) {
                PopUp.Instance.ShowPopUpWindow("Input was cancelled or empty.");
                return;
            }

            string newProjectName = UniqueNameEnsure(userInput.Trim());
            string id = System.Guid.NewGuid().ToString();

            ServerCommunicationManager.Instance.CreateProject(newProjectName, id, (success, response) => {
                if (!success) {
                    PopUp.Instance.ShowPopUpWindow("Vytvoření projektu selhalo: " + response);
                }

                OnProjectCreated(id);
            });
        });
    }

    void OnProjectCreated(string id) {
        GetProjectMetadataList((list) => {
            ProjectMetadata createdProject = ProjectManager.Instance.GetProjectMetadataByID(id);
            if (createdProject != null) {
                OpenProject(createdProject);
                return;
            }

            RefreshProjectList();
        });
    }

    public void RenameProject(ProjectMetadata projectMetadata) {
        PopUp.Instance.AskForInput("Přejmenovat projekt", (userInput) => {
            RenameProject(projectMetadata, userInput);
        });
    }

    public void RenameProject(ProjectMetadata projectMetadata, string text) {
        if (string.IsNullOrWhiteSpace(text)) {
            PopUp.Instance.ShowPopUpWindow("Input was cancelled or empty.");
            return;
        }

        ServerCommunicationManager.Instance.EditProjectName(projectMetadata.projectName, text.Trim(), (success, response) => {
            if (!success) {
                PopUp.Instance.ShowPopUpWindow("Failed " + response);
            }

            RefreshProjectList();
        });
    }

    public void EditProject(string oldName, string newName, string description, string imageID, System.Action<bool> onCompleted) {
        oldName = oldName?.Trim();
        newName = newName?.Trim();
        description = description?.Trim();

        ServerCommunicationManager.Instance.EditProject(oldName, newName, description, imageID, (success, response) => {
            if (!success) {
                PopUp.Instance.ShowPopUpWindow("Failed " + response);
            }

            onCompleted?.Invoke(success);
        });
    }

    public void DuplicateProject(ProjectMetadata projectMetadata) {
        ServerCommunicationManager.Instance.DuplicateProject(projectMetadata.projectName, (success, response) => {
            if (!success) {
                MessageDisplayManager.Instance.ShowMessage("Duplikování projektu selhalo: " + response);
            } else {
                MessageDisplayManager.Instance.ShowMessage("Projekt duplikován!");
            }

            RefreshProjectList();
        });
    }

    public void DeleteProject(ProjectMetadata projectMetadata) {
        ServerCommunicationManager.Instance.DeleteProject(projectMetadata.projectName, (success, response) => {
            if (!success) {
                MessageDisplayManager.Instance.ShowMessage("Projekt se nepodařilo smazat! " + response);
            } else {
                MessageDisplayManager.Instance.ShowMessage("Projekt smazán!");
            }

            RefreshProjectList();
        });
    }

    public void ExportProject(ProjectMetadata projectMetadata) {
        ProjectManager.Instance.GetProjectIframeExport(projectMetadata.projectName, (iframeString) => {
            if (string.IsNullOrEmpty(iframeString))
                return;

            ProjectExportUI exportUI =
                SceneLoadingManager.Instance
                .InstantiateObjectInScene(ProjectExportUIReff)
                .GetComponent<ProjectExportUI>();

            string url = ProjectManager.GetUrlFromIframe(iframeString);

            print($"trying to fill {exportUI.name} with {iframeString} and {url}");

            exportUI.FillTextFields(iframeString, url);
        });
    }

    public void DownloadSurveyResponses(ProjectMetadata projectMetadata) {
        if (projectMetadata == null || string.IsNullOrEmpty(projectMetadata.projectName)) {
            PopUp.Instance.ShowPopUpWindow("Projekt není vybrán.");
            return;
        }

        string projectName = projectMetadata.projectName;
        string exportUrl = $"{ServerCommunicationManager.Instance.serverUrl}/export_survey_csv?project_name={UnityEngine.Networking.UnityWebRequest.EscapeURL(projectName)}";

        ServerCommunicationManager.Instance.StartSurveyResponsesDownload(projectName, (success, data) => {
            if (!success) {
                PopUp.Instance.ShowPopUpWindow("K exportu odpovědí tohoto projektu nemáte oprávnění nebo nastala chyba.");
                return;
            }

            if (string.IsNullOrEmpty(data) || data.Trim() == "[]" || data.Trim() == "{}") {
                PopUp.Instance.ShowPopUpWindow("Pro tento projekt zatím nebyly odeslány žádné odpovědi.");
                return;
            }

            if (WebGLTabOpener.Instance != null) {
                WebGLTabOpener.Instance.OpenLinkInNewTab(exportUrl);
            } else {
                Application.OpenURL(exportUrl);
            }
        });
    }

    public void ShowFeedBack(ProjectMetadata projectMetadata) {
        DownloadSurveyResponses(projectMetadata);
    }

    #endregion

    /// <summary>
    /// Appends " (n)" to the name until no existing project uses it.
    /// </summary>
    string UniqueNameEnsure(string name) {
        List<ProjectMetadata> existingProjects = ProjectManager.Instance.GetProjectMetadataList();

        string baseName = name?.Trim() ?? string.Empty;
        string uniqueName = baseName;
        int copyNumber = 1;

        bool NameExists(string checkName) {
            return existingProjects.Any(wr => wr.projectName == checkName);
        }

        if (!NameExists(uniqueName)) {
            return uniqueName;
        }

        while (NameExists(uniqueName)) {
            int lastIndexOfOpenParenthesis = baseName.LastIndexOf('(');
            int lastIndexOfCloseParenthesis = baseName.LastIndexOf(')');
            if (lastIndexOfOpenParenthesis != -1 && lastIndexOfCloseParenthesis == baseName.Length - 1) {
                string suffix = baseName.Substring(lastIndexOfOpenParenthesis + 1, lastIndexOfCloseParenthesis - lastIndexOfOpenParenthesis - 1);
                if (int.TryParse(suffix, out int existingNumber)) {
                    copyNumber = existingNumber + 1;
                    baseName = baseName.Substring(0, lastIndexOfOpenParenthesis).Trim();
                }
            }
            uniqueName = $"{baseName} ({copyNumber})";
            copyNumber++;
        }
        return uniqueName;
    }

}
