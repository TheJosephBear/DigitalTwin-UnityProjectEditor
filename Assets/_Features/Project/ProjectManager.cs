using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using QuestionnaireToolkit.Scripts.SimpleJSON;

public class ProjectManager : Singleton<ProjectManager> {

    public Project SelectedProject;

    List<ProjectMetadata> _projectMetadataList = new List<ProjectMetadata>();

    protected override void Awake() {
        base.Awake();
        SelectedProject = GetComponent<Project>();
    }

    /// <summary>
    /// Save new project data into the database
    /// </summary>
    public void SaveProject(SerializableProject serializableProject) {
        AssetManager.Instance.UploadModelsToWeb(SelectedProject.ProjectName);
        string serializedProject = JsonUtility.ToJson(serializableProject);
        ServerCommunicationManager.Instance.StartSurveyUpload(serializedProject, serializableProject.projectName);
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
                    SelectedProject.CreateSerializedProjectFromJson(data);
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

    public void CreateNewProject(Action onCompleted) {
        PopUp.Instance.AskForInput("Jméno projektu", (userInput) => {
            if (string.IsNullOrEmpty(userInput)) {
                PopUp.Instance.ShowPopUpWindow("Input was cancelled or empty.");
                return;
            }

            string newProjectName = UniqueNameEnsure(userInput);
            string id = Guid.NewGuid().ToString();

            ServerCommunicationManager.Instance.CreateProject(newProjectName, id, (success, response) => {
                if (!success) {
                    PopUp.Instance.ShowPopUpWindow("Vytvoření projektu selhalo: " + response);
                }

                onCompleted?.Invoke();
            });
        });
    }

    public void RenameProject(ProjectMetadata projectMetadata, Action onCompleted) {
        PopUp.Instance.AskForInput("Přejmenovat projekt", (userInput) => {
            if (string.IsNullOrEmpty(userInput)) {
                PopUp.Instance.ShowPopUpWindow("Input was cancelled or empty.");
                return;
            }

            ServerCommunicationManager.Instance.EditProjectName(projectMetadata.projectName, userInput, (success, response) => {
                if (!success) {
                    PopUp.Instance.ShowPopUpWindow("Failed " + response);
                }

                onCompleted?.Invoke();
            });
        });
    }

    public void DuplicateProject(ProjectMetadata projectMetadata, Action onCompleted) {
        ServerCommunicationManager.Instance.DuplicateProject(projectMetadata.projectName, (success, response) => {
            if (!success) {
                PopUp.Instance.ShowPopUpWindow("Duplikování projektu selhalo: " + response);
            }

            onCompleted?.Invoke();
        });
    }

    public void DeleteProject(ProjectMetadata projectMetadata, Action onCompleted) {
        ServerCommunicationManager.Instance.DeleteProject(projectMetadata.projectName, (success, response) => {
            if (!success) {
                PopUp.Instance.ShowPopUpWindow("Projekt se nepodařilo smazat! " + response);
            } else {
                PopUp.Instance.ShowPopUpWindow("Projekt smazán!");
            }

            onCompleted?.Invoke();
        });
    }

    public void GetProjectSurveyResponseData(ProjectMetadata projectMedata) {
        PopUp.Instance.ShowPopUpWindow("Toto zatím nic nedělá!");
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

    #endregion

    string UniqueNameEnsure(string name) {
        string baseName = name;
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
}
