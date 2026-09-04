using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;

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
        AssetManager.Instance.UploadModelsToWeb(SelectedProject.ProjectName);
        ImageManager.Instance.UploadImagesToServer(SelectedProject.ProjectName);
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
            PopUp.Instance.ShowPopUpWindow("Načítání projektu selhalo.");
            onFinished?.Invoke(null, false);
            yield break;
        }

        onFinished?.Invoke(downloadedData, true);
    }


    #region Project List actions

    public void CreateNewProject(Action<string> onCompleted) {
        PopUp.Instance.AskForInput("Jméno projektu", (userInput) => {
            if (string.IsNullOrWhiteSpace(userInput)) {
                PopUp.Instance.ShowPopUpWindow("Input was cancelled or empty.");
                return;
            }

            string trimmedInput = userInput.Trim();
            string newProjectName = UniqueNameEnsure(trimmedInput);
            string id = Guid.NewGuid().ToString();

            ServerCommunicationManager.Instance.CreateProject(newProjectName, id, (success, response) => {
                if (!success) {
                    PopUp.Instance.ShowPopUpWindow("Vytvoření projektu selhalo: " + response);
                }

                onCompleted?.Invoke(id);
            });
        });
    }

    public void RenameProject(ProjectMetadata projectMetadata, Action onCompleted) {
        PopUp.Instance.AskForInput("Přejmenovat projekt", (userInput) => {
            if (string.IsNullOrWhiteSpace(userInput)) {
                PopUp.Instance.ShowPopUpWindow("Input was cancelled or empty.");
                return;
            }

            string trimmedInput = userInput.Trim();

            ServerCommunicationManager.Instance.EditProjectName(projectMetadata.projectName, trimmedInput, (success, response) => {
                if (!success) {
                    PopUp.Instance.ShowPopUpWindow("Failed " + response);
                }

                onCompleted?.Invoke();
            });
        });
    }

    public void EditProject(string oldName, string newName, string description, string imageID, Action onCompleted) {
        oldName = oldName?.Trim();
        newName = newName?.Trim();
        description = description?.Trim();

        ServerCommunicationManager.Instance.EditProject(oldName, newName, description, imageID, (success, response) => {
            if (!success) {
                PopUp.Instance.ShowPopUpWindow("Failed " + response);
            }

            onCompleted?.Invoke();
        });
    }

    public void RenameProject(ProjectMetadata projectMetadata, string userInput, Action onCompleted) {
        if (string.IsNullOrWhiteSpace(userInput)) {
            PopUp.Instance.ShowPopUpWindow("Input was cancelled or empty.");
            return;
        }

        string trimmedInput = userInput.Trim();

        ServerCommunicationManager.Instance.EditProjectName(projectMetadata.projectName, trimmedInput, (success, response) => {
            if (!success) {
                PopUp.Instance.ShowPopUpWindow("Failed " + response);
            }

            onCompleted?.Invoke();
        });
    }

    public void DuplicateProject(ProjectMetadata projectMetadata, Action onCompleted) {
        ServerCommunicationManager.Instance.DuplicateProject(projectMetadata.projectName, (success, response) => {
            if (!success) {
                MessageDisplayManager.Instance.ShowMessage("Duplikování projektu selhalo: " + response);
            } else {
                MessageDisplayManager.Instance.ShowMessage("Projekt duplikován!");
            }

            onCompleted?.Invoke();
        });
    }

    public void DeleteProject(ProjectMetadata projectMetadata, Action onCompleted) {
        ServerCommunicationManager.Instance.DeleteProject(projectMetadata.projectName, (success, response) => {
            if (!success) {
                MessageDisplayManager.Instance.ShowMessage("Projekt se nepodařilo smazat! " + response);
            } else {
                MessageDisplayManager.Instance.ShowMessage("Projekt smazán!");
            }

            onCompleted?.Invoke();
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

    public void GetProjectSurveyResponseData(ProjectMetadata projectMetadata) {
        DownloadSurveyResponses(projectMetadata);
    }

    public void GetProjectIframeExport(ProjectMetadata projectMetadata, System.Action<string> onFinished) {
        ServerCommunicationManager.Instance.GenerateViewerIframe(
        projectMetadata.projectName,
        (success, data) => {
            if (!success || string.IsNullOrEmpty(data)) {
                PopUp.Instance.ShowPopUpWindow("Failed to generate iframe.");
                onFinished?.Invoke(null);
                return;
            }

            onFinished?.Invoke(data);
        });
    }

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

    string UniqueNameEnsure(string name) {
        string baseName = name?.Trim() ?? string.Empty;
        string uniqueName = baseName;
        int copyNumber = 1;

        bool NameExists(string checkName) {
            return _projectMetadataList.Any(wr => wr.projectName == checkName);
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

    public void GetOpenedProjectExportURL(System.Action<string> onFinished) {
        GetProjectIframeExport(SelectedProject.ProjectName, (iframeString) => {
            if (string.IsNullOrEmpty(iframeString)) {
                onFinished.Invoke(null);
                return;
            }

            onFinished.Invoke(GetUrlFromIframe(iframeString));
        });
    }

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
