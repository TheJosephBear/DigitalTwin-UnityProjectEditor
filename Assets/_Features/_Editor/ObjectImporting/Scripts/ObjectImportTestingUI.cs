using Dummiesman;
using FrostweepGames.Plugins.WebGLFileBrowser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TriLibCore;
using UnityEngine;

public class ObjectImportTestingUI : MonoBehaviour {

    public string ProjectToOpen = "ragh";

    void Start() {
        print(FileLoadingManager.Instance.GetAllAllowedExtensionsString());

        // OpenProject(ProjectToOpen);
    }

    public static List<string> GetAllSupportedExtensions() {
        // Returns a list of supported extensions (e.g., "fbx", "obj", "gltf", "glb", "stl", etc.)
        return new List<string>(Readers.Extensions);
    }

    void OpenProject(string projectName) {
        StartCoroutine(ProjectManager.Instance.DownloadProjectData(projectName, (list, success) => {
            
        }));
    }

    #region OnClicks

    public void OnUpload() {
        FileBrowserManager.Instance.ShowLoadDialog(OnFileSelected);
    }

    public void OnUploadWebGL() {
        // This function is used only in editor/pc build and makes the user select the whole folder instead of the singular files
        //  FileBrowserManager.Instance.ShowLoadDialogDebugMultiFile(OnFolderSelected, "obj, mtl, png, jpg", true);

        ModelUploadManager.Instance.AskForModel((createdAsset) => {
            // :-)
        });
    }

    public void OnUploadToWeb() {
        ProjectManager.Instance.SaveProject(SerializeProject());
    }

    public void OnDownload() {
        DeserializeProject(ProjectManager.Instance.SelectedProject);
    }

    #endregion

    void OnFileSelected(FrostweepGames.Plugins.WebGLFileBrowser.File[] files) {
        FileLoadingManager.Instance.UploadFromPC(files[0].fileInfo.path, "something");
    }

    void OnFolderSelected(FrostweepGames.Plugins.WebGLFileBrowser.File[] files) {
        AssetManager.Instance.CreateNewAssetFromFiles(files);
    }

    public SerializableProject SerializeProject() {
        Project OpenedProject = ProjectManager.Instance.SelectedProject;

        SerializableProject serializableProject = new SerializableProject {
            projectId = OpenedProject.ProjectID,
            projectName = OpenedProject.ProjectName,
            serializableModelAssets = AssetManager.Instance.SerializeAssetList(),
        };
        return serializableProject;
    }

    public void DeserializeProject(Project project) {
        StartCoroutine(DeserializeCoroutine(project));
    }

    IEnumerator DeserializeCoroutine(Project project) {
        SerializableProject serializedProject = project.SerializableProject;

        // Wait for asset manager
        bool isAssetDeserializationComplete = false;
        AssetManager.Instance.DeserializeAssetList(serializedProject.serializableModelAssets, () => {
            isAssetDeserializationComplete = true;
        });
        yield return new WaitUntil(() => isAssetDeserializationComplete);
    }

}
