using Dummiesman;
using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class ObjectImportTestingUI : MonoBehaviour {

    public string ProjectToOpen = "ragh";

    void Awake() {
        OpenProject(ProjectToOpen);
    }

    void OpenProject(string projectName) {
        StartCoroutine(ProjectManager.Instance.DownloadProjectData(projectName, (list) => {
            
        }));
    }

    #region OnClicks

    public void OnUpload() {
        FileBrowserManager.Instance.ShowLoadDialog(OnFileSelected);
    }

    public void OnUploadToWeb() {
        ProjectManager.Instance.SaveProject(SerializeProject());
    }

    public void OnDownload() {
        DeserializeProject(ProjectManager.Instance.SelectedProject);
    }

    #endregion

    void OnFileSelected(FrostweepGames.Plugins.WebGLFileBrowser.File[] files) {
        AssetManager.Instance.CreateNewAssetFromFile(files[0]);
        //     FileLoadingManager.Instance.UploadFromPC(files[0].fileInfo.path);

    }

    public SerializableProject SerializeProject() {
        Project OpenedProject = ProjectManager.Instance.SelectedProject;

        SerializableProject serializableProject = new SerializableProject {
            projectId = OpenedProject.ProjectID,
            projectName = OpenedProject.ProjectName,
            serializedModelAssets = AssetManager.Instance.SerializeAssetList(),
        };
        return serializableProject;
    }

    public void DeserializeProject(Project project) {
        StartCoroutine(DeserializeCoroutine(project));
    }

    IEnumerator DeserializeCoroutine(Project project) {
        SerializableProject serializedProject = project.SerializedProject;

        // Wait for asset manager
        bool isAssetDeserializationComplete = false;
        AssetManager.Instance.DeserializeAssetList(serializedProject.serializedModelAssets, () => {
            isAssetDeserializationComplete = true;
        });
        yield return new WaitUntil(() => isAssetDeserializationComplete);
    }

}