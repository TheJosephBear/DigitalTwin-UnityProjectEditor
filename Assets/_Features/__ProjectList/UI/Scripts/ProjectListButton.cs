using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProjectListButton : MonoBehaviour {
    
    public TMP_Text ProjectNameText;
    ProjectMetadata _projectMetadata;
    ProjectListUI _projectListUI;

    public void Initialize(ProjectMetadata project, ProjectListUI UIScript) {
        _projectMetadata = project;
        SetButtonText(_projectMetadata.ProjectName);
        _projectListUI = UIScript;
    }

    void SetButtonText(string projectName) {
        ProjectNameText.text = projectName;
    }

    public void OnOpenProject() {
        _projectListUI.OnOpenProject(_projectMetadata);
    }

    public void OnCreateNewProject() {
        _projectListUI.OnCreateNewProject();
    }

    public void OnRenameProject() {
        _projectListUI.OnRenameProject(_projectMetadata);
    }

    public void OnDuplicateProject() {
        _projectListUI.OnDuplicateProject(_projectMetadata);
    }

    public void OnExportProject() {
        _projectListUI.OnExportProject(_projectMetadata);
    }

    public void OnShowFeedBack() {
        _projectListUI.OnShowFeedBack(_projectMetadata);
    }

    public void OnDeleteProject() {
        _projectListUI.OnDeleteProject(_projectMetadata);
    }
}
