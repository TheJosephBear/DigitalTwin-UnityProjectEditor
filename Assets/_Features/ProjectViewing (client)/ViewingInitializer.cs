using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ViewingInitializer : MonoBehaviour, Iinitializer {

    public string _projectName;
    public bool InEditorDebugging = false;

    public void Initialize() {
        if (!InEditorDebugging) {
            _projectName = GetUrlParameter("projectName");
        }

        SceneLoadingManager.Instance.SetActiveScene(SceneType.Viewing);
        InitializeViewer();
    }

    public void StartRunning() {

    }

    public void Unload() {

    }

    public void InitializeViewer() {
        StartCoroutine(InitializeViewerCoroutine(_projectName));
    }

    IEnumerator InitializeViewerCoroutine(string projectName) {
        bool downloadFinished = false;
        bool downloadSuccess = false;

        UIManager.Instance.ShowUI(UIType.LoadingScreen);

        StartCoroutine(ProjectManager.Instance.DownloadProjectData(projectName, (list, success) => {
            downloadSuccess = success;
            downloadFinished = true;
        }));

        while (!downloadFinished)
            yield return null;

        if (!downloadSuccess) {
            MessageDisplayManager.Instance.ShowMessage("Project download failed!");
            UIManager.Instance.HideUI(UIType.LoadingScreen);
            yield break; // Stops coroutine safely
        }
            
        UIManager.Instance.ShowUI(UIType.ViewerHUD);
        DeserializeProject(ProjectManager.Instance.SelectedProject);

        UIManager.Instance.HideUI(UIType.LoadingScreen);
    }


    public void DeserializeProject(Project project) {
        StartCoroutine(DeserializeCoroutine(project));
    }

    IEnumerator DeserializeCoroutine(Project project) {
        UIManager.Instance.ShowUI(UIType.LoadingScreen);

        SerializableProject serializedProject = project.SerializedProject;

        // Wait for asset manager
        bool isAssetDeserializationComplete = false;
        AssetManager.Instance.DeserializeAssetList(serializedProject.serializedModelAssets, () => {
            isAssetDeserializationComplete = true;
        });
        yield return new WaitUntil(() => isAssetDeserializationComplete);

        // Deserialize everything else
        ViewingManager.Instance.MapManager.Deserialize(serializedProject.serializedMap);
        ViewingManager.Instance.ViewManager.Deserialize(serializedProject.serializedViewPointManager);
        ViewingManager.Instance.GeoMapManager.DeserializeManager(serializedProject.serializedGeoMap);

        UIManager.Instance.HideUI(UIType.LoadingScreen);
    }

    public static string GetUrlParameter(string parameterName) {
        // Basic checks
        if (string.IsNullOrEmpty(parameterName))
            return "";

#if UNITY_WEBGL && !UNITY_EDITOR
        string url = Application.absoluteURL;

        if (string.IsNullOrEmpty(url))
            return "";

        int questionMarkIndex = url.IndexOf('?');
        if (questionMarkIndex == -1)
            return "";

        string query = url.Substring(questionMarkIndex + 1);
        string[] pairs = query.Split('&');

        foreach (string pair in pairs)
        {
            if (string.IsNullOrEmpty(pair))
                continue;

            string[] kv = pair.Split('=');
            if (kv.Length != 2)
                continue;

            if (kv[0] == parameterName)
                return Uri.UnescapeDataString(kv[1]);
        }
#endif

        return "";
    }
}
