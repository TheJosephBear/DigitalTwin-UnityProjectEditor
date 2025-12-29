using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Initializer : MonoBehaviour {
    /// <summary>
    /// This is the first script ever called. 
    /// Sets up utilities and loads the first scene
    /// </summary>

    public SceneType firstSceneToLoadEditor;
    public SceneType firstSceneToLoadViewer;


    void Awake() {
        InitializeCorrectAppMode();
    }

    void InitializeCorrectAppMode() {
        string viewing = GetUrlParameter("viewing");
        print(viewing);
        if(viewing == "False") {
            print("entering editor mode");
            EnterEditorMode();
        } else if (viewing == "True") {
            print("entering viewer mode");
            EnterViewerMode();
        } else {
            Debug.LogError("Viewing parameter invalid or empty: "+viewing);
        }

        string projectName = GetUrlParameter("projectName");
    }

    void EnterEditorMode() {
        StartCoroutine(LoadUitlitiesAndEnterFirstScene(firstSceneToLoadEditor));
    }

    void EnterViewerMode() {
        StartCoroutine(LoadUitlitiesAndEnterFirstScene(firstSceneToLoadViewer));
    }

    IEnumerator LoadUitlitiesAndEnterFirstScene(SceneType firstScene) {
        AsyncOperation loading = SceneManager.LoadSceneAsync("Utilities", LoadSceneMode.Additive);
        while (!loading.isDone) {
            yield return null;
        }
        UImanager.Instance.ShowUI(UIType.LoadingScreen);
        var loadTask = SceneLoadingManager.Instance.LoadSceneAsync(firstScene, 0f);
        yield return new WaitUntil(() => loadTask.IsCompleted);
        if (loadTask.Result) {
            UImanager.Instance.HideUI(UIType.LoadingScreen);
        } else {
            Debug.LogError("Failed to load FIRST scene.");
        }
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
