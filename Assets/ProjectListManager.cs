using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectListManager : Singleton<ProjectListManager> {

    List<ProjectWebRefference> projectRefferenceList = new List<ProjectWebRefference>();
    public ProjectWebRefference selectedProjectRefference;

    public void CreateNewProject(string newProjectName) {
        if (!IsUniqueProjectName(newProjectName)) {
            Debug.LogError("Project name must be unique!");
            return;
        }

        WebCommunicationManager.Instance.CreateProject(newProjectName, (success, response) => {
            if (success) {
                Debug.Log("Project created: " + response);
                //    RefreshProjectListFromServer(null);
                // Simulate opening and saving the project
                RefreshProjectListFromServer((a) => {
                    Project.Instance.OpenProject(projectRefferenceList.Find(x => x.projectName == newProjectName));
                    ProjectSaver.Instance.SaveProject();
                });
            } else {
                Debug.LogError("Failed to create project: " + response);
            }
        });
    }

    public void RefreshProjectListFromServer(System.Action<bool> callback) {
        WebCommunicationManager.Instance.FetchAllProjects((projects) => {
            if (projects != null) {
                projectRefferenceList.Clear(); // Clear the old list
                foreach (string project in projects) {
                    ProjectWebRefference p = new ProjectWebRefference();
                    p.projectName = project;
                    projectRefferenceList.Add(p);
                }
                callback(true);
            } else {
                callback(false);
                Debug.LogError("Failed to fetch project names.");
            }
        });
    }

    public List<ProjectWebRefference> GetProjectRefferenceList() {
        return projectRefferenceList;
    }

    bool IsUniqueProjectName(string projectName) {
        foreach (ProjectWebRefference project in projectRefferenceList) {
            if (project.projectName == projectName) {
                return false; 
            }
        }
        return true; 
    }

    public void OpenProject() {
        StartCoroutine(LoadEditing());
    }

    public void SelectProject(ProjectWebRefference project) {
        selectedProjectRefference = project;
        Debug.Log("Project selected: " + selectedProjectRefference.projectName);
    }

    public void RenameProject(string newName) {
        WebCommunicationManager.Instance.EditProjectName(selectedProjectRefference.projectName, newName, (success, response) => {
            if (success) {
                Debug.Log(response);
                RefreshProjectListFromServer((resp) => { print(resp); });
                FindAnyObjectByType<ProjectsUI>().RefreshProjectList();
            } else {
                Debug.LogError("Failed " + response);
            }
        });
    }

    public void DuplicateProject(ProjectWebRefference project) {
        WebCommunicationManager.Instance.DuplicateProject(project.projectName, (success, response) => {
            if (success) {
                Debug.Log(response);
                RefreshProjectListFromServer((resp) => { print(resp); });
                FindAnyObjectByType<ProjectsUI>().RefreshProjectList();
            } else {
                Debug.LogError("Failed " + response);
            }
        });
    }

    public void DeleteProject() {
        WebCommunicationManager.Instance.DeleteProject(selectedProjectRefference.projectName, (success, response) => {
            if (success) {
                Debug.Log("Project deleted: " + response);
                RefreshProjectListFromServer((resp) => { print(resp); });
                FindAnyObjectByType<ProjectsUI>().RefreshProjectList();
            } else {
                Debug.LogError("Failed to deelte project: " + response);
            }
        });
    }

    IEnumerator LoadEditing() {
        var loading = SceneLoadingManager.Instance.LoadSceneAsync(SceneType.Editing, 0f);
        while (!loading.IsCompleted) {
            yield return null;
        }
        UImanager.Instance.HideUI(UIType.Projects);
    }
}
