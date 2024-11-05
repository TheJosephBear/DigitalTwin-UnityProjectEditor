using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProjectListButton : MonoBehaviour {
    
    public TMP_Text projectNameText;
    ProjectWebRefference projectWebRefference;

    public void Initialize(ProjectWebRefference project) {
        projectWebRefference = project;
        SetButtonText(projectWebRefference.projectName);
   //     GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void SetButtonText(string projectName) {
        projectNameText.text = projectName;
    }

    public void OnOpenProject() {
        ProjectListManager.Instance.OpenProject(projectWebRefference);
    }

    public void OnCreateNewProject() {
        ProjectListManager.Instance.CreateNewProject();
    }

    public void OnRenameProject() {
        ProjectListManager.Instance.RenameProject(projectWebRefference);
    }

    public void OnDuplicateProject() {
        ProjectListManager.Instance.DuplicateProject(projectWebRefference);
    }

    public void OnExportProject() {
        ProjectListManager.Instance.ExportProject(projectWebRefference);
    }

    public void OnShowFeedBack() {
        ProjectListManager.Instance.ShowFeedBack(projectWebRefference);
    }

    public void OnDeleteProject() {
        ProjectListManager.Instance.DeleteProject(projectWebRefference);
    }

    /*
    public void OnClick() {
        ProjectListManager.Instance.SelectProject(projectWebRefference);
    }

    public void onOtevrit() {
        ProjectListManager.Instance.SelectProject(projectWebRefference);
        ProjectListManager.Instance.OpenProject();
    }

    public void onPrejmenovat() {
        ProjectListManager.Instance.SelectProject(projectWebRefference);
        PopUpTextInput.Instance.AskForInput("Nové jméno", (input) => { 
            ProjectListManager.Instance.RenameProject(input);
        });
    }

    public void onDuplikovat() {
        ProjectListManager.Instance.DuplicateProject(projectWebRefference);
    }

    public void onZpetnavVaz() {
        ProjectListManager.Instance.SelectProject(projectWebRefference);

    }

    public void onExport() {
        ProjectListManager.Instance.ExportProject(projectWebRefference);
    }

    public void onOdstranit() {
        ProjectListManager.Instance.SelectProject(projectWebRefference);
        ProjectListManager.Instance.DeleteProject();

    }
    */
}
