using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ProjectsUI : UIBehaviour {

    public GameObject projectButtonPrefab; // Button prefab for displaying each project
    public GameObject projectScrollViewContent; // Content area in the scroll view for project buttons

    // Store the currently displayed buttons
    private List<ProjectListButton> projectButtons = new List<ProjectListButton>();

    public override void Hide() {
        canvas.SetActive(false);
    }

    public override void Show() {
        canvas.SetActive(true);
        Initialize();
    }

    // Initialize and refresh the project list in the UI
    public void Initialize() {
        RefreshProjectList();
    }

    // Refresh the UI to show all projects from the ProjectListManager
    public void RefreshProjectList() {
        // Clear the old buttons
        foreach (var button in projectButtons) {
            Destroy(button.gameObject);
        }
        projectButtons.Clear();

        ProjectListManager.Instance.RefreshProjectListFromServer((success) => {
            if(!success) {
                return;
            }
            List<ProjectWebRefference> projectList = ProjectListManager.Instance.GetProjectRefferenceList();
            foreach (ProjectWebRefference project in projectList) {
                ProjectListButton button = AddProjectButtonToList();
                button.Initialize(project);
            }
        });
    }

    // Create a button for each project and add it to the scroll view
    ProjectListButton AddProjectButtonToList() {
        GameObject projectButtonGO = Instantiate(projectButtonPrefab, projectScrollViewContent.transform);
        ProjectListButton projectButtonScript = projectButtonGO.GetComponent<ProjectListButton>();
        projectButtons.Add(projectButtonScript);
        return projectButtonScript;
    }

    // Call this method when the user initiates the creation of a new project
    public void onNewProject() {
        AudioManager.Instance.PlaySound(SoundType.click);
        PopUpTextInput.Instance.AskForInput("Jméno projektu", (userInput) => {
            if (!string.IsNullOrEmpty(userInput)) {
                ProjectListManager.Instance.CreateNewProject(userInput);
                RefreshProjectList();
            } else {
                Debug.Log("Input was cancelled or empty.");
            }
        });
    }

    public void onRenameProject() {
        AudioManager.Instance.PlaySound(SoundType.click);
        PopUpTextInput.Instance.AskForInput("Pøejmenovat projekt", (userInput) => {
            if (!string.IsNullOrEmpty(userInput)) {
                ProjectListManager.Instance.RenameProject(userInput);
            } else {
                Debug.Log("Input was cancelled or empty.");
            }
        });
    }

    public void onDeleteProject() {
        AudioManager.Instance.PlaySound(SoundType.click);
        ProjectListManager.Instance.DeleteProject();
    }

    public void onFeedback() {
        AudioManager.Instance.PlaySound(SoundType.click);
    }

    public void onEditing() {
        AudioManager.Instance.PlaySound(SoundType.click);
        StartCoroutine(LoadEditing());
    }

    IEnumerator LoadEditing() {
        var loading = SceneLoadingManager.Instance.LoadSceneAsync(SceneType.Editing, 0f);
        while (!loading.IsCompleted) {
            yield return null;
        }
        UImanager.Instance.HideUI(UIType.Projects);
    }
}
