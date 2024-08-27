using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectSaver : Singleton<ProjectSaver> {

    public Project project;

    public void SaveProject() {
        // Upload all models
        AssetManager.Instance.UploadModelsToWeb();
        // project serialization
        string serializedProject = project.SerializeProject();
        //upload
        WebCommunicationManager.Instance.StartUpload(serializedProject);
    }

    public void LoadProject() {
        // Download data from server
        WebCommunicationManager.Instance.StartDataDownload((data) => {
            if (data != null) {
                project.DeserializeProject(data);
            }
        });
    }



}
