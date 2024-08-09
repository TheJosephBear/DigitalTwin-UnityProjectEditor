using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Initializer : MonoBehaviour {
    void Awake() {
        StartCoroutine(ShowLogin());
    }
    IEnumerator ShowLogin() {
        AsyncOperation loading = SceneManager.LoadSceneAsync("Utilities", LoadSceneMode.Additive);
        while (!loading.isDone) {
            yield return null;
        }
        UImanager.Instance.ShowUI(UIType.Login);
    }
}
