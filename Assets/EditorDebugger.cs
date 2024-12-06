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
        UImanager.Instance.ShowUI(UIType.EditorHUD);
    }

}
