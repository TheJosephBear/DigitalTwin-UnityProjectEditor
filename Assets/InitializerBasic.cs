using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitializerBasic : MonoBehaviour, Iinitializer {
    /// <summary>
    /// Basic initializer that loads utilities scene
    /// </summary>
    public void Initialize() {
        StartCoroutine(LoadUitlities());
    }

    public void StartRunning() {

    }

    public void Unload() {

    }

    IEnumerator LoadUitlities() {
        UImanager.Instance.ShowUI(UIType.LoadingScreen);
        AsyncOperation loading = SceneManager.LoadSceneAsync("Utilities", LoadSceneMode.Additive);
        while (!loading.isDone) {
            yield return null;
        }
        
        UImanager.Instance.HideUI(UIType.LoadingScreen);
    }
}
