using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class InitializerBasic : MonoBehaviour, Iinitializer {
    /// <summary>
    /// Basic initializer that loads utilities scene used for tester scenes
    /// </summary>

    void Start() {
        Initialize();
    }

    public void Initialize() {
        StartCoroutine(LoadUtilities());
    }

    public void StartRunning() {

    }

    public void Unload() {

    }

    void NotifyInitializationListeners() {
        var listeners = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                    .OfType<IInitializationListener>();

        foreach (IInitializationListener listener in listeners) {
            listener.OnSceneInitialized();
        }
    }

    IEnumerator LoadUtilities() {
        AsyncOperation loading = SceneManager.LoadSceneAsync("Utilities", LoadSceneMode.Additive);
        while (!loading.isDone) {
            yield return null;
        }

        NotifyInitializationListeners();
    }
}
