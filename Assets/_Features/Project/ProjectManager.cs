using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ProjectManager : Singleton<ProjectManager> {

    public Project SelectedProject;

    List<ProjectMetadata> _projectMetadataList = new List<ProjectMetadata>();

    protected override void Awake() {
        base.Awake();
        SelectedProject = GetComponent<Project>();
    }

    #region Project List actions

    public void OpenProject(ProjectMetadata projectMedata) {
        // Download the projectmetadata data into the Selected Project
        DownloadProjectData();
    }

    public void CreateNewProject(Action onCompleted) {
        PopUp.Instance.AskForInput("Jméno projektu", (userInput) => {
            if (string.IsNullOrEmpty(userInput)) {
                PopUp.Instance.ShowPopUpWindow("Input was cancelled or empty.");
                return;
            }

            string newProjectName = UniqueNameEnsure(userInput);

            ServerCommunicationManager.Instance.CreateProject(newProjectName, (success, response) => {
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

    #region Editor actions

    /// <summary>
    /// Download all project data
    /// </summary>
    public void DownloadProjectData() {

    }

    /// <summary>
    /// Save new project data into the database
    /// </summary>
    public void SaveProject() {

    }

    #endregion

    #region Project Metadata

    /// <summary>
    /// Download all of the users projects metadata for project list
    /// </summary>
    public IEnumerator DownloadAllProjectsMetadataCoroutine(
    System.Action<List<ProjectMetadata>> onFinished) {
        bool finished = false;
        bool success = false;
        List<string> projects = null;

        ServerCommunicationManager.Instance.FetchAllProjects((ok, proj) => {
            success = ok;
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

    /*
    public void SaveProject() {
        AssetManager.Instance.UploadModelsToWeb();
        string serializedProject = project.SerializeProject();
        ServerCommunicationManager.Instance.StartUpload(serializedProject, project.ProjectName);
    }
    */

    public async Task<bool> LoadProjectIntoSceneAsync() {
        var tcs = new TaskCompletionSource<bool>();
        ServerCommunicationManager.Instance.StartDataDownload(SelectedProject.ProjectName, async (success, data) => {
            if (data != null) {
                print("the data i got is: " + data);
                bool deserializeSuccess = await SelectedProject.DeserializeProjectAsync(data);
                tcs.SetResult(deserializeSuccess);
            } else {
                tcs.SetResult(false);
            }
        });
        return await tcs.Task;
    }
    /*
    public void OpenProject(ProjectMetadata projectWebReff) {
        SelectedProject.OpenProject(projectWebReff);
        StartCoroutine(ProjectLoading());
    }
    */
    public void CloseProject() {
        StartCoroutine(CloseProjectCoroutine());
    }

    public IEnumerator ProjectLoading() {
        UImanager.Instance.ShowUI(UIType.LoadingScreen);

        var loadTask = LoadProjectIntoSceneAsync();
        yield return new WaitUntil(() => loadTask.IsCompleted);

        if (loadTask.Result) {
            UImanager.Instance.HideUI(UIType.LoadingScreen);
        } else {
         //   PopUp.Instance.ShowPopUpWindow("Failed to load the project.");  // when there is nothing to load, the pop up message is shown regarldess, NOT NEEDED
        }
    }

    IEnumerator CloseProjectCoroutine() {
        UImanager.Instance.ShowUI(UIType.LoadingScreen);
        var loadTask = SceneLoadingManager.Instance.LoadSceneAsync(SceneType.ProjectList);
        while (!loadTask.IsCompleted) {
            yield return null;
        }
        SelectedProject.CloseProject();
        var unloadTask = SceneLoadingManager.Instance.UnLoadSceneAsync(SceneType.Editing);
        while (!unloadTask.IsCompleted) {
            yield return null;
        }
        UImanager.Instance.HideUI(UIType.LoadingScreen);
    }

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
