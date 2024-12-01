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
                bool deserializeSuccess = await project.DeserializeProjectAsync(data);
                tcs.SetResult(deserializeSuccess);
            } else {
                tcs.SetResult(false);
            }
        });
        return await tcs.Task;
    }

    public void OpenProject(ProjectWebRefference projectWebReff) {
        // Toto naète akorát potøebná data, ne modely atd.
        project.OpenProject(projectWebReff);
    }

    public void CloseProject() {
        project.CloseProject();
    }

}
