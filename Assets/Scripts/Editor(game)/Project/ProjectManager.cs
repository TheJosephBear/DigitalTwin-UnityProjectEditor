using System;
using System.Collections;
using System.Collections.Generic;
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

    public void LoadProject() {
        WebCommunicationManager.Instance.StartDataDownload(project.ProjectName, (data) => {
            if (data != null) {
                project.DeserializeProject(data);
            }
        });
    }

    public void OpenProject(ProjectWebRefference projectWebReff) {
        project.OpenProject(projectWebReff);
    }

    public void CloseProject() {
        project.CloseProject();
    }

}
