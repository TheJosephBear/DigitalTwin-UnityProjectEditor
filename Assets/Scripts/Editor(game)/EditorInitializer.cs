using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorInitializer : MonoBehaviour, Iinitializer
{
    public void Initialize() {
        UImanager.Instance.ShowUI(UIType.EditorHUD);
        ProjectManager.Instance.OpenProject(ProjectListManager.Instance.selectedProjectRefference);
        StartCoroutine(ProjectLoading());
    }

    public void StartRunning() {

    }

    public void Unload() {

    }

    public IEnumerator ProjectLoading() {
        UImanager.Instance.ShowUI(UIType.LoadingScreen);

        var loadTask = ProjectManager.Instance.LoadProjectAsync();
        yield return new WaitUntil(() => loadTask.IsCompleted);

        if (loadTask.Result) {
            UImanager.Instance.HideUI(UIType.LoadingScreen);
        } else {
            Debug.LogError("Failed to load the project.");
        }
    }

}
