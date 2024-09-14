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
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void SetButtonText(string projectName) {
        projectNameText.text = projectName;
    }

    public void OnClick() {
        ProjectListManager.Instance.SelectProject(projectWebRefference);
    }
}
