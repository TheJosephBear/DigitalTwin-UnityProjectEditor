using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectSaver : Singleton<ProjectSaver> {

    public Project currentProject;

    public void SaveProject() {
        // Upload all models
        AssetManager.Instance.UploadModelsToWeb();
        // project serialization
        string serializedProject = currentProject.SerializeProject();
        //upload
        WebCommunicationManager.Instance.StartUpload(serializedProject);
    }

    public void LoadProject() {
        // Download data from server
        WebCommunicationManager.Instance.StartDownload((data) => {
            if (data != null) {
                currentProject.DeserializeProject(data);
            }
        });
    }

    

}
