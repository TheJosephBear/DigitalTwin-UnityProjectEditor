using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EditorDebugger : MonoBehaviour {
    
    void Start() {
        StartCoroutine(LoadCouroutine());
    }

    IEnumerator LoadCouroutine() {
        var load = SceneManager.LoadSceneAsync("Utilities", LoadSceneMode.Additive);
        yield return new WaitUntil(()=> load.isDone);
        yield return new WaitForSeconds(1f);
        EditorInitializer ei = FindAnyObjectByType<EditorInitializer>();
        if(ei != null) {
            FindAnyObjectByType<EditorInitializer>().Initialize();
            FindAnyObjectByType<EditorInitializer>().StartRunning();
        }
    }

}
