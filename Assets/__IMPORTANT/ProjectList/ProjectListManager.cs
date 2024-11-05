using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ProjectListManager : Singleton<ProjectListManager> {

    List<ProjectWebRefference> projectRefferenceList = new List<ProjectWebRefference>();

    public void OpenProject(ProjectWebRefference projectWebRefference) {
        ProjectManager.Instance.OpenProject(projectWebRefference);
        StartCoroutine(LoadEditing());
    }

    IEnumerator LoadEditing() {
        var loading = SceneLoadingManager.Instance.LoadSceneAsync(SceneType.Editing, 0f);
        while (!loading.IsCompleted) {
            yield return null;
        }
        UImanager.Instance.HideUI(UIType.Projects);
    }

    public void CreateNewProject() {
        // Popup
        PopUpTextInput.Instance.AskForInput("Jméno projektu", (userInput) => {
            if (!string.IsNullOrEmpty(userInput)) {
                // Handle input and create project
                string newProjectName = UniqueNameEnsure(userInput);
                WebCommunicationManager.Instance.CreateProject(newProjectName, (success, response) => {
                    if (success) {
                        Debug.Log("Project created: " + response);
                        // This is refreshProjectList logic, i didnt want to add the async because of one time need for it
                        WebCommunicationManager.Instance.FetchAllProjects((projects) => {
                            if (projects != null) {
                                projectRefferenceList.Clear();
                                foreach (string project in projects) {
                                    projectRefferenceList.Add(CreateProjectWebRefference(project));
                                }
                                FindAnyObjectByType<ProjectsUI>().RefreshProjectList();
                                // Simulate opening and saving the project
                                ProjectManager.Instance.OpenProject(projectRefferenceList.Find(x => x.projectName == newProjectName));
                                ProjectManager.Instance.SaveProject();
                            } else {
                                PopUp.Instance.ShowPopUpWindow("Naèítání projektù selhalo.");
                            }
                        });
                    } else {
                        PopUp.Instance.ShowPopUpWindow("Vytvoøení projektu selhalo: " + response);
                    }
                });
            } else {
                Debug.Log("Input was cancelled or empty.");
            }
        });
    }

    public void RenameProject(ProjectWebRefference projectWebRefference) {
        PopUpTextInput.Instance.AskForInput("Pøejmenovat projekt", (userInput) => {
            if (!string.IsNullOrEmpty(userInput)) {
                WebCommunicationManager.Instance.EditProjectName(projectWebRefference.projectName, userInput, (success, response) => {
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
        WebCommunicationManager.Instance.DuplicateProject(projectWebRefference.projectName, (success, response) => {
            if (success) {
                Debug.Log(response);
                RefreshProjectListFromServer();
            } else {
                PopUp.Instance.ShowPopUpWindow("Duplikování projektu selhalo: " + response);
            }
        });
    }

    public void ExportProject(ProjectWebRefference projectWebRefference) {
        WebCommunicationManager.Instance.GenerateViewerIframe(projectWebRefference.projectName, (fileData) => {
            if (fileData == null) {
                Debug.LogError("Failed to generate iframe.");
                return;
            }
            PopUpTextInput.Instance.ShowCopyableText("Zkopírujte toto do vaší stránky.", fileData);
        });
    }

    public void ShowFeedBack(ProjectWebRefference projectWebRefference) {
        PopUp.Instance.ShowPopUpWindow("Toto zatím nic nedìlá!");
    }

    public void DeleteProject(ProjectWebRefference projectWebRefference) {
        WebCommunicationManager.Instance.DeleteProject(projectWebRefference.projectName, (success, response) => {
            if (success) {
                PopUp.Instance.ShowPopUpWindow("Projekt smazán!");
                RefreshProjectListFromServer();
            } else {
                PopUp.Instance.ShowPopUpWindow("Projekt se nepodaøilo smazat! " + response);
            }
        });
    }

    public void RefreshProjectListFromServer() {
        WebCommunicationManager.Instance.FetchAllProjects((projects) => {
            if (projects != null) {
                projectRefferenceList.Clear();
                foreach (string project in projects) {
                    projectRefferenceList.Add(CreateProjectWebRefference(project));
                }
                FindAnyObjectByType<ProjectsUI>().RefreshProjectList();
            } else {
                PopUp.Instance.ShowPopUpWindow("Naèítání projektù selhalo.");
            }
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
