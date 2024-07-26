using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadingManager : MonoBehaviour {
    public static SceneLoadingManager Instance { get; private set; }

    private List<SceneField> loadedScenes = new List<SceneField>();
    private List<SceneField> loadedGameplayScenes = new List<SceneField>();

    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    public async Task<bool> LoadSceneAsync(SceneType sceneType, float loadingScreenLength, bool addToGameplayScenes = false) {
        SceneField scene = SceneList.Instance.GetScene(sceneType);
        var tcs = new TaskCompletionSource<bool>();
        StartCoroutine(LoadSceneAsyncC(scene, tcs, loadingScreenLength, addToGameplayScenes));
        return await tcs.Task;
    }

    public void LoadScene(SceneType sceneType) {
        SceneField scene = SceneList.Instance.GetScene(sceneType);
        SceneManager.LoadScene(scene, LoadSceneMode.Additive);
        loadedScenes.Add(scene);
    }

    public async Task<bool> UnLoadSceneAsync(SceneType sceneType) {
        SceneField scene = SceneList.Instance.GetScene(sceneType);
        var tcs = new TaskCompletionSource<bool>();
        StartCoroutine(UnloadSceneAsyncC(scene, tcs));
        return await tcs.Task;
    }

    public async Task UnLoadAllGameplayScenes() {
        List<Task<bool>> unloadTasks = new List<Task<bool>>();
        foreach (SceneField scene in loadedGameplayScenes) {
            if (Enum.TryParse(scene.SceneName, out SceneType sceneType)) {
                unloadTasks.Add(UnLoadSceneAsync(sceneType));
            } else {
                Debug.LogWarning($"Invalid scene name: {scene.SceneName}");
            }
        }
        loadedGameplayScenes.Clear();
        await Task.WhenAll(unloadTasks);
    }

    public void InstantiateObjectInScene(GameObject gameObject, Vector3 position, SceneType scene) {
        GameObject go = Instantiate(gameObject, position, Quaternion.identity);
        SceneManager.MoveGameObjectToScene(go, SceneManager.GetSceneByName(scene.ToString()));
    }

    IEnumerator LoadSceneAsyncC(SceneField scene, TaskCompletionSource<bool> tcs, float loadingScreenLength, bool addToGameplayScenes) {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        while (!asyncLoad.isDone) {
            yield return null;
        }
        loadedScenes.Add(scene);
        if (addToGameplayScenes) loadedGameplayScenes.Add(scene);
        yield return CallSceneInitializerC(scene, loadingScreenLength);
        tcs.SetResult(true);
    }

    IEnumerator UnloadSceneAsyncC(SceneField scene, TaskCompletionSource<bool> tcs) {
        Iinitializer initializer = FindInitializerInScene(scene);
        initializer?.Unload();
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(scene);
        while (!asyncUnload.isDone) {
            yield return null;
        }
        if (loadedScenes.Contains(scene)) {
            loadedScenes.Remove(scene);
        }
        tcs.SetResult(true);
    }

    IEnumerator CallSceneInitializerC(SceneField scene, float waitAfterInitialization) {
        Iinitializer initializer = FindInitializerInScene(scene);
        initializer?.Initialize();

        yield return new WaitForSeconds(waitAfterInitialization);

        initializer?.StartRunning();
    }

    Iinitializer FindInitializerInScene(SceneField scene) {
        GameObject[] rootObjects = SceneManager.GetSceneByName(scene.SceneName).GetRootGameObjects();
        foreach (GameObject obj in rootObjects) {
            Iinitializer initializer = obj.GetComponentInChildren<Iinitializer>();
            if (initializer != null) {
                return initializer;
            }
        }
        return null;
    }
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
