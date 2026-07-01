using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadingManager : Singleton<SceneLoadingManager> {

    private List<SceneField> _loadedScenes = new List<SceneField>();

    protected override void Awake() {
        base.Awake();
    }

    /// <summary>
    /// Sets a loaded scene to be the "Active Scene".
    /// The active scene's environment settings (like lighting and skybox) are prioritized.
    /// </summary>
    /// <param name="sceneType">The SceneType enum representing the scene to activate.</param>
    public void SetActiveScene(SceneType sceneType) {
        // Retrieves the SceneField data from the global list
        SceneField sceneField = SceneList.Instance.GetScene(sceneType);
        // Gets the actual UnityEngine.Scene object by its name
        Scene scene = SceneManager.GetSceneByName(sceneField.SceneName);

        if (scene.isLoaded) {
            SceneManager.SetActiveScene(scene);
        } else {
            Debug.LogWarning($"Scene {scene.name} is not loaded and cannot be set as active.");
        }
    }

    public void SetActiveScene(SceneField sceneField) {
        // Gets the actual UnityEngine.Scene object by its name
        Scene scene = SceneManager.GetSceneByName(sceneField.SceneName);

        if (scene.isLoaded) {
            SceneManager.SetActiveScene(scene);
        } else {
            Debug.LogWarning($"Scene {scene.name} is not loaded and cannot be set as active.");
        }
    }

    /// <summary>
    /// Retrieves the SceneType enumeration value that corresponds to the current active scene in Unity.
    /// </summary>
    /// <returns>The SceneType enum of the active scene. Returns SceneType.Editing if no match is found.</returns>
    public SceneType GetActiveScene() {
        Scene activeScene = SceneManager.GetActiveScene();

        foreach (SceneType sceneType in Enum.GetValues(typeof(SceneType))) {
            SceneField sceneField = SceneList.Instance.GetScene(sceneType);
            if (sceneField.SceneName == activeScene.name) {
                return sceneType;
            }
        }

        Debug.LogWarning($"Active scene '{activeScene.name}' does not match any known SceneType.");
        return SceneType.Editing;
    }

    /// <summary>
    /// Asynchronously loads a scene additively, waits for completion, runs the Iinitializer, and waits for an optional delay to call the StartRunning function of the IInitializer.
    /// Also sets the loaded scene as active scene.
    /// </summary>
    /// <param name="sceneType">The SceneType of the scene to load.</param>
    /// <param name="loadingScreenLength">The minimum time (in seconds) to wait after the scene is initialized before calling IInitializers StartRunning function and returning (e.g., to keep a loading screen visible).</param>
    /// <returns>A Task that completes with true if the scene was loaded and initialized successfully, otherwise false (though error logging handles failure).</returns>
    public async Task<bool> LoadSceneAsync(SceneType sceneType, float loadingScreenLength = 0f) {
        // Gets the scene data
        SceneField scene = SceneList.Instance.GetScene(sceneType);
        // TaskCompletionSource bridges the Coroutine (Unity's async) with the C# Task system
        var tcs = new TaskCompletionSource<bool>();
        StartCoroutine(LoadSceneAsyncC(scene, tcs, loadingScreenLength));
        return await tcs.Task;
    }

    /// <summary>
    /// Synchronously loads a scene additively.
    /// Note: Does not run the scene's Iinitializer. For proper initialization, use LoadSceneAsync.
    /// </summary>
    /// <param name="sceneType">The SceneType of the scene to load.</param>
    public void LoadScene(SceneType sceneType) {
        SceneField scene = SceneList.Instance.GetScene(sceneType);
        SceneManager.LoadScene(scene, LoadSceneMode.Additive);
        _loadedScenes.Add(scene);
    }

    /// <summary>
    /// Asynchronously unloads a scene, first calling the Iinitializer's Unload method.
    /// </summary>
    /// <param name="sceneType">The SceneType of the scene to unload.</param>
    /// <returns>A Task that completes with true when the scene is fully unloaded.</returns>
    public async Task<bool> UnLoadSceneAsync(SceneType sceneType) {
        SceneField scene = SceneList.Instance.GetScene(sceneType);
        var tcs = new TaskCompletionSource<bool>();
        StartCoroutine(UnloadSceneAsyncC(scene, tcs));
        return await tcs.Task;
    }

    public GameObject InstantiateObjectInScene(GameObject gameObject, Vector3 position, Quaternion rotation) {
        GameObject go = Instantiate(gameObject, position, rotation);
        SceneType scene = GetActiveScene();
        SceneManager.MoveGameObjectToScene(go, SceneManager.GetSceneByName(scene.ToString()));
        return go;
    }

    public GameObject InstantiateObjectInScene(GameObject gameObject, Vector3 position, Quaternion rotation, SceneType scene) {
        GameObject go = Instantiate(gameObject, position, rotation);
        SceneManager.MoveGameObjectToScene(go, SceneManager.GetSceneByName(scene.ToString()));
        return go;
    }

    /// <summary>
    /// Instantiates a GameObject at a specific position and moves it into a specified scene.
    /// </summary>
    /// <param name="gameObject">The prefab or GameObject to instantiate.</param>
    /// <param name="position">The world position for the new object.</param>
    /// <param name="scene">The SceneType defining the target scene.</param>
    /// <returns>The newly instantiated GameObject.</returns>
    public GameObject InstantiateObjectInScene(GameObject gameObject, Vector3 position, SceneType scene) {
        GameObject go = Instantiate(gameObject, position, Quaternion.identity);
        SceneManager.MoveGameObjectToScene(go, SceneManager.GetSceneByName(scene.ToString()));
        return go;
    }

    /// <summary>
    /// Instantiates a GameObject at (0, 0, 0) and moves it into a specified scene.
    /// </summary>
    /// <param name="gameObject">The prefab or GameObject to instantiate.</param>
    /// <param name="scene">The SceneType defining the target scene.</param>
    /// <returns>The newly instantiated GameObject.</returns>
    public GameObject InstantiateObjectInScene(GameObject gameObject, SceneType scene) {
        GameObject go = Instantiate(gameObject, new Vector3(0, 0, 0), Quaternion.identity);
        SceneManager.MoveGameObjectToScene(go, SceneManager.GetSceneByName(scene.ToString()));
        return go;
    }

    /// <summary>
    /// Instantiates a GameObject at (0, 0, 0) and moves it into the current Active Scene.
    /// </summary>
    /// <param name="gameObject">The prefab or GameObject to instantiate.</param>
    /// <returns>The newly instantiated GameObject.</returns>
    public GameObject InstantiateObjectInScene(GameObject gameObject) {
        GameObject go = Instantiate(gameObject, new Vector3(0, 0, 0), Quaternion.identity);
        SceneType scene = GetActiveScene();
        SceneManager.MoveGameObjectToScene(go, SceneManager.GetSceneByName(scene.ToString()));
        return go;
    }

    /// <summary>
    /// Instantiates a GameObject at a specific position and moves it into the current Active Scene.
    /// </summary>
    /// <param name="gameObject">The prefab or GameObject to instantiate.</param>
    /// <param name="position">The world position for the new object.</param>
    /// <returns>The newly instantiated GameObject.</returns>
    public GameObject InstantiateObjectInScene(GameObject gameObject, Vector3 position) {
        GameObject go = Instantiate(gameObject, position, Quaternion.identity);
        SceneType scene = GetActiveScene();
        SceneManager.MoveGameObjectToScene(go, SceneManager.GetSceneByName(scene.ToString()));
        return go;
    }

    void CallSceneInitializer(SceneField scene) {
        Iinitializer initializer = FindInitializerInScene(scene);
        initializer?.Initialize(); // Run initial setup tasks
    }

    #region Coroutine implementations

    IEnumerator LoadSceneAsyncC(SceneField scene, TaskCompletionSource<bool> tcs, float loadingScreenLength) {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        while (!asyncLoad.isDone) {
            yield return null;
        }
        _loadedScenes.Add(scene);
        SetActiveScene(scene);
        CallSceneInitializer(scene);
        tcs.SetResult(true);
    }

    IEnumerator UnloadSceneAsyncC(SceneField scene, TaskCompletionSource<bool> tcs) {
        Iinitializer initializer = FindInitializerInScene(scene);
        initializer?.Unload(); // Call the initializer's cleanup method
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(scene);
        while (!asyncUnload.isDone) {
            yield return null;
        }
        if (_loadedScenes.Contains(scene)) {
            _loadedScenes.Remove(scene);
        }
        tcs.SetResult(true);
    }

    Iinitializer FindInitializerInScene(SceneField scene) {
        // Only searches root objects of the target scene
        GameObject[] rootObjects = SceneManager.GetSceneByName(scene.SceneName).GetRootGameObjects();
        foreach (GameObject obj in rootObjects) {
            // Searches the root object and its children for the Iinitializer component
            Iinitializer initializer = obj.GetComponentInChildren<Iinitializer>();
            if (initializer != null) {
                return initializer;
            }
        }
        return null;
    }

    #endregion

}

/**
 * how to load scene async externally
 * 
    private IEnumerator InitializeGameC() {
        // Wait until the scene loading task is complete
        var loadTask = sceneLoader.LoadMainMenuAsync(SceneType.sum, 2f);
        yield return new WaitUntil(() => loadTask.IsCompleted);
        if (loadTask.Result) {
            // after its loaded do something
        } else {
            Debug.LogError("Failed to load scene.");
        }
    }
*/
