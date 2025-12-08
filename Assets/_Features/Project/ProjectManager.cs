using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

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
        AssetManager.Instance.UploadModelsToWeb(); // WHY THE FUCK IS ASSET MANAGER UPLOADING IT??? IT IS PART OF THE PROJECT!!!
        string serializedProject = JsonUtility.ToJson(serializableProject);
        ServerCommunicationManager.Instance.StartUpload(serializedProject, serializableProject.ProjectName);
    }

    /// <summary>
    /// Download selected projects whole data
    /// </summary>
    /// 
    public IEnumerator DownloadSelectedProjectData(ProjectMetadata projectMetadata, System.Action<string> onFinished) {
        if (SelectedProject == null) yield break;

        bool finished = false;
        bool success = false;
        string downloadedData = "";

        ServerCommunicationManager.Instance.StartDataDownload(projectMetadata.ProjectName, async (successful, data) => {
            //       bool deserializeSuccess = await SelectedProject.DeserializeProjectAsync(data);;
            success = false;
            if (data != null) {
                downloadedData = data;
                success = successful;
                finished = true;

                SelectedProject = new Project();
                SelectedProject.CreateSerializedProjectFromJson(data);
            }
        });

        yield return new WaitUntil(() => finished);

        if (!success) {
            PopUp.Instance.ShowPopUpWindow("Naèítání projektù selhalo.");
            onFinished(null);
            yield break;
        }

        onFinished(downloadedData);
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
                    PopUp.Instance.ShowPopUpWindow("Vytvoøení projektu selhalo: " + response);
                }

                onCompleted?.Invoke();
            });
        });
    }

    public void RenameProject(ProjectMetadata projectMetadata, Action onCompleted) {
        PopUp.Instance.AskForInput("Pøejmenovat projekt", (userInput) => {
            if (string.IsNullOrEmpty(userInput)) {
                PopUp.Instance.ShowPopUpWindow("Input was cancelled or empty.");
                return;
            }

            ServerCommunicationManager.Instance.EditProjectName(projectMetadata.ProjectName, userInput, (success, response) => {
                if (!success) {
                    PopUp.Instance.ShowPopUpWindow("Failed " + response);
                }

                onCompleted?.Invoke();
            });
        });
    }

    public void DuplicateProject(ProjectMetadata projectMetadata, Action onCompleted) {
        ServerCommunicationManager.Instance.DuplicateProject(projectMetadata.ProjectName, (success, response) => {
            if (!success) {
                PopUp.Instance.ShowPopUpWindow("Duplikování projektu selhalo: " + response);
            }

            onCompleted?.Invoke();
        });
    }

    public void DeleteProject(ProjectMetadata projectMetadata, Action onCompleted) {
        ServerCommunicationManager.Instance.DeleteProject(projectMetadata.ProjectName, (success, response) => {
            if (!success) {
                PopUp.Instance.ShowPopUpWindow("Projekt se nepodaøilo smazat! " + response);
            } else {
                PopUp.Instance.ShowPopUpWindow("Projekt smazán!");
            }

            onCompleted?.Invoke();
        });
    }

    public void GetProjectSurveyResponseData(ProjectMetadata projectMedata) {
        PopUp.Instance.ShowPopUpWindow("Toto zatím nic nedìlá!");
    }

    public void GetProjectIframeExport(ProjectMetadata projectMedata) {
        ServerCommunicationManager.Instance.GenerateViewerIframe(projectMedata.ProjectName, (success, data) => {
            if (data == null) {
                PopUp.Instance.ShowPopUpWindow("Failed to generate iframe.");
                return;
            }

            PopUp.Instance.ShowCopyableText("Zkopírujte toto do vaší stránky.", data);
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
        List<string> projects = null;

        ServerCommunicationManager.Instance.FetchAllProjects((successful, proj) => {
            success = successful;
            projects = proj;
            finished = true;
        });

        yield return new WaitUntil(() => finished);

        if (!success) {
            PopUp.Instance.ShowPopUpWindow("Naèítání projektù selhalo.");
            onFinished(null);
            yield break;
        }

        _projectMetadataList.Clear();



        foreach (string project in projects) {
            _projectMetadataList.Add(CreateProjectMetadataClass(project));
        }

        onFinished(_projectMetadataList);
    }

    ProjectMetadata CreateProjectMetadataClass(string project) {
        ProjectMetadata p = new ProjectMetadata();
        p.ProjectName = project;
        return p;
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
            return _projectMetadataList.Any(wr => wr.ProjectName == checkName);
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
