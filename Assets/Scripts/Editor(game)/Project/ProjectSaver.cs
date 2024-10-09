using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ProjectSaver : Singleton<ProjectSaver> {

    public Project project;

    public void SaveProject() {
        // Upload all models
        AssetManager.Instance.UploadModelsToWeb();

        // Project serialization
        string serializedProject = project.SerializeProject();

        // Use the correct endpoint to upload data
        WebCommunicationManager.Instance.StartUpload(serializedProject, project.ProjectName);
    }

    public async Task<bool> LoadProjectAsync() {
        var tcs = new TaskCompletionSource<bool>();

        // Start downloading data from the server
        WebCommunicationManager.Instance.StartDataDownload(project.ProjectName, async (data) => {
            if (data != null) {
                bool success = await Project.Instance.DeserializeProjectAsync(data); // Await deserialization
                tcs.SetResult(success); // Return true if the whole process succeeded
            } else {
                tcs.SetResult(false); // Task failed
            }
        });

        return await tcs.Task; // Await the completion of the task
    }


}
