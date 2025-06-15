using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ProjectManager : Singleton<ProjectManager> {

    public Project project;

    protected override void Awake() {
        base.Awake();
        project = GetComponent<Project>();
    }

    public void SaveProject() {
        AssetManager.Instance.UploadModelsToWeb();
        string serializedProject = project.SerializeProject();
        print("serializedProjectIs: "+serializedProject);
        ServerCommunicationManager.Instance.StartUpload(serializedProject, project.ProjectName);
    }

    public async Task<bool> LoadProjectAsync() {
        var tcs = new TaskCompletionSource<bool>();
        ServerCommunicationManager.Instance.StartDataDownload(project.ProjectName, async (success, data) => {
            if (data != null) {
                print("the data i got is: " + data);
                bool deserializeSuccess = await project.DeserializeProjectAsync(data);
                tcs.SetResult(deserializeSuccess);
            } else {
                tcs.SetResult(false);
            }
        });
        return await tcs.Task;
    }

    public void OpenProject(ProjectWebRefference projectWebReff) {
        project.OpenProject(projectWebReff);
        StartCoroutine(ProjectLoading());
    }

    public void CloseProject() {
        StartCoroutine(CloseProjectCoroutine());
    }

    public IEnumerator ProjectLoading() {
        UImanager.Instance.ShowUI(UIType.LoadingScreen);

        var loadTask = LoadProjectAsync();
        yield return new WaitUntil(() => loadTask.IsCompleted);

        if (loadTask.Result) {
            UImanager.Instance.HideUI(UIType.LoadingScreen);
        } else {
         //   PopUp.Instance.ShowPopUpWindow("Failed to load the project.");  // when there is nothing to load, the pop up message is shown regarldess, NOT NEEDED
        }
    }

    IEnumerator CloseProjectCoroutine() {
        UImanager.Instance.ShowUI(UIType.LoadingScreen);
        var loadTask = SceneLoadingManager.Instance.LoadSceneAsync(SceneType.ProjectList);
        while (!loadTask.IsCompleted) {
            yield return null;
        }
        project.CloseProject();
        var unloadTask = SceneLoadingManager.Instance.UnLoadSceneAsync(SceneType.Editing);
        while (!unloadTask.IsCompleted) {
            yield return null;
        }
        UImanager.Instance.HideUI(UIType.LoadingScreen);
    }
    
}
