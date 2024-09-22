using System;
using System.Collections;
using System.Collections.Generic;
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


    public void LoadProject() {
        // Download data from server
        WebCommunicationManager.Instance.StartDataDownload(project.ProjectName, (data) => {
            if (data != null) {
                project.DeserializeProject(data);
            }
        });
    }




}
