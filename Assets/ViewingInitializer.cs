using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ViewingInitializer : MonoBehaviour {

    private void Awake() {
        string projectName = GetUrlParameter("projectName");
        StartCoroutine(InitializeViewer(projectName));
    }

    IEnumerator InitializeViewer(string projectName) {
        bool downloadFinished = false;

        UIManager.Instance.ShowUI(UIType.LoadingScreen);
        StartCoroutine(ProjectManager.Instance.DownloadProjectData(projectName, (list) => {
            downloadFinished = true;
        }));

        while (!downloadFinished)
            yield return null;

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
        EditorManager.Instance.MapManager.Deserialize(serializedProject.serializedMap);
        EditorManager.Instance.ViewManager.Deserialize(serializedProject.serializedViewPointManager);
        EditorManager.Instance.GeoMapManager.DeserializeManager(serializedProject.serializedGeoMap);

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
