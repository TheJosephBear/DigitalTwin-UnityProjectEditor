using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ProjectListUI : UIBehaviour {

    public GameObject projectButtonPrefab;
    public GameObject projectScrollViewContent; 

    private List<ProjectListButton> projectButtons = new List<ProjectListButton>();


    public override void Show() {
        base.Show();
        Initialize();
        UImanager.Instance.SetRaycasterFromLatestUI();
    }

    public void Initialize() {
        ProjectListManager.Instance.RefreshProjectListFromServer();
    }

    public void CreateNewProject() {
        ProjectListManager.Instance.CreateNewProject();
    }

    public void RefreshProjectList() {
        foreach (var button in projectButtons) {
            Destroy(button.gameObject);
        }
        projectButtons.Clear();

        List<ProjectWebRefference> projectList = ProjectListManager.Instance.GetProjectReffList();
        foreach (ProjectWebRefference project in projectList) {
            ProjectListButton button = AddProjectButtonToList();
            button.Initialize(project);
        }
    }

    // Create a button for each project and add it to the scroll view
    ProjectListButton AddProjectButtonToList() {
        GameObject projectButtonGO = Instantiate(projectButtonPrefab, projectScrollViewContent.transform);
        ProjectListButton projectButtonScript = projectButtonGO.GetComponent<ProjectListButton>();
        projectButtons.Add(projectButtonScript);
        return projectButtonScript;
    }
}
