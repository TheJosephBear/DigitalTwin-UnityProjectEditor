using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ViewTester : MonoBehaviour {

    void Start() {
        StartCoroutine(LoadCouroutine());
    }

    IEnumerator LoadCouroutine() {
        var load = SceneManager.LoadSceneAsync("Utilities", LoadSceneMode.Additive);
        yield return new WaitUntil(() => load.isDone);
        yield return new WaitForSeconds(0.2f);
        UIManager.Instance.ShowUI(UIType.EditorHUD);
        EditorManager.Instance.ChangeState(ProjectState.Freecam);
    }
}
