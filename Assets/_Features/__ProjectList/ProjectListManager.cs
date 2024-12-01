using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class ProjectListManager : Singleton<ProjectListManager> {

    List<ProjectWebRefference> projectRefferenceList = new List<ProjectWebRefference>();

    public void OpenProject(ProjectWebRefference projectWebRefference) {
        ProjectManager.Instance.OpenProject(projectWebRefference);
        StartCoroutine(LoadEditing());
    }

    IEnumerator LoadEditing() {
        UImanager.Instance.ShowUI(UIType.LoadingScreen);
        var loading = SceneLoadingManager.Instance.LoadSceneAsync(SceneType.Editing, 0f);
        while (!loading.IsCompleted) {
            yield return null;
        }
        UImanager.Instance.HideUI(UIType.LoadingScreen);
        UImanager.Instance.HideUI(UIType.ProjectsList);
        var unloadTask = SceneLoadingManager.Instance.UnLoadSceneAsync(SceneType.ProjectList); // No need to wait for this
    }

    public void CreateNewProject() {
        // Popup
        PopUp.Instance.AskForInput("Jméno projektu", (userInput) => {
            if (string.IsNullOrEmpty(userInput)) {
                Debug.Log("Input was cancelled or empty.");
                return;
            }
            // Handle input and create project
            string newProjectName = UniqueNameEnsure(userInput);
            ServerCommunicationManager.Instance.CreateProject(newProjectName, (success, response) => {
                if (!success) {
                    PopUp.Instance.ShowPopUpWindow("Vytvoøení projektu selhalo: " + response);
                }

                Debug.Log("Project created: " + response);
                FetchAndRefreshProjectList(() => {
                    // Simulate opening and saving the project
                    var createdProject = projectRefferenceList.Find(x => x.projectName == newProjectName);
                    if (createdProject == null) {
                        Debug.LogWarning("Created project not found in the refreshed list.");
                    }
                    ProjectManager.Instance.OpenProject(createdProject);
                    ProjectManager.Instance.SaveProject();
                });
            });
        });
    }

    public void RenameProject(ProjectWebRefference projectWebRefference) {
        PopUp.Instance.AskForInput("Pøejmenovat projekt", (userInput) => {
            if (!string.IsNullOrEmpty(userInput)) {
                ServerCommunicationManager.Instance.EditProjectName(projectWebRefference.projectName, userInput, (success, response) => {
                    if (success) {
                        Debug.Log(response);
                        RefreshProjectListFromServer();
                    } else {
                        Debug.LogError("Failed " + response);
                    }
                });
            } else {
                Debug.Log("Input was cancelled or empty.");
            }
        });
    }

    public void DuplicateProject(ProjectWebRefference projectWebRefference) {
        ServerCommunicationManager.Instance.DuplicateProject(projectWebRefference.projectName, (success, response) => {
            if (success) {
                Debug.Log(response);
                RefreshProjectListFromServer();
            } else {
                PopUp.Instance.ShowPopUpWindow("Duplikování projektu selhalo: " + response);
            }
        });
    }

    public void ExportProject(ProjectWebRefference projectWebRefference) {
        ServerCommunicationManager.Instance.GenerateViewerIframe(projectWebRefference.projectName, (success, data) => {
            if (data == null) {
                Debug.LogError("Failed to generate iframe.");
                return;
            }
            print($"in ExportProject response is: {data} {success}");
            PopUp.Instance.ShowCopyableText("Zkopírujte toto do vaší stránky.", data);
        });
    }

    public void ShowFeedBack(ProjectWebRefference projectWebRefference) {
        PopUp.Instance.ShowPopUpWindow("Toto zatím nic nedìlá!");
    }

    public void DeleteProject(ProjectWebRefference projectWebRefference) {
        ServerCommunicationManager.Instance.DeleteProject(projectWebRefference.projectName, (success, response) => {
            if (success) {
                PopUp.Instance.ShowPopUpWindow("Projekt smazán!");
                RefreshProjectListFromServer();
            } else {
                PopUp.Instance.ShowPopUpWindow("Projekt se nepodaøilo smazat! " + response);
            }
        });
    }


    public void RefreshProjectListFromServer() {
        FetchAndRefreshProjectList();
    }

    private void FetchAndRefreshProjectList(Action onComplete = null) {
        ServerCommunicationManager.Instance.FetchAllProjects((success, projects) => {
            if (!success) {
                PopUp.Instance.ShowPopUpWindow("Naèítání projektù selhalo.");
            }
            projectRefferenceList.Clear();
            foreach (string project in projects) {
                projectRefferenceList.Add(CreateProjectWebRefference(project));
            }
            FindAnyObjectByType<ProjectListUI>().RefreshProjectList();
            onComplete?.Invoke(); // Call additional actions if provided
        });
    }

    ProjectWebRefference CreateProjectWebRefference(string project) {
        ProjectWebRefference p = new ProjectWebRefference();
        p.projectName = project;
        return p;
    }

    string UniqueNameEnsure(string name) {
        string baseName = name;
        string uniqueName = baseName;
        int copyNumber = 1;

        bool NameExists(string checkName) {
            return projectRefferenceList.Any(wr => wr.projectName == checkName);
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

    public List<ProjectWebRefference> GetProjectReffList() {
        return projectRefferenceList;
    }
}
