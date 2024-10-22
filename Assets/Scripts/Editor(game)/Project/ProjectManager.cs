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
        WebCommunicationManager.Instance.StartUpload(serializedProject, project.ProjectName);
    }


    public async Task<bool> LoadProjectAsync() {
        var tcs = new TaskCompletionSource<bool>();

        // Start downloading data from the server
        /*     WebCommunicationManager.Instance.StartDataDownload(project.ProjectName, async (successBool, data) => {
                 if (data != null) {
                     bool success = await project.DeserializeProjectAsync(data); // Await deserialization
                     tcs.SetResult(success); // Return true if the whole process succeeded
                 } else {
                     tcs.SetResult(false); // Task failed
                 }
             });*/
        WebCommunicationManager.Instance.StartDataDownload(project.ProjectName, async (data) => {
            if (data != null) {
                bool success = await project.DeserializeProjectAsync(data); // Await deserialization
                tcs.SetResult(success); // Return true if the whole process succeeded
            } else {
                tcs.SetResult(false); // Task failed
            }
        });

        return await tcs.Task; // Await the completion of the task
    }

    public void OpenProject(ProjectWebRefference projectWebReff) {
        project.OpenProject(projectWebReff);
    }

    public void CloseProject() {
        project.CloseProject();
    }

}
