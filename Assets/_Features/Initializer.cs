using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Initializer : MonoBehaviour {
    /// <summary>
    /// This is the first script ever called. Sets up utilities and loads the first scene
    /// </summary>

    public SceneType firstSceneToLoad;


    void Awake() {
        StartCoroutine(ShowLogin());
    }
    IEnumerator ShowLogin() {
        AsyncOperation loading = SceneManager.LoadSceneAsync("Utilities", LoadSceneMode.Additive);
        while (!loading.isDone) {
            yield return null;
        }
        UImanager.Instance.ShowUI(UIType.LoadingScreen);
        var loadTask = SceneLoadingManager.Instance.LoadSceneAsync(firstSceneToLoad, 0f);
        yield return new WaitUntil(() => loadTask.IsCompleted);
        if (loadTask.Result) {
            UImanager.Instance.HideUI(UIType.LoadingScreen);
        } else {
            Debug.LogError("Failed to load FIRST scene.");
        }
    }
}
