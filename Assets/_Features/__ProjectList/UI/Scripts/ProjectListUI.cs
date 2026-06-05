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
        UIManager.Instance.SetRaycasterFromLatestUI();
    }

    public void Initialize() {
        RefreshProjectList();
    }

    public void RefreshProjectList() {
        // Clear old buttons
        foreach (var button in projectButtons) {
            Destroy(button.gameObject);
        }
        projectButtons.Clear();

        // Create updated buttons
        ProjectListManager.Instance.GetProjectMetadataList(list => {
            foreach (ProjectMetadata project in list) {
                AddProjectButtonToList().Initialize(project: project, UIScript: this);
            }
        });
    }

    /// <summary>
    /// Create a button for each project and add it to the scroll view
    /// </summary>
    /// <returns> The project list button class instance </returns>
    ProjectListButton AddProjectButtonToList() {
        GameObject projectButtonGO = Instantiate(projectButtonPrefab, projectScrollViewContent.transform);
        ProjectListButton projectButtonScript = projectButtonGO.GetComponent<ProjectListButton>();
        projectButtons.Add(projectButtonScript);
        return projectButtonScript;
    }

    #region Button onclicks

    public void OnCreateNewProject() {
        ProjectListManager.Instance.CreateNewProject();
    }

    public void OnOpenProject(ProjectMetadata projectMedata) {
        ProjectListManager.Instance.OpenProject(projectMedata);
    }

    public void OnRenameProject(ProjectMetadata projectMedata) {
        ProjectListManager.Instance.RenameProject(projectMedata);
    }

    public void OnDuplicateProject(ProjectMetadata projectMedata) {
        ProjectListManager.Instance.DuplicateProject(projectMedata);
    }

    public void OnExportProject(ProjectMetadata projectMedata) {
        ProjectListManager.Instance.ExportProject(projectMedata);
    }

    public void OnShowFeedBack(ProjectMetadata projectMedata) {
        ProjectListManager.Instance.ShowFeedBack(projectMedata);
    }

    public void OnDeleteProject(ProjectMetadata projectMedata) {
        ProjectListManager.Instance.DeleteProject(projectMedata);
    }

    public void OnLogout() {
        UIManager.Instance.HideUI(UIType.ProjectsList);
        AuthorizationManager.Instance.Logout();
    }

    #endregion


}
